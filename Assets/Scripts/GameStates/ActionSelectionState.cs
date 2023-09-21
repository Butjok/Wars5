using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static Rules;
using Random = UnityEngine.Random;

// ReSharper disable ConvertToUsingDeclaration

public class ActionSelectionState : StateMachineState {

    public enum Command { Cancel, CycleActions, Execute }

    public List<UnitAction> actions = new();
    public UnitActionsPanel panel;
    public UnitAction oldAction;
    public int index = -1;

    public UnitAction selectedAction;

    public ActionSelectionState(StateMachine stateMachine) : base(stateMachine) {
        panel = Object.FindObjectOfType<UnitActionsPanel>(true);
        Assert.IsTrue(panel);
    }

    public override void Exit() {
        HidePanel();
        foreach (var action in actions)
            action.Dispose();
        actions.Clear();

        var level = FindState<LevelSessionState>().level;
        if (level.view.actionCircle)
            level.view.actionCircle.gameObject.SetActive(false);
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

        var level = FindState<LevelSessionState>().level;
        var circle = level.view.actionCircle;
        var label = level.view.actionLabel;
        if (circle) {

            circle.gameObject.SetActive(true);
            circle.position = null;

            if (action.targetUnit != null)
                circle.position = action.targetUnit.view.transform.position;
            if (action.targetBuilding != null)
                circle.position = action.targetBuilding.view.transform.position;
            if (action.type == UnitActionType.Drop)
                circle.position = action.targetPosition.ToVector3();

            if (action.type == UnitActionType.Attack && TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damagePercentage)) {
                label.gameObject.SetActive(true);
                label.text.text = '-' + Mathf.RoundToInt(MaxHp(action.targetUnit) * damagePercentage).ToString();
            }
            else
                label.gameObject.SetActive(false);
        }
    }

    public IEnumerable<UnitAction> SpawnActions() {

        var (level, unit, path) = (FindState<LevelSessionState>().level, FindState<SelectionState>().unit, FindState<PathSelectionState>().path);

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
        foreach (var offset in gridOffsets) {
            var otherPosition = destination + offset;
            if (level.TryGetUnit(otherPosition, out var target) && CanSupply(unit, target))
                yield return new UnitAction(UnitActionType.Supply, unit, path, target, targetPosition: otherPosition, spawnView: true);
        }

        // drop out
        foreach (var cargo in unit.Cargo)
        foreach (var offset in gridOffsets) {
            var targetPosition = destination + offset;
            if ((!level.TryGetUnit(targetPosition, out var other2) || other2 == unit) &&
                level.TryGetTile(targetPosition, out var tileType) &&
                CanStay(cargo, tileType))
                yield return new UnitAction(UnitActionType.Drop, unit, path, targetUnit: cargo, targetPosition: targetPosition, spawnView: true);
        }
    }

    public override IEnumerator<StateChange> Enter {
        get {
            var levelSession = FindState<LevelSessionState>();
            var (game, level, unit, path) = (FindState<GameSessionState>().game, levelSession.level, FindState<SelectionState>().unit, FindState<PathSelectionState>().path);

            var destination = path[^1];
            level.TryGetUnit(destination, out var other);
            level.TryGetBuilding(destination, out var building);

            // !!! important
            actions = SpawnActions().ToList();

            // some weirdness
            PlayerView.globalVisibility = false;
            yield return StateChange.none;

            if (!levelSession.autoplay) {
                panel.Show(() => game.EnqueueCommand(Command.Cancel), actions, (_, action) => SelectAction(action));
                if (actions.Count > 0)
                    SelectAction(actions[0]);
            }

            var issuedAiCommands = false;
            while (true) {
                yield return StateChange.none;

                if (levelSession.autoplay) {
                    if (!issuedAiCommands) {
                        issuedAiCommands = true;
                        game.aiPlayerCommander.IssueCommandsForActionSelectionState();
                    }
                }
                else if (!level.CurrentPlayer.IsAi) {

                    if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                        game.EnqueueCommand(Command.Cancel);

                    else if (Input.GetKeyDown(KeyCode.Tab))
                        game.EnqueueCommand(Command.CycleActions, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
                        if (actions.Count == 0)
                            UiSound.Instance.notAllowed.PlayOneShot();
                        else
                            game.EnqueueCommand(Command.Execute, actions[index]);
                    }
                }

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Execute, UnitAction action): {

                            level.view.tilemapCursor.Hide();
                            if (level.view.actionCircle)
                                level.view.actionCircle.gameObject.SetActive(false);

                            selectedAction = action;
                            HidePanel();

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
                                    var oldCp = building.Cp;
                                    building.Cp -= Cp(unit);

                                    // if building is going to be captured by enemy, jump camera to it
                                    if (building.Cp <= 0 && level.CurrentPlayer != level.localPlayer) {
                                        level.view.cameraRig.Jump(building.view.transform.position);
                                        var time = Time.time;
                                        while (Time.time < time + level.view.cameraRig.jumpDuration)
                                            yield return StateChange.none;
                                    }

                                    var captureScreen = level.view.captureScreen;
                                    captureScreen.Visible = true;
                                    captureScreen.circle.position = action.targetBuilding.view.Position.ToVector3();
                                    captureScreen.UiColor = building.Player?.UiColor ?? captureScreen.defaultUiColor;
                                    captureScreen.SetCp(oldCp, MaxCp(building));
                                    captureScreen.SpawnView(action.targetBuilding.view, building.Player?.UiColor ?? captureScreen.defaultColor);

                                    // pause
                                    var startTime = Time.time;
                                    while (Time.time < startTime + captureScreen.pauseBefore)
                                        yield return StateChange.none;

                                    var completed = captureScreen.AnimateCp(building.Cp, MaxCp(action.targetBuilding));
                                    while (!completed())
                                        yield return StateChange.none;

                                    if (building.Cp <= 0) {

                                        building.Player = unit.Player;
                                        building.Cp = MaxCp(building);

                                        captureScreen.UiColor = building.Player.UiColor;
                                        captureScreen.Color = building.Player.Color;
                                        captureScreen.SetCp(0, MaxCp(building));

                                        // pause
                                        startTime = Time.time;
                                        while (Time.time < startTime + captureScreen.pauseOwnerChange)
                                            yield return StateChange.none;
                                        
                                        completed = captureScreen.AnimateCp(building.Cp, MaxCp(action.targetBuilding));
                                        while (!completed())
                                            yield return StateChange.none;
                                    }

                                    // pause
                                    startTime = Time.time;
                                    while (Time.time < startTime + captureScreen.pauseAfter)
                                        yield return StateChange.none;

                                    captureScreen.Visible = false;
                                    captureScreen.circle.position = null;
                                    captureScreen.DestroyView();

                                    break;
                                }

                                case UnitActionType.Attack: {
                                    yield return StateChange.Push(new AttackActionState(stateMachine));
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
                                    yield return StateChange.Push(new MissileTargetSelectionState(stateMachine));
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
                                    game.StartCoroutine(new MoveSequence(unit.view.transform, null, _finalDirection: unit.Player.unitLookDirection).Animation());
                            }

                            /*
                             * some custom level logic on action completion 
                             */

                            if (level.triggers.TryGetValue(TriggerName.A, out var reconTrigger)) {

                                var unitsInReconTrigger = level.units.Values.Where(u => u.Player == level.localPlayer && u.Position is { } position && reconTrigger.Contains(position)).ToArray();
                                if (unitsInReconTrigger.Length > 0) {

                                    reconTrigger.Clear();
                                    // ((LevelEditor)level).LoadAdditively("1");

                                    var cameraRig = level.view.cameraRig;
                                    var startTime = Time.time;
                                    while (Time.time < startTime + .5f)
                                        yield return StateChange.none;
                                    cameraRig.Jump(new Vector2Int(-21, -14).Raycast());
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

                            var won = Won(level.localPlayer);
                            var lost = Lost(level.localPlayer);

                            if (won || lost) {

                                foreach (var u in level.units.Values)
                                    u.Moved = false;

                                // TODO: add a DRAW outcome
                                if (won)
                                    yield return StateChange.PopThenPush(3, new VictoryState(stateMachine));
                                else
                                    yield return StateChange.PopThenPush(3, new DefeatState(stateMachine));
                            }

                            else {
                                yield return StateChange.PopThenPush(3, new SelectionState(stateMachine));
                            }

                            break;
                        }

                        case (Command.Cancel, _):

                            unit.view.Position = path[0];
                            unit.view.LookDirection = stateMachine.TryFind<PathSelectionState>().initialLookDirection;

                            yield return StateChange.PopThenPush(2, new PathSelectionState(stateMachine));
                            break;

                        case (Command.CycleActions, int offset): {
                            if (actions.Count > 0) {
                                index = (index + offset).PositiveModulo(actions.Count);
                                SelectAction(actions[index]);
                            }
                            else
                                UiSound.Instance.notAllowed.PlayOneShot();
                            break;
                        }

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }

                level.UpdateTilemapCursor();
            }
        }
    }

}