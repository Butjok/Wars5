using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;
using static MoveFinder2;

// ReSharper disable LoopCanBePartlyConvertedToQuery

public class AiPlayerCommander : MonoBehaviour {

    [Serializable]
    public class PotentialUnitAction {

        public Unit unit;
        public UnitActionType type;
        public Unit targetUnit;
        public Building targetBuilding;
        public Vector2Int? targetPosition;
        public WeaponName weaponName;
        public List<Vector2Int> path, restPath;
        public string unitName;

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
    public enum PriorityPreference { Min, Max }

    public GradientMap priorityGradientMap = new();

    public Main2 main;
    public Vector2Int selectPosition;
    public Vector2Int movePosition;
    [Command] public float textSize = 14;

    [Command]
    public bool playForHuman = true;
    [Command]
    public bool waitForKeyPress = true;
    [Command]
    public KeyCode nextStepKeyCode = KeyCode.RightArrow;

    //
    // potential unit action priority formula handling
    //

    public static string PriorityFormulasHistoryPath => Path.Combine(Application.dataPath, "PriorityFormula.txt");
    public Stack<(string text, DateTime dateTime)> priorityFormulaHistory = new();

    [Command]
    public string PriorityFormula {
        get => priorityFormulaHistory.TryPeek(out var top) ? top.text : "0";
        set => priorityFormulaHistory.Push((value, DateTime.Now));
    }
    [Command]
    public bool TryPopPriorityFormula() {
        return priorityFormulaHistory.TryPop(out _);
    }

    private void Awake() {
        if (File.Exists(PriorityFormulasHistoryPath)) {
            var list = File.ReadAllText(PriorityFormulasHistoryPath).FromJson<List<(string, DateTime)>>();
            list.Reverse();
            foreach (var item in list)
                priorityFormulaHistory.Push(item);
        }
    }
    private void OnApplicationQuit() {
        var list = new List<(string, DateTime)>();
        foreach (var item in priorityFormulaHistory)
            list.Add(item);
        File.WriteAllText(PriorityFormulasHistoryPath, list.ToJson());
    }

    public PriorityPreference priorityPreference;

    public bool TryFindBestMove(out PotentialUnitAction bestPotentialUnitAction) {

        var actions = new List<PotentialUnitAction>();
        foreach (var unit in main.units.Values.Where(unit => unit.Player == main.CurrentPlayer && !unit.Moved))
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
        None, SelectUnit, SelectPath, SelectAction
    }

    public PotentialUnitAction selectedAction;
    public NextTask nextTask;
    public bool issuedPathSelectionCommand;
    public bool issuedActionSelectionCommand;

    public void IssueCommandsForSelectionState() {
        var shouldEndTurn = !TryFindBestMove(out selectedAction);
        issuedPathSelectionCommand = issuedActionSelectionCommand = false;

        // if unit cannot move the entire path change the action type to stay
        // also if an artillery unit is trying to attack somebody but it moves first - change to stay as well
        if (selectedAction != null &&
            (selectedAction.restPath[^1] != selectedAction.path[^1] ||
             selectedAction.path.Count > 1 && IsArtillery(selectedAction.unit)))

            selectedAction.type = UnitActionType.Stay;

        if (shouldEndTurn) {
            nextTask = NextTask.None;
            main.commands.Enqueue(SelectionState.endTurn);
        }
        else if (selectedAction != null) {
            nextTask = NextTask.SelectPath;
            selectedAction.unitName = selectedAction.unit.ToString();
            main.commands.Enqueue($"{selectedAction.unit.NonNullPosition.x} {selectedAction.unit.NonNullPosition.y} int2 {SelectionState.select}");
        }
    }
    public void IssueCommandsForPathSelectionState() {
        issuedPathSelectionCommand = true;
        foreach (var position in selectedAction.path)
            main.commands.Enqueue($"{position.x} {position.y} int2 {PathSelectionState.appendToPath}");
        main.commands.Enqueue($"false {PathSelectionState.move}");
        nextTask = NextTask.SelectAction;
    }
    public void IssueCommandsForActionSelectionState() {
        issuedActionSelectionCommand = true;

        switch (selectedAction.type) {

            case UnitActionType.Stay:
            case UnitActionType.Join:
            case UnitActionType.Capture:
            case UnitActionType.GetIn:
                main.stack.Peek<List<UnitAction>>().RemoveAll(action => action.type != selectedAction.type);
                break;

            case UnitActionType.Attack:
                main.stack.Peek<List<UnitAction>>().RemoveAll(action => action.type != selectedAction.type || action.targetUnit != selectedAction.targetUnit || action.weaponName != selectedAction.weaponName);
                break;

            case UnitActionType.Supply:
                main.stack.Peek<List<UnitAction>>().RemoveAll(action => action.type != selectedAction.type || action.targetUnit != selectedAction.targetUnit);
                break;

            case UnitActionType.Drop:
                main.stack.Peek<List<UnitAction>>().RemoveAll(action => action.type != selectedAction.type || action.targetUnit != selectedAction.targetUnit || action.targetPosition != selectedAction.targetPosition);
                break;

            default:
                throw new ArgumentOutOfRangeException(selectedAction.type.ToJson());
        }

        Assert.AreEqual(1, main.stack.Peek<List<UnitAction>>().Count);
        main.commands.Enqueue(ActionSelectionState.execute);
        nextTask = NextTask.None;
    }

    [Command]
    public void DrawPotentialUnitActions() {

        if (!Mouse.TryGetPosition(out Vector2Int mousePosition) || !main.TryGetUnit(mousePosition, out var unit))
            return;

        StopAllCoroutines();
        var actions = EnumeratePotentialUnitActions(unit).ToList();
        PrioritizePotentialUnitActions(actions);
        StartCoroutine(DrawPotentialUnitActions(actions));
    }

