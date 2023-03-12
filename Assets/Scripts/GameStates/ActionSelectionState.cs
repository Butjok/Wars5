using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static Rules;

// ReSharper disable ConvertToUsingDeclaration

public class ActionSelectionState : IDisposableState {

    public const string prefix = "action-selection-state.";

    public const string cancel = prefix + "cancel";
    public const string cycleActions = prefix + "cycle-actions";
    public const string execute = prefix + "execute";

    public Level level;
    public Unit unit;
    public IReadOnlyList<Vector2Int> path;
    public Vector2Int? initialLookDirection;
    public List<UnitAction> actions = new();
    public UnitActionsPanel panel;
    public UnitAction oldAction;
    public int index = -1;

    public ActionSelectionState(Level level, Unit unit, IReadOnlyList<Vector2Int> path, Vector2Int? initialLookDirection = null) {
        this.level = level;
        this.unit = unit;
        this.path = path;
        this.initialLookDirection = initialLookDirection;
        panel = Object.FindObjectOfType<UnitActionsPanel>(true);
        Assert.IsTrue(panel);
    }

    public void Dispose() {
        HidePanel();
        foreach (var action in actions)
            action.Dispose();
        actions.Clear();
    }

    public void HidePanel() {
        if (oldAction != null && oldAction.view)
            oldAction.view.Show = false;
        panel.Hide();
        PlayerView.globalVisibility = true;
    }

    public void SelectAction(UnitAction action) {
        index = actions.IndexOf(action);
        Assert.IsTrue(index != -1);
//      Debug.Log(actions[index]);
        panel.HighlightAction(action);
        if (oldAction != null && oldAction.view)
            oldAction.view.Show = false;
        if (action.view)
            action.view.Show = true;
        oldAction = action;
    }

    public IEnumerable<UnitAction> SpawnActions() {

        var destination = path[^1];
        level.TryGetUnit(destination, out var other);

        // stay / capture / launch missile
        if (other == null || other == unit) {
            if (level.TryGetBuilding(destination, out var building) && CanCapture(unit, building))
                yield return new UnitAction(UnitActionType.Capture, unit, path, null, building, spawnView: true);
            else
                yield return new UnitAction(UnitActionType.Stay, unit, path, spawnView: true);

            if (level.TryGetBuilding(destination, out building) &&
                building.type is TileType.MissileSilo &&
                CanLaunchMissile(unit, building)) {

                yield return new UnitAction(UnitActionType.LaunchMissile, unit, path, targetBuilding: building, spawnView: true);
            }
        }

        // join
        if (other != null && CanJoin(unit, other))
            yield return new UnitAction(UnitActionType.Join, unit, path, unit, spawnView: true);

        // load in
        if (other != null && CanGetIn(unit, other))
            yield return new UnitAction(UnitActionType.GetIn, unit, path, other, spawnView: true);

        // attack
        if ((!IsArtillery(unit) || path.Count == 1) && TryGetAttackRange(unit, out var attackRange))
            foreach (var otherPosition in level.PositionsInRange(destination, attackRange))
                if (level.TryGetUnit(otherPosition, out var target))
                    foreach (var (weaponName, _) in GetDamageValues(unit, target))
                        yield return new UnitAction(UnitActionType.Attack, unit, path, target, weaponName: weaponName, targetPosition: otherPosition, spawnView: true);

        // supply
        foreach (var offset in offsets) {
            var otherPosition = destination + offset;
            if (level.TryGetUnit(otherPosition, out var target) && CanSupply(unit, target))
                yield return new UnitAction(UnitActionType.Supply, unit, path, target, targetPosition: otherPosition, spawnView: true);
        }

        // drop out
        foreach (var cargo in unit.Cargo)
        foreach (var offset in offsets) {
            var targetPosition = destination + offset;
            if ((!level.TryGetUnit(targetPosition, out var other2) || other2 == unit) &&
                level.TryGetTile(targetPosition, out var tileType) &&
                CanStay(cargo, tileType))
                yield return new UnitAction(UnitActionType.Drop, unit, path, targetUnit: cargo, targetPosition: targetPosition, spawnView: true);
        }
    }

