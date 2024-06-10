using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static Rules;

// ReSharper disable ConvertToUsingDeclaration

public class ActionSelectionState : StateMachineState {

    public enum Command {
        Cancel,
        CycleActions,
        Execute
    }

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
        actions.Clear();

        var level = stateMachine.Find<LevelSessionState>().level;
        if (level.view.actionCircle)
            level.view.actionCircle.gameObject.SetActive(false);

        Game.Instance.dontShowMoveUi = false;
    }

    public void HidePanel() {
        panel.Hide();
        PlayerView.globalVisibility = true;
    }

    public void SelectAction(UnitAction action) {
        index = actions.IndexOf(action);
        Assert.IsTrue(index != -1);
//      Debug.Log(actions[index]);
        panel.HighlightAction(action);
        oldAction = action;

        var level = stateMachine.Find<LevelSessionState>().level;
        var circle = level.view.actionCircle;
        var label = level.view.actionLabel;
        if (circle) {
            circle.gameObject.SetActive(true);
            circle.position = null;

            if (action.targetUnit != null)
                circle.position = action.targetUnit.view.body.transform.position;
            if (action.targetBuilding != null)
                circle.position = action.targetBuilding.position.TryRaycast(out var hit) ? hit.point : action.targetBuilding.position.ToVector3();
            if (action.type == UnitActionType.Drop)
                circle.position = action.targetPosition.TryRaycast(out var hit) ? hit.point : action.targetPosition.ToVector3();

            if (action.type == UnitActionType.Attack && TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damagePercentage)) {
                label.gameObject.SetActive(true);
                label.text.text = '-' + Mathf.RoundToInt(MaxHp(action.targetUnit) * damagePercentage).ToString();
            }
            else
                label.gameObject.SetActive(false);
        }
    }

    public IEnumerable<UnitAction> SpawnActions() {
        var (level, unit, path) = (stateMachine.Find<LevelSessionState>().level, stateMachine.Find<SelectionState>().unit, stateMachine.Find<PathSelectionState>().path);

        var destination = path[^1];
        level.TryGetUnit(destination, out var other);

        // stay / capture / launch missile
        if (other == null || other == unit) {
            if (level.TryGetBuilding(destination, out var building)) {
                if (CanCapture(unit, building))
                    yield return new UnitAction(UnitActionType.Capture, unit, path, null, building);
                if (building.type == TileType.MissileSilo) {
                    if (CanLaunchMissile(unit, building))
                        yield return new UnitAction(UnitActionType.LaunchMissile, unit, path, targetBuilding: building);
                    if (CanLoadMissileSilo(unit, path, building))
                        yield return new UnitAction(UnitActionType.LoadMissileSilo, unit, path, targetBuilding: building);
                }
                if (building.type == TileType.MissileStorage && CanTakeRocket(unit, path, building))
                    yield return new UnitAction(UnitActionType.TakeMissile, unit, path, targetBuilding: building);
            }

            yield return new UnitAction(UnitActionType.Stay, unit, path);

            // pick up crate
            if (level.TryGetCrate(destination, out var crate) && CanPickUpCrate(unit, path, crate))
                yield return new UnitAction(UnitActionType.PickUpCrate, unit, path, targetCrate: crate);

            // tunnel entrance
            if (level.TryGetTunnelEntrance(destination, out var tunnelEntrance) && CanTravelThroughTunnel(unit, path, tunnelEntrance))
                yield return new UnitAction(UnitActionType.TravelThroughTunnel, unit, path, targetTunnelEntrance: tunnelEntrance);
        }

        if (other != null && other != unit) {
            // join
            if (CanJoin(unit, other))
                yield return new UnitAction(UnitActionType.Join, unit, path, unit);

            // load in
            if (CanGetIn(unit, other))
                yield return new UnitAction(UnitActionType.GetIn, unit, path, other);
        }
        else {
            // attack
            if ((!IsIndirect(unit) || path.Count == 1) && TryGetAttackRange(unit, out var attackRange))
                foreach (var otherPosition in level.PositionsInRange(destination, attackRange)) {
                    if (level.TryGetUnit(otherPosition, out var target))
                        foreach (var (weaponName, _) in GetDamageValues(unit, target))
                            yield return new UnitAction(UnitActionType.Attack, unit, path, target, weaponName: weaponName, targetPosition: otherPosition);
                    if (level.TryGetPipeSection(otherPosition, out var pipeSection))
                        yield return new UnitAction(UnitActionType.AttackPipeSection, unit, path, targetPipeSection: pipeSection);
                }

            // supply
            foreach (var offset in gridOffsets) {
                var otherPosition = destination + offset;
                if (level.TryGetUnit(otherPosition, out var target) && CanSupply(unit, target))
                    yield return new UnitAction(UnitActionType.Supply, unit, path, target, targetPosition: otherPosition);
            }

            // drop out
            foreach (var cargo in unit.Cargo)
            foreach (var offset in gridOffsets) {
                var targetPosition = destination + offset;
                if ((!level.TryGetUnit(targetPosition, out var other2) || other2 == unit) &&
                    level.TryGetTile(targetPosition, out var tileType) &&
                    CanStay(cargo, tileType))
                    yield return new UnitAction(UnitActionType.Drop, unit, path, targetUnit: cargo, targetPosition: targetPosition);
            }
        }
    }

    public override IEnumerator<StateChange> Enter {
        get {
            var levelSession = stateMachine.Find<LevelSessionState>();
            var unit = stateMachine.Find<SelectionState>().unit;
            var path = stateMachine.Find<PathSelectionState>().path;

            var destination = path[^1];
            Level.TryGetUnit(destination, out var other);
            Level.TryGetBuilding(destination, out var building);

            // !!! important
            actions = SpawnActions().ToList();

            // some weirdness
            PlayerView.globalVisibility = false;
            yield return StateChange.none;

            if (!Game.Instance.dontShowMoveUi) {
                panel.Show(() => Game.EnqueueCommand(Command.Cancel), actions, (_, action) => SelectAction(action));
                if (actions.Count > 0)
                    SelectAction(actions[0]);
            }

            if (Level.EnableTutorial && !Level.tutorialState.startedCapturing && !Level.tutorialState.explainedActionSelection) {
                Level.tutorialState.explainedActionSelection = true;
                yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.ActionSelectionExplanation));
            }

            while (true) {
                yield return StateChange.none;

                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    Game.EnqueueCommand(Command.Cancel);

                else if (Input.GetKeyDown(KeyCode.Tab))
                    Game.EnqueueCommand(Command.CycleActions, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
                    if (actions.Count == 0)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else
                        Game.EnqueueCommand(Command.Execute, actions[index]);
                }

                while (Game.TryDequeueCommand(out var command)) {
                    // Tutorial logic
                    var showCaptureDialogue = false;
                    if (Level.EnableTutorial && Level.name == LevelName.Tutorial) {
                        if (!Level.tutorialState.startedCapturing)
                            switch (command) {
                                case (Command.Cancel or Command.CycleActions, _):
                                    break;
                                case (Command.Execute, UnitAction action):
                                    if (action.type == UnitActionType.Capture) {
                                        Level.tutorialState.startedCapturing = true;
                                        showCaptureDialogue = true;
                                        break;
                                    }
                                    goto default;
                                default:
                                    yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.WrongActionSelectionPleaseCaptureBuilding));
                                    continue;
                            }
                    }

                    switch (command) {
                        case (Command.Execute, UnitAction action): {
                            Level.view.tilemapCursor.Hide();
                            if (Level.view.actionCircle)
                                Level.view.actionCircle.gameObject.SetActive(false);

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
                                    unit.Despawn();
                                    break;
                                }

                                case UnitActionType.Capture: {
                                    unit.Position = destination;
                                    var oldCp = building.Cp;
                                    building.Cp -= Cp(unit);

                                    // if building is going to be captured by enemy, jump camera to it
                                    if (building.Cp <= 0 && Level.CurrentPlayer != Level.localPlayer) {
                                        Level.view.cameraRig.Jump(building.view.transform.position);
                                        var time = Time.time;
                                        while (Time.time < time + Level.view.cameraRig.jumpDuration)
                                            yield return StateChange.none;
                                    }

                                    var captureScreen = Level.view.captureScreen;
                                    captureScreen.Visible = true;
                                    captureScreen.circle.position = action.targetBuilding.view.Position.ToVector3();
                                    captureScreen.UiColor = building.Player?.UiColor ?? captureScreen.defaultUiColor;
                                    captureScreen.SetCp(oldCp, MaxCp(building));
                                    captureScreen.SpawnView(action.targetBuilding.view, building.Player?.Color);

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

                                    if (showCaptureDialogue)
                                        yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.NiceJobStartedCapturingBuilding));

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

                                case UnitActionType.LaunchMissile: {
                                    yield return StateChange.Push(new MissileTargetSelectionState(stateMachine));
                                    // unit can destroy itself with a missile lol
                                    if (unit.Hp > 0)
                                        unit.Position = destination;
                                    break;
                                }

                                case UnitActionType.PickUpCrate: {
                                    unit.Position = destination;
                                    action.targetCrate.PickUp(unit);
                                    break;
                                }

                                case UnitActionType.TravelThroughTunnel: {
                                    unit.Position = action.targetTunnelEntrance.connected.position;
                                    break;
                                }

                                case UnitActionType.TakeMissile: {
                                    unit.Position = destination;
                                    unit.HasMissile = true;
                                    action.targetBuilding.missileStorage.lastRechargeDay = Level.Day();
                                    break;
                                }

                                case UnitActionType.LoadMissileSilo: {
                                    unit.Position = destination;
                                    unit.HasMissile = false;
                                    action.targetBuilding.missileSilo.hasMissile = true;
                                    action.targetBuilding.Moved = false;
                                    break;
                                }

                                case UnitActionType.AttackPipeSection: {
                                    var pipeSection = action.targetPipeSection;
                                    pipeSection.hp -= 5;
                                    if (pipeSection.hp <= 0) {
                                        var level = pipeSection.level;
                                        level.pipeSections.Remove(pipeSection.position);
                                        pipeSection.Despawn();
                                        level.tiles[pipeSection.position] = TileType.Plain;
                                        Effects.SpawnExplosion(pipeSection.position.Raycasted(), Vector3.up, parent: level.view.transform);
                                        ExplosionCrater.SpawnDecal(pipeSection.position, level.view.transform);
                                        Sounds.PlayOneShot(Sounds.explosion);
                                        level.view.cameraRig.Shake();
                                    }
                                    unit.Position = destination;
                                    break;
                                }

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            if (unit.Hp > 0) {
                                unit.Moved = true;
                                if (unit.view.LookDirection != unit.Player.unitLookDirection) {
                                    var bipedalWalker = unit.view.GetComponent<BipedalWalker>();
                                    Game.StartCoroutine(new MoveSequence(unit.view.transform, null, _finalDirection: unit.Player.unitLookDirection,
                                        onComplete: bipedalWalker ? bipedalWalker.ResetFeet : null).Animation());
                                }
                            }

                            /*
                             * some custom level logic on action completion 
                             */

                            if (Level.triggers.TryGetValue(TriggerName.A, out var reconTrigger)) {
                                var unitsInReconTrigger = Level.units.Values.Where(u => u.Player == Level.localPlayer && u.Position is { } position && reconTrigger.Contains(position)).ToArray();
                                if (unitsInReconTrigger.Length > 0) {
                                    reconTrigger.Clear();
                                    // ((LevelEditor)level).LoadAdditively("1");

                                    var cameraRig = Level.view.cameraRig;
                                    var startTime = Time.time;
                                    while (Time.time < startTime + .5f)
                                        yield return StateChange.none;
                                    if (new Vector2Int(-21, -14).TryRaycast(out var hit))
                                        cameraRig.Jump(hit.point);
                                }
                            }

                            if (Level.triggers.TryGetValue(TriggerName.B, out var aggroTrigger)) {
                                var unitsInAggroTrigger = Level.units.Values.Where(u => u.Player == Level.localPlayer && u.Position is { } position && aggroTrigger.Contains(position)).ToArray();
                                if (unitsInAggroTrigger.Length > 0) {
                                    aggroTrigger.Clear();
                                    Debug.Log("ENEMY NOTICED YOU");
                                }
                            }

                            /*
                             * check victory or defeat
                             */

                            var won = Won(Level.localPlayer);
                            var lost = Lost(Level.localPlayer);

                            if (won || lost) {
                                foreach (var u in Level.units.Values)
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
                }

                UpdateTilemapCursor();
            }
        }
    }

}