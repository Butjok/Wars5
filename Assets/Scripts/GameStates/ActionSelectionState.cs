using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class ActionSelectionState {

    public const string prefix = "action-selection-state.";
    
    public const string cancel = prefix + "cancel";
    public const string cycleActions = prefix + "cycle-actions";
    public const string execute = prefix + "execute";
    public const string filterWithType = prefix + "filter-with-type";
    
    public static IEnumerator Run(Main main, Unit unit, MovePath path, Vector2Int startForward) {

        main.TryGetUnit(path.Destination, out var other);

        var actions = new List<UnitAction>();
        var index = -1;

        // stay/capture
        if (other == null || other == unit) {
            if (main.TryGetBuilding(path.Destination, out var building) && Rules.CanCapture(unit, building))
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
        if (!Rules.IsArtillery(unit) || path.Count == 1)
            foreach (var otherPosition in main.AttackPositions(path.Destination, Rules.AttackRange(unit)))
                if (main.TryGetUnit(otherPosition, out other))
                    for (var weapon = 0; weapon < Rules.WeaponsCount(unit); weapon++)
                        if (Rules.CanAttack(unit, other, path, weapon))
                            actions.Add(new UnitAction(UnitActionType.Attack, unit, path, other, weaponIndex: weapon, targetPosition: otherPosition));

        // supply
        foreach (var offset in Rules.offsets) {
            var otherPosition = path.Destination + offset;
            if (main.TryGetUnit(otherPosition, out other) && Rules.CanSupply(unit, other))
                actions.Add(new UnitAction(UnitActionType.Supply, unit, path, other, targetPosition: otherPosition));
        }

        // drop out
        foreach (var cargo in unit.cargo)
        foreach (var offset in Rules.offsets) {
            var targetPosition = path.Destination + offset;
            if (!main.TryGetUnit(targetPosition, out other) && Rules.CanStay(cargo, targetPosition))
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
                        main.stack.Push(new List<UnitAction>{actions[index]});
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
                            yield return action.Execute();

                            foreach (var item in filteredActions)
                                item.Dispose();

                            var won = Rules.Won(main.localPlayer);
                            var lost = Rules.Lost(main.localPlayer);

                            if (won || lost) {

                                foreach (var u in main.units.Values)
                                    u.moved.v = false;

                                var nextState = won ? main.levelLogic.OnVictory(main) : main.levelLogic.OnDefeat(main);
                                yield return nextState;

                                yield return won ? VictoryState.Run(main) : DefeatState.New(main);
                                yield break;
                            }

                            else {
                                var (controlFlow, nextState) = main.levelLogic.OnActionCompletion(main, action);
                                yield return nextState;
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
                            unit.view.LookDirection = startForward;

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