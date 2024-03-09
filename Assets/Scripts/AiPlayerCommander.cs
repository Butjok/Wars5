using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;

// ReSharper disable LoopCanBePartlyConvertedToQuery

public class AiPlayerCommander : MonoBehaviour {

    public enum PriorityPreference {
        Min,
        Max
    }

    public GradientMap priorityGradientMap = new();

    public Vector2Int selectPosition;
    public Vector2Int movePosition;
    [Command] public float textSize = 14;
    public HashSet<Vector2Int> gatheringPoints = new();

    public Game game;
    public Level Level => game.stateMachine.TryFind<LevelSessionState>()?.level ?? game.stateMachine.TryFind<LevelEditorSessionState>().level;

    [Command]
    public void ClearGatheringPoints() {
        gatheringPoints.Clear();
    }
    [Command]
    public void TryAddGatheringPoint() {
        if (Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int position))
            gatheringPoints.Add(position);
    }

    [Command]
    public bool playForHuman = true;
    [Command]
    public bool waitForKeyPress = true;
    [Command]
    public KeyCode nextStepKeyCode = KeyCode.RightArrow;

    //
    // potential unit action priority formula handling
    //

    public PriorityPreference priorityPreference = PriorityPreference.Max;

    public bool TryFindBestMove(out PotentialUnitAction bestPotentialUnitAction) {
        var actions = new List<PotentialUnitAction>();
        foreach (var unit in Level.units.Values.Where(unit => unit.Player == Level.CurrentPlayer && !unit.Moved))
            actions.AddRange(EnumeratePotentialUnitActions(unit));

        if (actions.Count == 0) {
            bestPotentialUnitAction = null;
            return false;
        }

        PrioritizePotentialUnitActions(actions);
        bestPotentialUnitAction = (priorityPreference == PriorityPreference.Max
                ? actions.OrderByDescending(a => a.priority)
                : actions.OrderBy(a => a.priority))
            .ThenBy(a => a.path.Count) // prefer shortest immediate path
            .ThenBy(a => a.restPath.Count) // prefer shortest full path
            .FirstOrDefault();
        return bestPotentialUnitAction != null;
    }

    public enum NextTask {
        None,
        SelectUnit,
        SelectPath,
        SelectAction
    }

    public PotentialUnitAction selectedAction;

    public void IssueCommandsForSelectionState() {
        Assert.IsTrue(game.stateMachine.IsInState<SelectionState>());

        var foundMove = TryFindBestMove(out selectedAction);

        // if unit cannot move the entire path change the action type to stay
        // also if an artillery unit is trying to attack somebody but it moves first - change to stay as well
        if (selectedAction != null) {
            if (selectedAction.restPath[^1] != selectedAction.path[^1] ||
                selectedAction.path.Count > 1 && IsArtillery(selectedAction.unit) ||
                selectedAction.type == UnitActionType.Gather)

                if (selectedAction.type != UnitActionType.Join)
                    selectedAction.type = UnitActionType.Stay;
        }

        if (!foundMove)
            game.EnqueueCommand(SelectionState.Command.EndTurn);
        else
            game.EnqueueCommand(SelectionState.Command.Select, selectedAction.unit.NonNullPosition);
    }

    public void IssueCommandsForPathSelectionState() {
        Assert.IsTrue(game.stateMachine.IsInState<PathSelectionState>());

        foreach (var position in selectedAction.path)
            game.EnqueueCommand(PathSelectionState.Command.AppendToPath, position);
        game.EnqueueCommand(PathSelectionState.Command.Move);
    }

    public void IssueCommandsForActionSelectionState() {
        var actionSelectionState = game.stateMachine.TryPeek<ActionSelectionState>();
        Assert.IsNotNull(actionSelectionState);

        var actions = actionSelectionState.actions.Where(action => {
            var valid = action.type == selectedAction.type;
            if (valid)
                valid = action.type switch {
                    UnitActionType.Supply => action.targetUnit == selectedAction.targetUnit,
                    UnitActionType.Attack => action.targetUnit == selectedAction.targetUnit && action.weaponName == selectedAction.weaponName,
                    UnitActionType.Drop => action.targetUnit == selectedAction.targetUnit && action.targetPosition == selectedAction.targetPosition,
                    _ => true
                };
            return valid;
        }).ToList();

        Debug.Log($"selected action type: {selectedAction.type}");


        Assert.AreEqual(1, actions.Count);
        game.EnqueueCommand(ActionSelectionState.Command.Execute, actions[0]);
    }

    [Command]
    public void DrawPotentialUnitActions() {
        if (!Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition) || !Level.TryGetUnit(mousePosition, out var unit))
            return;

        StopAllCoroutines();
        var actions = EnumeratePotentialUnitActions(unit).ToList();
        PrioritizePotentialUnitActions(actions);
        StartCoroutine(DrawPotentialUnitActions(actions));
    }

    public PathFinder stayMovesFinder = new();
    public PathFinder joinMovesFinder = new();

    public IEnumerable<PotentialUnitAction> EnumeratePotentialUnitActions(Unit unit) {
        stayMovesFinder.FindStayMoves(unit);

        //
        // get to the closest gathering point
        //

        if (gatheringPoints.Count > 0) {
            var targets = gatheringPoints.Where(position => CanStay(unit, position));
            if (stayMovesFinder.TryFindPath(out var path, out var restPath, targets: targets))
                yield return new PotentialUnitAction {
                    unit = unit,
                    type = UnitActionType.Gather,
                    path = path,
                    restPath = restPath
                };
        }

        //
        // capture buildings
        //

        /*
         * TODO: add capturing through the carrier (APC, TransportHelicopter etc.).
         * - Viktor 9.2.23
         */

        var buildingsToCapture = Level.buildings.Values.Where(building => CanCapture(unit, building) && CanStay(unit, building.position));
        foreach (var building in buildingsToCapture)
            if (stayMovesFinder.TryFindPath(out var path, out var restPath, building.position))
                yield return new PotentialUnitAction {
                    unit = unit,
                    type = UnitActionType.Capture,
                    targetBuilding = building,
                    path = path,
                    restPath = restPath
                };

        //
        // find units to attack
        //

        /*
         * TODO: somehow de-prioritize artillery attacks - atm they are charging forward as mad men.
         * - Viktor 9.2.23
         */

        if (TryGetAttackRange(unit, out var attackRange))

            foreach (var target in Level.units.Values) {
                List<Vector2Int> path = null;
                List<Vector2Int> restPath = null;

                foreach (var weaponName in GetWeaponNames(unit))
                    if (AreEnemies(unit.Player, target.Player) &&
                        TryGetDamage(unit, target, weaponName, out _)) {
                        if (path == null) {
                            var targets = Level.PositionsInRange(target.NonNullPosition, attackRange).Where(position => CanStay(unit, position));
                            if (!stayMovesFinder.TryFindPath(out path, out restPath, targets: targets))
                                break;
                        }

                        yield return new PotentialUnitAction {
                            unit = unit,
                            type = UnitActionType.Attack,
                            targetUnit = target,
                            weaponName = weaponName,
                            path = path,
                            restPath = restPath
                        };
                    }
            }

        //
        // find units to supply
        //

        var allies = Level.units.Values.Where(u => AreAllies(unit.Player, u.Player)).ToList();

        foreach (var ally in allies)
            if (CanSupply(unit, ally)) {
                var targets = Level.PositionsInRange(ally.NonNullPosition, Vector2Int.one).Where(position => CanStay(unit, position));
                if (stayMovesFinder.TryFindPath(out var path, out var restPath, targets: targets))
                    yield return new PotentialUnitAction {
                        unit = unit,
                        type = UnitActionType.Supply,
                        targetUnit = ally,
                        path = path,
                        restPath = restPath
                    };
            }

        //
        // find units to join / get in
        //

        var joinMovesFinderInitialized = false;

        foreach (var ally in allies) {
            var canJoin = CanJoin(unit, ally);
            var canGetIn = CanGetIn(unit, ally);
            if (!canJoin && !canGetIn)
                continue;

            if (!joinMovesFinderInitialized) {
                joinMovesFinderInitialized = true;
                joinMovesFinder.FindMoves(unit);
            }

            if (!joinMovesFinder.TryFindPath(out var path, out var restPath, ally.NonNullPosition))
                continue;

            if ((!Level.TryGetUnit(path[^1], out var other) || other != ally) && !stayMovesFinder.TryFindPath(out path, out restPath, ally.NonNullPosition))
                continue;

            if (canJoin)
                yield return new PotentialUnitAction {
                    unit = unit,
                    type = UnitActionType.Join,
                    targetUnit = ally,
                    path = path,
                    restPath = restPath
                };

            if (canGetIn)
                yield return new PotentialUnitAction {
                    unit = unit,
                    type = UnitActionType.GetIn,
                    targetUnit = ally,
                    path = path,
                    restPath = restPath
                };
        }
    }

    public void PrioritizePotentialUnitActions(IEnumerable<PotentialUnitAction> actions) {
        foreach (var action in actions) {
            float pathLength = action.path.Count;
            float fullPathLength = action.path.Count + action.restPath.Count - 1;
            float pathLengthPercentage = pathLength / fullPathLength;
            float pathCost = PathCost(action.unit, action.path);
            float fullPathCost = PathCost(action.unit, action.FullPath);
            float pathCostPercentage = pathCost / fullPathCost;

            action.priority = 0;

            switch (action.type) {
                case UnitActionType.Attack: {
                    var isValid = TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damagePercentage);
                    Assert.IsTrue(isValid);

                    action.priority = 2 * pathCostPercentage * damagePercentage;
                    if (IsArtillery(action.unit) && action.path.Count > 1)
                        action.priority /= 2;
                    break;
                }

                case UnitActionType.Capture: {
                    float unitCp = Cp(action.unit);
                    float buildingCp = action.targetBuilding.Cp;
                    float capturePercentage = Mathf.Clamp01(unitCp / buildingCp);
                    action.priority = 2 * pathCostPercentage * capturePercentage;
                    break;
                }

                case UnitActionType.Gather: {
                    action.priority = 1.5f * pathCostPercentage;
                    break;
                }

                case UnitActionType.Join: {
                    float targetHp = action.targetUnit.Hp;
                    float hp = action.unit.Hp;
                    float newTargetHp = Mathf.Min(MaxHp(action.targetUnit), targetHp + hp);
                    float hpAdded = newTargetHp - targetHp;
                    float hpLost = hp - hpAdded;
                    float hpBalance = hpAdded - hpLost;
                    action.priority = pathCostPercentage * (1 + hpBalance / 10);
                    break;
                }

                case UnitActionType.Supply: {
                    var weaponNames = GetWeaponNames(action.targetUnit).ToList();
                    if (action.targetUnit.Fuel == 0 || weaponNames.Count > 0 && weaponNames.Any(weaponName => action.targetUnit.GetAmmo(weaponName) == 0))
                        action.priority = pathCostPercentage;
                    break;
                }
            }
        }
    }

    public IEnumerator DrawPotentialUnitActions(IEnumerable<PotentialUnitAction> actions) {
        var list = actions.ToList();
        if (list.Count == 0)
            yield break;

        while (!Input.GetKeyDown(KeyCode.Alpha0)) {
            yield return null;

            // re-prioritize in real time
            PrioritizePotentialUnitActions(list);

            var minPriority = list.Min(action => action.priority);
            var maxPriority = list.Max(action => action.priority);
            var priorityRange = maxPriority - minPriority;

            foreach (var action in list) {
                var color = Mathf.Approximately(0, priorityRange) ? Color.blue : priorityGradientMap.Sample((action.priority - minPriority) / priorityRange);

                using (Draw.ingame.WithLineWidth(pathThickness))
                    for (var i = 1; i < action.path.Count; i++)
                        Draw.ingame.Arrow((Vector3)action.path[i - 1].ToVector3Int(), (Vector3)action.path[i].ToVector3Int(), Vector3.up, arrowHeadSize, color);

                using (Draw.ingame.WithLineWidth(restPathThickness))
                    for (var i = 1; i < action.restPath.Count; i++)
                        Draw.ingame.Arrow((Vector3)action.restPath[i - 1].ToVector3Int(), (Vector3)action.restPath[i].ToVector3Int(), Vector3.up, arrowHeadSize, color * restPathAlpha);

                //

                Vector3 to = action.restPath[^1].ToVector3Int();
                var from = (Vector3)action.restPath[^1].ToVector3Int();
                if (action.targetUnit is { Position: { } unitPosition })
                    to = unitPosition.ToVector3Int();
                if (action.targetPosition is { } targetPosition)
                    to = targetPosition.ToVector3Int();
                if (action.targetBuilding != null)
                    to = action.targetBuilding.position.ToVector3Int();

                using (Draw.ingame.WithLineWidth(actionLineThickness))
                    Draw.ingame.Line(Vector3.Lerp(from, to, actionLineLerp[0]), Vector3.Lerp(from, to, actionLineLerp[1]), GetPotentialUnitActionTypeColor(action.type));
                Draw.ingame.Label2D(Vector3.Lerp(from, to, .5f) + Vector3.up * .5f, $"{action.priority:0.00}: {action}\n", textSize, LabelAlignment.BottomCenter, color);
            }
        }
    }

    [Command]
    public Color colorGather = Color.blue;
    [Command]
    public Color colorJoin = Color.green;
    [Command]
    public Color colorCapture = Color.cyan;
    [Command]
    public Color colorAttack = Color.red;
    [Command]
    public Color colorGetIn = Color.yellow;
    [Command]
    public Color colorDrop = Color.yellow;
    [Command]
    public Color colorSupply = Color.yellow;
    [Command]
    public float pathThickness = 2;
    [Command]
    public float restPathThickness = 1;
    [Command]
    public float actionLineThickness = 1;
    [Command]
    public float arrowHeadSize = .1f;
    [Command]
    public Color restPathAlpha = new(1, 1, 1, .5f);
    [Command]
    public Vector2 actionLineLerp = new(.1f, 1);

    public Color GetPotentialUnitActionTypeColor(UnitActionType type) {
        return type switch {
            UnitActionType.Join => colorJoin,
            UnitActionType.Capture => colorCapture,
            UnitActionType.Attack => colorAttack,
            UnitActionType.GetIn => colorGetIn,
            UnitActionType.Drop => colorDrop,
            UnitActionType.Supply => colorSupply,
            UnitActionType.Gather => colorGather,
            _ => throw new ArgumentOutOfRangeException(type.ToString())
        };
    }

    public static int PathCost(Unit unit, IEnumerable<Vector2Int> path) {
        var total = 0;
        foreach (var position in path) {
            var isValid = Rules.TryGetMoveCost(unit, position, out var cost);
            Assert.IsTrue(isValid);
            total += cost;
        }

        return total;
    }
}

[Serializable]
public class PotentialUnitAction {

    public Unit unit;
    public UnitActionType type;
    public Unit targetUnit;
    public Building targetBuilding;
    public Vector2Int? targetPosition;
    public WeaponName weaponName;
    public List<Vector2Int> path, restPath;
    public IEnumerable<Vector2Int> FullPath => path.Concat(restPath.Skip(1));
    public Unit carrier;
    public List<Vector2Int> carrierPath, carrierRestPath;

    public float priority;

    public override string ToString() {
        var text = "";
        text += type.ToString();
        if (targetUnit != null)
            text += $" {targetUnit}";
        if (targetBuilding != null)
            text += $" {targetBuilding}";
        if (targetPosition is { } actualTargetPosition)
            text += $" to {actualTargetPosition}";
        if (type == UnitActionType.Attack)
            text += $" with {weaponName}";
        return text;
    }
}