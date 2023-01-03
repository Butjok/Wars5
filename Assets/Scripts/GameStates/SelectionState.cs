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
    
    public static IEnumerator New(Level level, bool turnStart = false) {

        // weird static variable issue
        PlayerView.globalVisibility = true;
        
        var unmovedUnits = level.units.Values
            .Where(unit => unit.player == level.CurrentPlayer && !unit.moved.v)
            .ToList();
        
        var accessibleBuildings = level.buildings.Values
            .Where(building => building.player.v == level.CurrentPlayer &&
                               Rules.BuildableUnits(building) != 0 &&
                               !level.TryGetUnit(building.position, out _))
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

            var (controlFlow, nextState) = level.levelLogic.OnTurnStart(level);
            if (nextState != null)
                yield return nextState;
            if (controlFlow == ControlFlow.Replace)
                yield break;

            //MusicPlayer.Instance.Queue = game.CurrentPlayer.co.themes.InfiniteSequence(game.settings.shuffleMusic);

            yield return TurnStartAnimationState.New(level);

            level.CurrentPlayer.view.visible = true;
        }

        CursorView.Instance.Visible = true;

        while (true) {
            yield return null;

            if (level.input.selectAt is { } position) {
                level.input.selectAt = null;
                
                if (level.TryGetUnit(position, out var unit)) {
                    unit.view.Selected = true;
                    yield return PathSelectionState.New(level,unit);
                    yield break;
                }
                else if (level.TryGetBuilding(position, out var building)) {
                    yield return UnitBuildingState.New(level,building);
                    yield break;
                }   
            }

            // end turn
            else if (level.input.endTurn) {

                level.input.Reset();

                foreach (var unit in level.units.Values)
                    unit.moved.v = false;

                level.CurrentPlayer.view.visible = false;
                CursorView.Instance.Visible = false;

                //MusicPlayer.Instance.source.Stop();
                //MusicPlayer.Instance.queue = null;

                Assert.IsTrue(level.turn != null);
                level.turn = (int)level.turn + 1;

                var (controlFlow, nextState) = level.levelLogic.OnTurnEnd(level);
                if (nextState != null)
                    yield return nextState;
                if (controlFlow == ControlFlow.Replace)
                    yield break;

                yield return New(level, true);
                yield break;
            }

            if (level.CurrentPlayer.IsAi)
                continue;

            if (Input.GetKeyDown(KeyCode.F2))
                level.input.endTurn = true;
            
            else if (Input.GetKeyDown(KeyCode.Escape))
                yield return GameMenuState.New(level);

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

                if (level.TryGetUnit(mousePosition, out var unit)) {
                    if (unit.player != level.CurrentPlayer || unit.moved.v)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else
                        level.input.selectAt = mousePosition;
                }

                else if (level.TryGetBuilding(mousePosition, out var building)) {
                    if (building.player.v != level.CurrentPlayer)
                        UiSound.Instance.notAllowed.PlayOneShot();
                    else
                        level.input.selectAt = mousePosition;
                }
            }
            
            else if (triggerVictory) {
                triggerVictory = false;
                yield return VictoryState.New(level);
                yield break;
            }
            else if (triggerDefeat) {
                triggerDefeat = false;
                yield return DefeatState.New(level);
                yield break;
            }
        }
    }
}