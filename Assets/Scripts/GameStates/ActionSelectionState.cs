using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class ActionSelectionState {

    public const string prefix = "action-selection-state.";

    public const string cancel = prefix + "cancel";
    public const string cycleActions = prefix + "cycle-actions";
    public const string execute = prefix + "execute";
    public const string filterWithType = prefix + "filter-with-type";
    public const string launchMissile = prefix + "launch-missile";

    public static IEnumerator<StateChange> Run(Main main, Unit unit, IReadOnlyList<Vector2Int> path, Vector2Int? initialLookDirection = null) {

        var destination = path.Last();
        main.TryGetUnit(destination, out var other);

        var actions = new List<UnitAction>();
        var index = -1;

        // stay / capture / launch missile
        if (other == null || other == unit) {
            if (main.TryGetBuilding(destination, out var building) && Rules.CanCapture(unit, building))
                actions.Add(new UnitAction(UnitActionType.Capture, unit, path, null, building));
            else
                actions.Add(new UnitAction(UnitActionType.Stay, unit, path));

            if (main.TryGetBuilding(destination, out building) &&
                building.type is TileType.MissileSilo &&
                Rules.CanLaunchMissile(unit, building)) {

                actions.Add(new UnitAction(UnitActionType.LaunchMissile, unit, path, targetBuilding: building));
            }
        }

        // join
        if (other != null && Rules.CanJoin(unit, other))
            actions.Add(new UnitAction(UnitActionType.Join, unit, path, unit));

        // load in
        if (other != null && Rules.CanLoadAsCargo(other, unit))
            actions.Add(new UnitAction(UnitActionType.GetIn, unit, path, other));

        // attack
        if ((!Rules.IsArtillery(unit) || path.Count == 1) && Rules.TryGetAttackRange(unit, out var attackRange))
            foreach (var otherPosition in main.PositionsInRange(destination, attackRange))
                if (main.TryGetUnit(otherPosition, out var target))
                    foreach (var (weaponName, _) in Rules.GetDamageValues(unit, target))
                        actions.Add(new UnitAction(UnitActionType.Attack, unit, path, target, weaponName: weaponName, targetPosition: otherPosition));

        // supply
        foreach (var offset in Rules.offsets) {
            var otherPosition = destination + offset;
            if (main.TryGetUnit(otherPosition, out var target) && Rules.CanSupply(unit, target))
                actions.Add(new UnitAction(UnitActionType.Supply, unit, path, target, targetPosition: otherPosition));
        }

        // drop out
        foreach (var cargo in unit.Cargo)
        foreach (var offset in Rules.offsets) {
            var targetPosition = destination + offset;
            if ((!main.TryGetUnit(targetPosition, out var other2) || other2 == unit) &&
                main.TryGetTile(targetPosition, out var tileType) &&
                Rules.CanStay(cargo, tileType))
                actions.Add(new UnitAction(UnitActionType.Drop, unit, path, targetUnit: cargo, targetPosition: targetPosition));
        }

        // !! important
        main.stack.Push(actions);

        var panel = Object.FindObjectOfType<UnitActionsPanel>(true);
        Assert.IsTrue(panel);

        UnitAction oldAction = null;
        void SelectAction(UnitAction action) {

            index = actions.IndexOf(action);
            Assert.IsTrue(index != -1);
            Debug.Log(actions[index]);
            panel.HighlightAction(action);

            if (oldAction != null && oldAction.view)
                oldAction.view.Show = false;
            if (action.view)
                action.view.Show = true;
            oldAction = action;
        }

        PlayerView.globalVisibility = false;
        yield return StateChange.none;

        panel.Show(main, actions, (_, action) => SelectAction(action));
        if (actions.Count > 0)
            SelectAction(actions[0]);

        void HidePanel() {
            if (oldAction != null && oldAction.view)
                oldAction.view.Show = false;
            panel.Hide();
            PlayerView.globalVisibility = true;
        }
        void CleanUp() {
            HidePanel();
            foreach (var action in actions)
                action.Dispose();
        }

        IEnumerator<StateChange> MissileTargetSelection(UnitAction action) {

            var missileBlastRange = new Vector2Int(0, 3);

            var oldShowCursorView = false;
            if (CursorView.TryFind(out var cursorView)) {
                oldShowCursorView = cursorView.show;
                cursorView.show = true;
            }

            var missileSilo = action.targetBuilding;
            var missileSiloView = missileSilo.view as MissileSiloView;

            void CleanUpSubstate() {
                if (cursorView)
                    cursorView.show = oldShowCursorView;
                
                if (missileSiloView)
                    missileSiloView.aim = false;
            }

            Vector2Int? launchPosition = null;
            
            while (true) {
                yield return StateChange.none;

                if (Input.GetMouseButtonDown(Mouse.left) && Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                    if ((mousePosition- missileSilo.position).ManhattanLength().IsIn(missileSilo.missileSiloRange)) {
                        if (launchPosition != mousePosition) {
                            launchPosition = mousePosition;
                        }
                        else {
                            main.stack.Push(missileSilo);
                            main.stack.Push(launchPosition);
                            main.commands.Enqueue(launchMissile);
                        }
                    }
                    else 
                        UiSound.Instance.notAllowed.PlayOneShot();
                }
                
                else if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    main.commands.Enqueue(cancel);

                while (main.commands.TryDequeue(out var input2))
                    foreach (var token2 in Tokenizer.Tokenize(input2))
                        switch (token2) {

                            case launchMissile: {

                                var targetPosition = main.stack.Pop<Vector2Int>();
                                var missileSilo1 = main.stack.Pop<Building>();
                                Assert.AreEqual(TileType.MissileSilo, missileSilo1.type);

                                Debug.Log($"Launching missile from {missileSilo1.position} to {targetPosition}");
                                using (Draw.ingame.WithDuration(1))
                                using (Draw.ingame.WithLineWidth(2))
                                    Draw.ingame.Arrow((Vector3)missileSilo1.position.ToVector3Int(), (Vector3)targetPosition.ToVector3Int(), Color.red);

                                if (missileSiloView) {
                                    
                                    missileSiloView.SnapToTargetRotationInstantly();
                                    
                                    var missile = missileSiloView.TryLaunchMissile();
                                    Assert.IsTrue(missile);
                                    if (missile.curve.totalTime is not { } flightTime)
                                        throw new AssertionException("missile.curve.totalTime = null", null);
                                    
                                    if (CameraRig.TryFind(out var cameraRig))
                                        cameraRig.Jump(Vector2.Lerp(missileSilo1.position, targetPosition,.5f).Raycast());
                                    yield return StateChange.Push("missile-flight", Wait.ForSeconds(flightTime));
                                }

                                unit.Position = destination;
                                missileSilo1.missileSiloLastLaunchTurn = main.turn;
                                missileSilo1.missileSiloAmmo--;

                                var targetedBridges = main.bridges.Where(bridge => bridge.tiles.ContainsKey(targetPosition));
                                foreach (var bridge in targetedBridges)
                                    bridge.SetHp(bridge.Hp-10,true);

                                CleanUpSubstate();
                                yield return StateChange.Pop();
                                break;
                            }

                            case cancel:
                                CleanUpSubstate();
                                CleanUp();
                                yield return StateChange.PopThenPush(2, "action-selection", Run(main, unit, path, initialLookDirection));
                                break;

                            default:
                                main.stack.ExecuteToken(token2);
                                break;
                        }

                if (launchPosition is { } position1)
                    foreach (var attackPosition in main.PositionsInRange(position1, missileBlastRange))
                        Draw.ingame.SolidPlane((Vector3)attackPosition.ToVector3Int(), Vector3.up, Vector2.one, Color.red);

                if (Mouse.TryGetPosition(out mousePosition) && missileSiloView &&
                    (mousePosition - missileSilo.position).ManhattanLength().IsIn(missileSilo.missileSiloRange)) {

                    missileSiloView.aim = true;
                    missileSiloView.targetPosition = mousePosition.Raycast();
                }
                else
                    missileSiloView.aim = false;
            }
        }

        while (true) {
            yield return StateChange.none;

            if (!main.CurrentPlayer.IsAi) {

                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    main.commands.Enqueue(cancel);

                else if (Input.GetKeyDown(KeyCode.Tab)) {
                    main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                    main.commands.Enqueue(cycleActions);
                }

                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
                    if (actions.Count == 0)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else {
                        main.stack.Pop();
                        main.stack.Push(new List<UnitAction> { actions[index] });
                        main.commands.Enqueue(execute);
                    }
                }
            }

            while (main.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {

                        case filterWithType: {
                            var type = main.stack.Pop<UnitActionType>();
                            main.stack.Push(main.stack.Pop<List<UnitAction>>().Where(action => action.type == type).ToList());
                            break;
                        }

                        case execute: {

                            var filteredActions = main.stack.Pop<List<UnitAction>>();
                            Assert.AreEqual(1, filteredActions.Count);
                            var action = filteredActions[0];

                            HidePanel();

                            /*
                             * Action execution
                             */

                            main.TryGetBuilding(destination, out var building);

                            Debug.Log($"EXECUTING: {unit} {action.type} {action.targetUnit}");

                            switch (action.type) {

                                case UnitActionType.Stay: {
                                    unit.Position = destination;
                                    break;
                                }

                                case UnitActionType.Join: {
                                    other.SetHp(other.Hp + unit.Hp);
                                    unit.Dispose();
                                    break;
                                }

                                case UnitActionType.Capture: {
                                    unit.Position = destination;
                                    building.Cp -= Rules.Cp(unit);
                                    if (building.Cp <= 0) {
                                        building.Player = unit.Player;
                                        building.Cp = Rules.MaxCp(building);
                                    }
                                    break;
                                }

                                case UnitActionType.Attack: {

                                    var attacker = action.unit;
                                    var target = action.targetUnit;

                                    if (!Rules.TryGetDamage(attacker, target, action.weaponName, out var damageToTarget))
                                        throw new Exception();

                                    var newTargetHp = Mathf.Max(0, target.Hp - damageToTarget);
                                    var newAttackerHp = attacker.Hp;
                                    var targetWeaponIndex = -1;
                                    /*if (newTargetHp > 0 && Rules.CanAttackInResponse(target, attacker, out targetWeaponIndex)) {
                                        if (Rules.Damage(target, attacker, targetWeaponIndex, newTargetHp) is not { } damageToAttacker)
                                            throw new Exception();
                                        newAttackerHp = Mathf.Max(0, newAttackerHp - damageToAttacker);
                                    }*/

                                    if (main.settings.showBattleAnimation)
                                        Debug.Log("BattleAnimationView");

                                    attacker.Position = action.path.Last();

                                    CameraRig.TryFind(out var cameraRig);

                                    if (newTargetHp <= 0 && cameraRig)
                                        yield return StateChange.Push("jump-to-target", Wait.ForCompletion(cameraRig.Jump(((Vector2Int)target.Position).Raycast())));

                                    target.SetHp(newTargetHp,true);

                                    //if (newTargetHp > 0 && targetWeaponIndex != -1)
                                    //    target.ammo[targetWeaponIndex]--;

                                    if (newAttackerHp <= 0 && cameraRig)
                                        yield return StateChange.Push("jump-to-attacker", Wait.ForCompletion(cameraRig.Jump(destination.Raycast())));

                                    attacker.SetHp(newAttackerHp,true);

                                    //if (newAttackerHp > 0)
                                    // attacker.ammo[action.weaponIndex]--;

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
                                    foreach (var weaponName in action.targetUnit.Ammo.Keys.ToArray())
                                        action.targetUnit.SetAmmo(weaponName, int.MaxValue);
                                    break;
                                }

                                case UnitActionType.LaunchMissile:
                                    yield return StateChange.Push("missile-target-selection", MissileTargetSelection(action));
                                    unit.Position = destination;
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            unit.Moved = true;

                            foreach (var item in actions.Except(new[] { action }))
                                item.Dispose();

                            if (unit is { Hp: >0 } && unit.view.LookDirection != unit.Player.unitLookDirection)
                                main.StartCoroutine(new MoveSequence(unit.view.transform, null, unit.Player.main.settings.unitSpeed, unit.Player.unitLookDirection).Animation());

                            if (main.triggers.TryGetValue(TriggerName.A, out var reconTrigger)) {

                                var unitsInReconTrigger = main.units.Values.Where(u => u.Player == main.localPlayer && u.Position is { } position && reconTrigger.Contains(position)).ToArray();
                                if (unitsInReconTrigger.Length > 0) {

                                    reconTrigger.Clear();
                                    ((Main2)main).LoadAdditively("1");

                                    if (CameraRig.TryFind(out var cameraRig)) {
                                        yield return StateChange.Push("wait", Wait.ForSeconds(.5f));
                                        cameraRig.Jump(new Vector2Int(-21, -14).Raycast());
                                    }
                                }
                            }

                            if (main.triggers.TryGetValue(TriggerName.B, out var aggroTrigger)) {
                                var unitsInAggroTrigger = main.units.Values.Where(u => u.Player == main.localPlayer && u.Position is { } position && aggroTrigger.Contains(position)).ToArray();
                                if (unitsInAggroTrigger.Length > 0) {
                                    aggroTrigger.Clear();
                                    Debug.Log("ENEMY NOTICED YOU");
                                }
                            }

                            // if (unit.view.LookDirection != unit.player.view.unitLookDirection)
                            // unit.view.LookDirection = unit.player.view.unitLookDirection;

                            var won = Rules.Won(main.localPlayer);
                            var lost = Rules.Lost(main.localPlayer);

                            if (won || lost) {

                                foreach (var u in main.units.Values)
                                    u.Moved = false;

                                if (won)
                                    yield return StateChange.ReplaceWith("victory", VictoryState.Run(main, action));
                                else
                                    yield return StateChange.ReplaceWith("defeat", DefeatState.Run(main, action));
                            }

                            else {
                                yield return main.levelLogic.OnActionCompletion(main, action);
                                yield return StateChange.ReplaceWith("selection", SelectionState.Run(main));
                            }

                            break;
                        }

                        case cancel:

                            main.stack.Pop(); // pop actions
                            CleanUp();

                            unit.view.Position = path[0];
                            if (initialLookDirection is { } value)
                                unit.view.LookDirection = value;

                            yield return StateChange.ReplaceWith("path-selection", PathSelectionState.Run(main, unit));
                            break;

                        case cycleActions: {
                            var offset = main.stack.Pop<int>();
                            if (actions.Count > 0) {
                                index = (index + offset).PositiveModulo(actions.Count);
                                SelectAction(actions[index]);
                            }
                            else
                                UiSound.Instance.notAllowed.PlayOneShot();
                            break;
                        }

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}