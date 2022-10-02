using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ActionSelectionState {

    public static IEnumerator New(Game2 game, Unit unit, Vector2Int startForward, MovePath path) {

        var position = path.positions.Last();
        game.TryGetUnit(position, out var other);

        var actions = new List<UnitAction>();
        var index = 0;

        // stay/capture
        if (other == null || other == unit) {
            if (game.TryGetBuilding(position, out var building) && Rules.CanCapture(unit, building))
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
        if (!Rules.IsArtillery(unit) || path.positions.Count == 1)
            foreach (var otherPosition in game.AttackPositions(position, Rules.AttackRange(unit)))
                if (game.TryGetUnit(otherPosition, out other))
                    for (var weapon = 0; weapon < Rules.WeaponsCount(unit); weapon++)
                        if (Rules.CanAttack(unit, other, path.positions, weapon))
                            actions.Add(new UnitAction(UnitActionType.Attack, unit, path, other, null, weapon));

        // supply
        foreach (var offset in Rules.offsets)
            if (game.TryGetUnit(position + offset, out other) && Rules.CanSupply(unit, other))
                actions.Add(new UnitAction(UnitActionType.Supply, unit, path, other));

        // drop out
        foreach (var cargo in unit.cargo)
        foreach (var offset in Rules.offsets) {
            var targetPosition = position + offset;
            if (!game.TryGetUnit(targetPosition, out other) && Rules.CanStay(cargo, targetPosition))
                actions.Add(new UnitAction(UnitActionType.DropOut, unit, path, targetUnit: cargo, targetPosition: targetPosition));
        }

        while (true) {
            yield return null;

            if (game.CurrentPlayer.IsAi || Input.GetKeyDown(KeyCode.Space) && actions.Count > 0) {

                var action = game.CurrentPlayer.IsAi ? game.CurrentPlayer.bestAction : actions[index];
                yield return action.Execute();

                foreach (var item in actions)
                    item.Dispose();

                var won = Rules.Won(game.realPlayer);
                var lost = Rules.Lost(game.realPlayer);

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

            else if (Input.GetMouseButtonDown(Mouse.right)) {

                foreach (var action in actions)
                    action.Dispose();

                unit.view.Position = path.positions[0];
                unit.view.Forward = startForward;

                yield return PathSelectionState.New(game);
                yield break;
            }

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                if (actions.Count > 0) {
                    index = (index + 1) % actions.Count;
                    Debug.Log(actions[index]);
                }
                else
                    UiSound.Instance.notAllowed.Play();
            }

            else if (Input.GetKeyDown(KeyCode.Return) && actions.Count == 0)
                UiSound.Instance.notAllowed.Play();
        }
    }
}