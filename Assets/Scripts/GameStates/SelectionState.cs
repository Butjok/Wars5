using System.Collections;
using System.Linq;
using DG.Tweening;
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
    public const string useAbility = prefix + "use-ability";

    public static IEnumerator Run(Main main, bool turnStart = false) {

        // stop the ability
        var player = main.CurrentPlayer;
        if (player.abilityActivationTurn != null && main.turn != player.abilityActivationTurn)
            yield return StopAbility(player);

        // weird static variable issue
        PlayerView.globalVisibility = true;

        // 1 frame skip to let units' views to update to correct positions
        // yield return null;

        var unmovedUnits = main.units.Values
            .Where(unit => unit.player == player && !unit.moved.v)
            .ToList();

        var accessibleBuildings = main.buildings.Values
            .Where(building => building.player.v == player &&
                               Rules.BuildableUnits(building) != 0 &&
                               !main.TryGetUnit(building.position, out _))
            .ToList();

        var positions = unmovedUnits.Select(unit => (priority: 1, coordinates: ((Vector2Int)unit.position.v).Raycast()))
            .Concat(accessibleBuildings.Select(building => (priority: 0, coordinates: building.position.Raycast())))
            .ToArray();

        CameraRig.TryFind(out var cameraRig);
        if (cameraRig)
            positions = positions
                .OrderByDescending(position => position.priority)
                .ThenBy(position => Vector3.Distance(cameraRig.transform.position, position.coordinates)).ToArray();

        var positionIndex = -1;

        PreselectionCursor.TryFind(out var preselectionCursor);
        if (preselectionCursor)
            preselectionCursor.Hide();

        if (turnStart) {

            var (controlFlow, nextState) = main.levelLogic.OnTurnStart(main);
            if (nextState != null)
                yield return nextState;
            if (controlFlow == ControlFlow.Replace)
                yield break;

            //MusicPlayer.Instance.Queue = game.CurrentPlayer.co.themes.InfiniteSequence(game.settings.shuffleMusic);

            yield return TurnStartAnimationState.Run(main);

            player.view.visible = true;
        }

        CursorView.TryFind(out var cursor);
        if (cursor)
            cursor.Visible = true;

        while (true) {
            yield return null;

            if (!player.IsAi) {

                if (Input.GetKeyDown(KeyCode.F2))
                    main.commands.Enqueue(endTurn);

                else if ((Input.GetKeyDown(KeyCode.Escape)) && (!preselectionCursor || !preselectionCursor.Visible))
                    main.commands.Enqueue(openGameMenu);

                else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right)) && preselectionCursor && preselectionCursor.Visible)
                    preselectionCursor.Hide();

                else if (Input.GetKeyDown(KeyCode.Tab))
                    main.commands.Enqueue(cyclePositions);

                else if (Input.GetKeyDown(KeyCode.Space) && preselectionCursor.Visible) {

                    main.stack.Push(preselectionCursor.transform.position);
                    main.commands.Enqueue(@select);
                }

                else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) &&
                         Mouse.TryGetPosition(out Vector3 mousePosition)) {

                    main.stack.Push(mousePosition);
                    main.commands.Enqueue(@select);
                }

                else if (Input.GetKeyDown(KeyCode.F6))
                    main.commands.Enqueue(useAbility);
            }

            while (main.commands.TryDequeue(out var input))
                foreach (var token in input.Tokenize()) {
                    switch (token) {

                        case @select: {

                            var position = main.stack.Pop<Vector3>();

                            var camera = Camera.main;
                            if (camera && cameraRig && preselectionCursor && !preselectionCursor.VisibleOnTheScreen(camera, position)) {
                                Debug.DrawLine(position, position + Vector3.up, Color.yellow, 3);
                                cameraRig.Jump(position);
                            }

                            if (main.TryGetUnit(position.ToVector2().RoundToInt(), out var unit)) {
                                if (unit.player != player || unit.moved.v)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    unit.view.Selected = true;
                                    if (preselectionCursor)
                                        preselectionCursor.Hide();
                                    yield return PathSelectionState.Run(main, unit);
                                    yield break;
                                }
                            }

                            else if (main.TryGetBuilding(position.ToVector2().RoundToInt(), out var building)) {
                                if (building.player.v != player)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    if (preselectionCursor)
                                        preselectionCursor.Hide();
                                    yield return UnitBuildState.New(main, building);
                                    yield break;
                                }
                            }
                            break;
                        }

                        case endTurn: {

                            foreach (var unit in main.units.Values)
                                unit.moved.v = false;

                            player.view.visible = false;
                            if (cursor)
                                cursor.Visible = false;
                            if (preselectionCursor)
                                preselectionCursor.Hide();

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
                                if (preselectionCursor) {
                                    var position = positions[positionIndex];
                                    if (preselectionCursor)
                                        preselectionCursor.ShowAt(position.coordinates);
                                    var mainCamera = Camera.main;
                                    if (mainCamera) {
                                        // var screenPosition = mainCamera.WorldToViewportPoint(position.ToVector3Int());
                                        // if (!new Rect(0, 0, 1, 1).Contains(screenPosition) && cameraRig)
                                        //     cameraRig.Jump(position);
                                    }
                                }
                            }
                            else if (preselectionCursor)
                                preselectionCursor.Hide();
                            break;
                        }

                        case useAbility: {
                            if (Rules.CanUseAbility(player))
                                yield return StartAbility(player, (int)main.turn);
                            else
                                UiSound.Instance.notAllowed.PlayOneShot();
                            break;
                        }

                        case triggerVictory:
                            if (preselectionCursor)
                                preselectionCursor.Hide();
                            yield return VictoryState.Run(main, null);
                            yield break;

                        case triggerDefeat:
                            if (preselectionCursor)
                                preselectionCursor.Hide();
                            yield return DefeatState.Run(main, null);
                            yield break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
                }
        }
    }

    public static IEnumerator StartAbility(Player player, int turn) {
        Assert.IsTrue(Rules.CanUseAbility(player));
        Assert.IsTrue(!Rules.AbilityInUse(player));

        player.abilityActivationTurn = turn;
        Debug.Log($"starting ability of {player}");

        yield break;
    }

    public static IEnumerator StopAbility(Player player) {
        Assert.IsTrue(Rules.AbilityInUse(player));

        player.abilityActivationTurn = null;
        Debug.Log($"stopping ability of {player}");

        yield break;
    }
}