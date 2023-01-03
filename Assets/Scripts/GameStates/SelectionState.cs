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
    
    public static IEnumerator New(Main main, bool turnStart = false) {

        // weird static variable issue
        PlayerView.globalVisibility = true;
        
        var unmovedUnits = main.units.Values
            .Where(unit => unit.player == main.CurrentPlayer && !unit.moved.v)
            .ToList();
        
        var accessibleBuildings = main.buildings.Values
            .Where(building => building.player.v == main.CurrentPlayer &&
                               Rules.BuildableUnits(building) != 0 &&
                               !main.TryGetUnit(building.position, out _))
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

            var (controlFlow, nextState) = main.levelLogic.OnTurnStart(main);
            if (nextState != null)
                yield return nextState;
            if (controlFlow == ControlFlow.Replace)
                yield break;

            //MusicPlayer.Instance.Queue = game.CurrentPlayer.co.themes.InfiniteSequence(game.settings.shuffleMusic);

            yield return TurnStartAnimationState.New(main);

            main.CurrentPlayer.view.visible = true;
        }

        CursorView.Instance.Visible = true;

        while (true) {
            yield return null;

            if (main.input.selectAt is { } position) {
                main.input.selectAt = null;
                
                if (main.TryGetUnit(position, out var unit)) {
                    unit.view.Selected = true;
                    yield return PathSelectionState.New(main,unit);
                    yield break;
                }
                else if (main.TryGetBuilding(position, out var building)) {
                    yield return UnitBuildingState.New(main,building);
                    yield break;
                }   
            }

            // end turn
            else if (main.input.endTurn) {

                main.input.Reset();

                foreach (var unit in main.units.Values)
                    unit.moved.v = false;

                main.CurrentPlayer.view.visible = false;
                CursorView.Instance.Visible = false;

                //MusicPlayer.Instance.source.Stop();
                //MusicPlayer.Instance.queue = null;

                Assert.IsTrue(main.turn != null);
                main.turn = (int)main.turn + 1;

                var (controlFlow, nextState) = main.levelLogic.OnTurnEnd(main);
                if (nextState != null)
                    yield return nextState;
                if (controlFlow == ControlFlow.Replace)
                    yield break;

                yield return New(main, true);
                yield break;
            }

            if (main.CurrentPlayer.IsAi)
                continue;

            if (Input.GetKeyDown(KeyCode.F2))
                main.input.endTurn = true;
            
            else if (Input.GetKeyDown(KeyCode.Escape))
                yield return GameMenuState.New(main);

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

                if (main.TryGetUnit(mousePosition, out var unit)) {
                    if (unit.player != main.CurrentPlayer || unit.moved.v)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else
                        main.input.selectAt = mousePosition;
                }

                else if (main.TryGetBuilding(mousePosition, out var building)) {
                    if (building.player.v != main.CurrentPlayer)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else
                        main.input.selectAt = mousePosition;
                }
            }
            
            else if (triggerVictory) {
                triggerVictory = false;
                yield return VictoryState.New(main);
                yield break;
            }
            else if (triggerDefeat) {
                triggerDefeat = false;
                yield return DefeatState.New(main);
                yield break;
            }
        }
    }
}