    public IEnumerator<StateChange> Run {
        get {

            var destination = path[^1];
            level.TryGetUnit(destination, out var other);
            level.TryGetBuilding(destination, out var building);
            
            // !!! important
            actions.AddRange(SpawnActions());
            level.stack.Push(actions.ToList());

            // some weirdness
            PlayerView.globalVisibility = false;
            yield return StateChange.none;

            if (!level.CurrentPlayer.IsAi && !level.autoplay) {
                panel.Show(level, actions, (_, action) => SelectAction(action));
                if (actions.Count > 0)
                    SelectAction(actions[0]);
            }

            var issuedAiCommands = false;
            while (true) {
                yield return StateChange.none;

                if (level.autoplay || Input.GetKey(KeyCode.Alpha8)) {
                    if (!issuedAiCommands) {
                        issuedAiCommands = true;
                        level.IssueAiCommandsForActionSelectionState();
                    }
                }
                else if (!level.CurrentPlayer.IsAi) {

                    if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                        level.commands.Enqueue(cancel);

                    else if (Input.GetKeyDown(KeyCode.Tab)) {
                        level.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                        level.commands.Enqueue(cycleActions);
                    }

                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
                        if (actions.Count == 0)
                            UiSound.Instance.notAllowed.PlayOneShot();
                        else {
                            level.stack.Pop();
                            level.stack.Push(new List<UnitAction> { actions[index] });
                            level.commands.Enqueue(execute);
                        }
                    }
                }

                while (level.commands.TryDequeue(out var input))
                    foreach (var token in Tokenizer.Tokenize(input))
                        switch (token) {

                            case execute: {

                                HidePanel();
                                
                                var filteredActions = level.stack.Pop<List<UnitAction>>();
                                Assert.AreEqual(1, filteredActions.Count);
                                var action = filteredActions[0];

                                switch (action.type) {

                                    case UnitActionType.Stay: {
                                        unit.Position = destination;
                                        break;
                                    }

                                    case UnitActionType.Join: {
                                        other.SetHp(other.Hp + unit.Hp);
                                        other.Moved = true;
                                        unit.Dispose();
                                        break;
                                    }

                                    case UnitActionType.Capture: {
                                        unit.Position = destination;
                                        building.Cp -= Cp(unit);
                                        if (building.Cp <= 0) {
                                            building.Player = unit.Player;
                                            building.Cp = MaxCp(building);
                                        }
                                        break;
                                    }

                                    case UnitActionType.Attack: {
                                        yield return StateChange.Push(nameof(AttackActionState),AttackActionState.Run(level,action));
                                        break;
                                    }

                                    case UnitActionType.GetIn: {
                                        unit.Position = null;
                                        unit.Carrier = other;
                                        other.AddCargo(unit);
                                        break;
                                    }

                                    case UnitActionType.Drop: {
                                        unit.Position = destination;
                                        unit.RemoveCargo(action.targetUnit);
                                        action.targetUnit.Position = action.targetPosition;
                                        action.targetUnit.Carrier = null;
                                        action.targetUnit.Moved = true;
                                        break;
                                    }

                                    case UnitActionType.Supply: {
                                        unit.Position = destination;
                                        action.targetUnit.Fuel = int.MaxValue;
                                        foreach (var weaponName in GetWeaponNames(action.targetUnit))
                                            action.targetUnit.SetAmmo(weaponName, int.MaxValue);
                                        break;
                                    }

                                    case UnitActionType.LaunchMissile:
                                        yield return StateChange.Push(nameof(MissileTargetSelectionState), MissileTargetSelectionState.Run(level, action, initialLookDirection));
                                        // unit can destroy itself with a missile lol
                                        if (!unit.Disposed)
                                            unit.Position = destination;
                                        break;

                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                if (!unit.Disposed) {
                                    unit.Moved = true;
                                    if (unit.view.LookDirection != unit.Player.unitLookDirection)
                                        level.StartCoroutine(new MoveSequence(unit.view.transform, null, unit.Player.level.persistentData.gameSettings.unitSpeed, unit.Player.unitLookDirection).Animation());
                                }

                                /*
                                 * some custom level logic on action completion 
                                 */
                                 
                                if (level.triggers.TryGetValue(TriggerName.A, out var reconTrigger)) {

                                    var unitsInReconTrigger = level.units.Values.Where(u => u.Player == level.localPlayer && u.Position is { } position && reconTrigger.Contains(position)).ToArray();
                                    if (unitsInReconTrigger.Length > 0) {

                                        reconTrigger.Clear();
                                        ((LevelEditor)level).LoadAdditively("1");

                                        if (CameraRig.TryFind(out var cameraRig)) {
                                            yield return StateChange.Push("MapRevealWait", Wait.ForSeconds(.5f));
                                            cameraRig.Jump(new Vector2Int(-21, -14).Raycast());
                                        }
                                    }
                                }

                                if (level.triggers.TryGetValue(TriggerName.B, out var aggroTrigger)) {
                                    var unitsInAggroTrigger = level.units.Values.Where(u => u.Player == level.localPlayer && u.Position is { } position && aggroTrigger.Contains(position)).ToArray();
                                    if (unitsInAggroTrigger.Length > 0) {
                                        aggroTrigger.Clear();
                                        Debug.Log("ENEMY NOTICED YOU");
                                    }
                                }
                                
                                /*
                                 * check victory or defeat
                                 */

                                actions.Remove(action);

                                var won = Won(level.localPlayer);
                                var lost = Lost(level.localPlayer);

                                if (won || lost) {

                                    foreach (var u in level.units.Values)
                                        u.Moved = false;

                                    // TODO: add a DRAW outcome
                                    if (won)
                                        yield return StateChange.ReplaceWith(nameof(VictoryDefeatState.Victory), VictoryDefeatState.Victory(level, action));
                                    else
                                        yield return StateChange.ReplaceWith(nameof(VictoryDefeatState.Defeat), VictoryDefeatState.Defeat(level, action));
                                }

                                else {
                                    yield return level.levelLogic.OnActionCompletion(level, action);
                                    yield return StateChange.ReplaceWith(new SelectionState2(level));
                                }

                                break;
                            }

                            case cancel:

                                level.stack.Pop(); // pop actions

                                unit.view.Position = path[0];
                                if (initialLookDirection is { } value)
                                    unit.view.LookDirection = value;

                                yield return StateChange.ReplaceWith(nameof(PathSelectionState), PathSelectionState.Run(level, unit));
                                break;

                            case cycleActions: {
                                var offset = level.stack.Pop<int>();
                                if (actions.Count > 0) {
                                    index = (index + offset).PositiveModulo(actions.Count);
                                    SelectAction(actions[index]);
                                }
                                else
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                break;
                            }

                            default:
                                level.stack.ExecuteToken(token);
                                break;
                        }
            }
        }
    }
}