using System.Collections;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public static class SelectionState {

    [Command]
    public static bool triggerVictory;
    [Command]
    public static bool triggerDefeat;
    
    public static IEnumerator New(Game game, bool turnStart = false) {

        var unmovedUnits = game.units.Values
            .Where(unit => unit.player == game.CurrentPlayer && !unit.moved.v)
            .ToList();
        
        var accessibleBuildings = game.buildings.Values
            .Where(building => building.player.v == game.CurrentPlayer &&
                               Rules.BuildableUnits(building) != 0 &&
                               !game.TryGetUnit(building.position, out _))
            .ToList();

        var positions = unmovedUnits.Select(unit => {
                Assert.IsTrue(unit.position.v != null);
                return (Vector2Int)unit.position.v;
            })
            .Union(accessibleBuildings.Select(building => building.position))
            .ToArray();
        
        if (CameraRig.Instance)
            positions = positions.OrderBy(position => Vector2.Distance(CameraRig.Instance.transform.position.ToVector2(), position)).ToArray();

        var positionIndex = -1;

        if (turnStart) {

            var (controlFlow, nextState) = game.levelLogic.OnTurnStart(game);
            if (nextState != null)
                yield return nextState;
            if (controlFlow == ControlFlow.Replace)
                yield break;

            //MusicPlayer.Instance.Queue = game.CurrentPlayer.co.themes.InfiniteSequence(game.settings.shuffleMusic);

            yield return TurnStartAnimationState.New(game);

            game.CurrentPlayer.view.visible = true;
        }

        CursorView.Instance.Visible = true;

        while (true) {
            yield return null;

            if (game.input.selectAt is { } position) {
                game.input.selectAt = null;
                
                if (game.TryGetUnit(position, out var unit)) {
                    unit.view.Selected = true;
                    yield return PathSelectionState.New(game,unit);
                    yield break;
                }
                else if (game.TryGetBuilding(position, out var building)) {
                    yield return UnitBuildingState.New(game,building);
                    yield break;
                }   
            }

            // end turn
            else if (game.input.endTurn) {

                game.input.Reset();

                foreach (var unit in game.units.Values)
                    unit.moved.v = false;

                game.CurrentPlayer.view.visible = false;
                CursorView.Instance.Visible = false;

                //MusicPlayer.Instance.source.Stop();
                //MusicPlayer.Instance.queue = null;

                Assert.IsTrue(game.turn != null);
                game.turn = (int)game.turn + 1;

                var (controlFlow, nextState) = game.levelLogic.OnTurnEnd(game);
                if (nextState != null)
                    yield return nextState;
                if (controlFlow == ControlFlow.Replace)
                    yield break;

                yield return New(game, true);
                yield break;
            }

            if (game.CurrentPlayer.IsAi)
                continue;

            if (Input.GetKeyDown(KeyCode.F2))
                game.input.endTurn = true;
            
            else if (Input.GetKeyDown(KeyCode.Escape))
                yield return GameMenuState.New(game);

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                if (positions.Length > 0) {
                    positionIndex = (positionIndex + 1) % positions.Length;
                    if (CameraRig.Instance)
                        CameraRig.Instance.Jump(positions[positionIndex]);
                }
                else
                    UiSound.Instance.notAllowed.PlayOneShot();
            }

            else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) &&
                     Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                if (game.TryGetUnit(mousePosition, out var unit)) {
                    if (unit.player != game.CurrentPlayer || unit.moved.v)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else
                        game.input.selectAt = mousePosition;
                }

                else if (game.TryGetBuilding(mousePosition, out var building)) {
                    if (building.player.v != game.CurrentPlayer)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else
                        game.input.selectAt = mousePosition;
                }
            }
            
            else if (triggerVictory) {
                triggerVictory = false;
                yield return VictoryState.New(game);
                yield break;
            }
            else if (triggerDefeat) {
                triggerDefeat = false;
                yield return DefeatState.New(game);
                yield break;
            }
        }
    }
}