    public IEnumerable<PotentialUnitAction> EnumeratePotentialUnitActions(Unit unit) {

        FindDestinations(unit);

        //
        // find buildings to capture
        //

        var buildingsToCapture = main.buildings.Values.Where(building => CanCapture(unit, building));
        foreach (var building in buildingsToCapture) {
            if (TryFindPath( out var path, out var restPath, building.position)) {
                yield return new PotentialUnitAction {
                    unit = unit,
                    type = UnitActionType.Capture,
                    targetBuilding = building,
                    path = path,
                    restPath = restPath
                };
            }
        }

        //
        // find missile silos to use
        //

        /*var missileSilos = main.buildings.Values.Where(building => building.type == TileType.MissileSilo && CanLaunchMissile(unit, building));
        foreach (var missileSilo in missileSilos)
            if (MoveFinder.TryFindMove(unit, missileSilo.position))
                yield return new PotentialUnitAction {
                    unit = unit,
                    type = UnitActionType.LaunchMissile,
                    targetBuilding = missileSilo,
                    path = MoveFinder.Path,
                    restPath = MoveFinder.RestPath
                };*/

        //
        // find units to attack
        //

        if (TryGetAttackRange(unit, out var attackRange)) {

            foreach (var target in main.units.Values) {

                List<Vector2Int> path = null;
                List<Vector2Int> restPath = null;

                foreach (var weaponName in GetWeaponNames(unit))
                    if (CanAttack(unit, target, weaponName)) {

                        if (path == null) {
                            var targets = main.PositionsInRange(target.NonNullPosition, attackRange).Where(position => CanStay(unit, position));
                            if (!TryFindPath(out path, out restPath, targets: targets))
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
        }
    }

    public void PrioritizePotentialUnitActions(IEnumerable<PotentialUnitAction> actions) {

        var mostExpensiveUnitCost = UnitTypeSettings.Loaded.Values.Max(item => item.cost);

        foreach (var action in actions) {

            float damageDealt = 0;
            float damageDealtInCredits = 0;
            float damageTaken = 0;
            float damageTakenInCredits = 0;

            if (action.type == UnitActionType.Attack) {

                var isValid = TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damageDealtInteger);
                Assert.IsTrue(isValid);

                damageDealt = damageDealtInteger;
                damageDealtInCredits = damageDealt / MaxHp(action.targetUnit) * mostExpensiveUnitCost;

                // TODO: add damage taken
            }

            action.priority = ExpressionEvaluator.Evaluate(PriorityFormula,
                ("hp", action.unit.Hp),
                ("fuel", action.unit.Fuel),
                ("hasCargo", action.unit.Cargo.Count > 0 ? 1 : 0),
                ("pathLength", action.path.Count),
                ("fullPathLength", action.path.Count + action.restPath.Count - 1),
                ("pathCost", CalculatePathCost(action.unit, action.path)),
                ("fullPathCost", CalculatePathCost(action.unit, action.path.Concat(action.restPath.Skip(1)))),
                (nameof(damageDealt), damageDealt),
                (nameof(damageDealtInCredits), damageDealtInCredits),
                (nameof(damageTaken), damageTaken),
                (nameof(damageTakenInCredits), damageTakenInCredits));
        }
    }

    public static int CalculatePathCost(Unit unit, IEnumerable<Vector2Int> path) {
        return path.Skip(1).Sum(position => {
            var isValid = TryGetMoveCost(unit, position, out var cost);
            Assert.IsTrue(isValid, $"{unit} {position}");
            return cost;
        });
    }

    public IEnumerator DrawPotentialUnitActions(IEnumerable<PotentialUnitAction> actions) {

        var list = actions.ToList();
        if (list.Count == 0)
            yield break;

//var moveFinderDebugDrawer = FindObjectOfType<MoveFinderDebugDrawer>();
        //if (moveFinderDebugDrawer)
        //    moveFinderDebugDrawer.Show = false;

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

                Vector3? to = null;
                var from = (Vector3)action.restPath[^1].ToVector3Int();
                if (action.targetUnit is { Position: { } unitPosition })
                    to = unitPosition.ToVector3Int();
                if (action.targetPosition is { } targetPosition)
                    to = targetPosition.ToVector3Int();
                if (action.targetBuilding != null)
                    to = action.targetBuilding.position.ToVector3Int();

                if (to is { } vector) {
                    Draw.ingame.Line(Vector3.Lerp(from, vector, actionLineLerp[0]), Vector3.Lerp(from, vector, actionLineLerp[1]), GetUnitActionTypeColor(action.type));
                    Draw.ingame.Label2D(Vector3.Lerp(from, vector, .5f), $"{action.priority}: {action}\n", textSize, LabelAlignment.BottomCenter, color);
                }
            }
        }
    }

    [Command]
    public Color colorStay = Color.blue;
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
    public Color colorLaunchMissile = Color.red;
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

    public Color GetUnitActionTypeColor(UnitActionType type) {
        return type switch {
            UnitActionType.Stay => colorStay,
            UnitActionType.Join => colorJoin,
            UnitActionType.Capture => colorCapture,
            UnitActionType.Attack => colorAttack,
            UnitActionType.GetIn => colorGetIn,
            UnitActionType.Drop => colorDrop,
            UnitActionType.Supply => colorSupply,
            UnitActionType.LaunchMissile => colorLaunchMissile,
            _ => throw new ArgumentOutOfRangeException(type.ToString())
        };
    }
}