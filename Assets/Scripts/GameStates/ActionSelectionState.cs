using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class ActionSelectionState {

    public static bool cancel = false;
    
    public static IEnumerator New(Game game, Unit unit, MovePath path, Vector2Int startForward) {

        cancel = false;

        game.TryGetUnit(path.Destination, out var other);

        var actions = new List<UnitAction>();
        var index = -1;

        // stay/capture
        if (other == null || other == unit) {
            if (game.TryGetBuilding(path.Destination, out var building) && Rules.CanCapture(unit, building))
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
            foreach (var otherPosition in game.AttackPositions(path.Destination, Rules.AttackRange(unit)))
                if (game.TryGetUnit(otherPosition, out other))
                    for (var weapon = 0; weapon < Rules.WeaponsCount(unit); weapon++)
                        if (Rules.CanAttack(unit, other, path, weapon))
                            actions.Add(new UnitAction(UnitActionType.Attack, unit, path, other, weaponIndex: weapon, targetPosition: otherPosition));

        // supply
        foreach (var offset in Rules.offsets) {
            var otherPosition = path.Destination + offset;
            if (game.TryGetUnit(otherPosition, out other) && Rules.CanSupply(unit, other))
                actions.Add(new UnitAction(UnitActionType.Supply, unit, path, other, targetPosition: otherPosition));
        }

        // drop out
        foreach (var cargo in unit.cargo)
        foreach (var offset in Rules.offsets) {
            var targetPosition = path.Destination + offset;
            if (!game.TryGetUnit(targetPosition, out other) && Rules.CanStay(cargo, targetPosition))
                actions.Add(new UnitAction(UnitActionType.Drop, unit, path, targetUnit: cargo, targetPosition: targetPosition));
        }

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
        
        panel.Show(actions, (_, action) => SelectAction(action));
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

            // action is selected
            if (game.input.actionFilter != null) {

                HidePanel();

                var action = actions.Single(action => game.input.actionFilter(action));
                yield return action.Execute();
                game.input.actionFilter = null;

                foreach (var item in actions)
                    item.Dispose();

                var won = Rules.Won(game.localPlayer);
                var lost = Rules.Lost(game.localPlayer);

                if (won || lost) {

                    foreach (var u in game.units.Values)
                        u.moved.v = false;

                    var nextState = won ? game.levelLogic.OnVictory(game) : game.levelLogic.OnDefeat(game);
                    yield return nextState;

                    yield return won ? VictoryState.New(game) : DefeatState.New(game);
                    yield break;
                }

                else {
                    var (controlFlow, nextState) = game.levelLogic.OnActionCompletion(game, action);
                    yield return nextState;
                    if (controlFlow == ControlFlow.Replace)
                        yield break;
                    yield return SelectionState.New(game);
                    yield break;
                }
            }

            if (game.CurrentPlayer.IsAi)
                continue;

            if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape) || cancel) {
                cancel = false;
                
                HidePanel();

                foreach (var action in actions)
                    action.Dispose();

                unit.view.Position = path[0];
                unit.view.Forward = startForward;

                yield return PathSelectionState.New(game, unit);
                yield break;
            }

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                if (actions.Count > 0) {
                    index = (index + 1) % actions.Count;
                    SelectAction(actions[index]);
                }
                else
                    UiSound.Instance.notAllowed.PlayOneShot();
            }

            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
                if (actions.Count == 0)
                    UiSound.Instance.notAllowed.PlayOneShot();
                else {
                    var action = actions[index];
                    game.input.actionFilter = a => a == action;
                }
            }
        }
    }
}