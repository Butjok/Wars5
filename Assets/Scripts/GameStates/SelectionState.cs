using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class SelectionState {

    public static IEnumerator New(Game game, bool turnStart = false) {

        var unmovedUnits = game.units.Values
            .Where(unit => unit.player == game.CurrentPlayer && !unit.moved.v)
            .OrderBy(unit => Vector3.Distance(CameraRig.Instance.transform.position, unit.view.center.position))
            .ToList();

        var unitIndex = -1;
        Unit cycledUnit = null;

        var musicPlayer = MusicPlayer.Instance;

        if (turnStart) {

            var (controlFlow, nextState) = game.levelLogic.OnTurnStart(game);
            if (nextState != null)
                yield return nextState;
            if (controlFlow == ControlFlow.Replace)
                yield break;

            musicPlayer.Queue = game.CurrentPlayer.co.themes.InfiniteSequence(game.settings.shuffleMusic);

            yield return TurnStartAnimationState.New(game);

            game.CurrentPlayer.view.Visible = true;
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

                game.CurrentPlayer.view.Visible = false;
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

            // track cycles (with tab) unit on the screen
            if (cycledUnit != null && Camera.main) {
                var worldPosition = cycledUnit.view.center.position;
                var screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
                if (screenPosition.x is < 0 or > 1 || screenPosition.y is < 0 or > 1)
                    cycledUnit = null;
            }

            if (Input.GetKeyDown(KeyCode.F2))
                game.input.endTurn = true;

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                if (unmovedUnits.Count > 0) {
                    unitIndex = (unitIndex + 1) % unmovedUnits.Count;
                    var next = unmovedUnits[unitIndex];
                    CameraRig.Instance.Jump(next.view.center.position.ToVector2());
                    cycledUnit = next;
                }
                else
                    UiSound.Instance.notAllowed.Play();
            }

            else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) &&
                     Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                if (game.TryGetUnit(mousePosition, out var unit)) {
                    if (unit.player != game.CurrentPlayer || unit.moved.v)
                        UiSound.Instance.notAllowed.Play();
                    else
                        game.input.selectAt = mousePosition;
                }

                else if (game.TryGetBuilding(mousePosition, out var building)) {
                    if (building.player.v != game.CurrentPlayer)
                        UiSound.Instance.notAllowed.Play();
                    else
                        game.input.selectAt = mousePosition;
                }
            }

            else if (Input.GetKeyDown(KeyCode.Space))
                if (cycledUnit != null)
                    game.input.selectAt = cycledUnit.position.v;
                else
                    UiSound.Instance.notAllowed.Play();

            else if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftShift)) {
                CursorView.Instance.Visible = false;
                yield return VictoryState.New(game);
                yield break;
            }

            else if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift)) {
                CursorView.Instance.Visible = false;
                yield return DefeatState.New(game);
                yield break;
            }
        }
    }
}