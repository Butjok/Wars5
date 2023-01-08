using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class SelectionState {

    public const string prefix = "selection-state.";

    public const string endTurn = prefix + "end-turn";
    public const string openGameMenu = prefix + "open-game-menu";
    public const string cyclePositions = prefix + "cycle-positions";
    public const string select = prefix + "select";
    public const string triggerVictory = prefix + "trigger-victory";
    public const string triggerDefeat = prefix + "trigger-defeat";

    public static IEnumerator Run(Main main, bool turnStart = false) {

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

            yield return TurnStartAnimationState.Run(main);

            main.CurrentPlayer.view.visible = true;
        }

        CursorView.TryFind(out var cursor);
        if (cursor)
            cursor.Visible = true;

        HighlightingCursorView.TryFind(out var highlightingCursor);

        while (true) {
            yield return null;

            if (!main.CurrentPlayer.IsAi) {

                if (Input.GetKeyDown(KeyCode.F2))
                    main.commands.Enqueue(endTurn);

                else if (Input.GetKeyDown(KeyCode.Escape))
                    main.commands.Enqueue(openGameMenu);

                else if (Input.GetKeyDown(KeyCode.Tab))
                    main.commands.Enqueue(cyclePositions);

                else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) &&
                         Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                    main.stack.Push(mousePosition);
                    main.commands.Enqueue(@select);
                }
            }

            while (main.commands.TryDequeue(out var input))
                foreach (var token in input.Tokenize()) {
                    switch (token) {

                        case @select: {

                            var position = main.stack.Pop<Vector2Int>();

                            if (main.TryGetUnit(position, out var unit)) {
                                if (unit.player != main.CurrentPlayer || unit.moved.v)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    unit.view.Selected = true;
                                    yield return PathSelectionState.Run(main, unit);
                                    yield break;
                                }
                            }

                            else if (main.TryGetBuilding(position, out var building)) {
                                if (building.player.v != main.CurrentPlayer)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    yield return UnitBuildState.New(main, building);
                                    yield break;
                                }
                            }
                            break;
                        }

                        case endTurn: {

                            foreach (var unit in main.units.Values)
                                unit.moved.v = false;

                            main.CurrentPlayer.view.visible = false;
                            if (cursor)
                                cursor.Visible = false;
                            if (highlightingCursor)
                                highlightingCursor.Hide();

                            //MusicPlayer.Instance.source.Stop();
                            //MusicPlayer.Instance.queue = null;

                            Assert.IsTrue(main.turn != null);
                            main.turn = (int)main.turn + 1;

                            var (controlFlow, nextState) = main.levelLogic.OnTurnEnd(main);
                            if (nextState != null)
                                yield return nextState;
                            if (controlFlow == ControlFlow.Replace)
                                yield break;

                            yield return Run(main, true);
                            yield break;
                        }

                        case openGameMenu:
                            yield return GameMenuState.Run(main);
                            break;

                        case cyclePositions: {
                            if (positions.Length > 0) {
                                positionIndex = (positionIndex + 1) % positions.Length;
                                if (highlightingCursor)
                                highlightingCursor.ShowAt(positions[positionIndex]);
                            }
                            else if (highlightingCursor)
                            highlightingCursor.Hide();
                            break;
                        }

                        case triggerVictory:
                            yield return VictoryState.Run(main, null);
                            yield break;

                        case triggerDefeat:
                            yield return DefeatState.Run(main, null);
                            yield break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
                }
        }
    }
}