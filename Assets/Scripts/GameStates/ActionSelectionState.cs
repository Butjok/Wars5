using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class ActionSelectionState {

    public const string prefix = "action-selection-state.";

    public const string cancel = prefix + "cancel";
    public const string cycleActions = prefix + "cycle-actions";
    public const string execute = prefix + "execute";
    public const string filterWithType = prefix + "filter-with-type";

    public static IEnumerator Run(Main main, Unit unit, IReadOnlyList<Vector2Int> path, Vector2Int? _initialLookDirection = null) {

        var destination = path.Last();
        main.TryGetUnit(destination, out var other);

        var actions = new List<UnitAction>();
        var index = -1;

        // stay/capture
        if (other == null || other == unit) {
            if (main.TryGetBuilding(destination, out var building) && Rules.CanCapture(unit, building))
                actions.Add(new UnitAction(UnitActionType.Capture, unit, path, null, building));
            else
                actions.Add(new UnitAction(UnitActionType.Stay, unit, path));
        }

        // join
        if (other != null && Rules.CanJoin(unit, other))
            actions.Add(new UnitAction(UnitActionType.Join, unit, path, unit));

        // load in
        if (other != null && Rules.CanLoadAsCargo(other, unit))
            actions.Add(new UnitAction(UnitActionType.GetIn, unit, path, other));

        // attack
        if ((!Rules.IsArtillery(unit) || path.Count == 1) && Rules.TryGetAttackRange(unit, out var attackRange))
            foreach (var otherPosition in main.AttackPositions(destination, attackRange))
                if (main.TryGetUnit(otherPosition, out var target))
                    foreach (var (weaponName,_) in Rules.GetDamageValues(unit, target))
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
        yield return null;

        panel.Show(main, actions, (_, action) => SelectAction(action));
        if (actions.Count > 0)
            SelectAction(actions[0]);

        void HidePanel() {
            if (oldAction != null && oldAction.view)
                oldAction.view.Show = false;
            panel.Hide();
            PlayerView.globalVisibility = true;
        }

        while (true) {
            yield return null;

            if (!main.CurrentPlayer.IsAi) {

                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    main.commands.Enqueue(cancel);

                else if (Input.GetKeyDown(KeyCode.Tab))
                    main.commands.Enqueue(cycleActions);

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
                foreach (var token in input.Tokenize())
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
                            unit.Moved = true;

                            Debug.Log($"EXECUTING: {unit} {action.type} {action.targetUnit}");

                            switch (action.type) {

                                case UnitActionType.Stay: {
                                    unit.Position = destination;
                                    break;
                                }

                                case UnitActionType.Join: {
                                    other.Hp = Mathf.Min(Rules.MaxHp(other), other.Hp + unit.Hp);
                                    unit.Dispose();
                                    unit = null;
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
                                    
                                    if (!Rules.TryGetDamage(attacker,target,action.weaponName, out var damageToTarget))
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

                                    if (newTargetHp <= 0 && cameraRig) {
                                        var animation = cameraRig.Jump(((Vector2Int)target.Position).Raycast());
                                        while (animation.active)
                                            yield return null;
                                    }
                                    target.Hp= newTargetHp;

                                    //if (newTargetHp > 0 && targetWeaponIndex != -1)
                                    //    target.ammo[targetWeaponIndex]--;

                                    if (newAttackerHp <= 0 && cameraRig) {
                                        var animation = cameraRig.Jump(destination.Raycast());
                                        while (animation.active)
                                            yield return null;
                                    }
                                    attacker.Hp = newAttackerHp;

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
                                    foreach (var weaponName in action.targetUnit.Ammo.Keys)
                                        action.targetUnit.SetAmmo(weaponName,int.MaxValue);
                                    break;
                                }

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

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
                                        yield return new WaitForSeconds(1);
                                        yield return cameraRig.Jump(new Vector2Int(-21,-14).Raycast());
                                        
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

                                var nextState = won ? main.levelLogic.OnVictory(main, action) : main.levelLogic.OnDefeat(main, action);
                                yield return nextState;
                                yield return won ? VictoryState.Run(main, action) : DefeatState.Run(main, action);
                                action.Dispose();
                                yield break;
                            }

                            else {
                                var (controlFlow, nextState) = main.levelLogic.OnActionCompletion(main, action);
                                yield return nextState;
                                action.Dispose();
                                if (controlFlow == ControlFlow.Replace)
                                    yield break;
                                yield return SelectionState.Run(main);
                                yield break;
                            }
                        }

                        case cancel:

                            main.stack.Pop();

                            HidePanel();

                            foreach (var action in actions)
                                action.Dispose();

                            unit.view.Position = path[0];
                            if (_initialLookDirection is { } initialLookDirection)
                                unit.view.LookDirection = initialLookDirection;

                            yield return PathSelectionState.Run(main, unit);
                            yield break;

                        case cycleActions:
                            if (actions.Count > 0) {
                                index = (index + 1) % actions.Count;
                                SelectAction(actions[index]);
                            }
                            else
                                UiSound.Instance.notAllowed.PlayOneShot();
                            break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}