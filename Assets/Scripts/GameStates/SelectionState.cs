using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public static class SelectionState {

    public const string prefix = "selection-state.";

    public const string endTurn = prefix + "end-turn";
    public const string openGameMenu = prefix + "open-game-menu";
    public const string exitToLevelEditor = prefix + "exit-to-level-editor";
    public const string cyclePositions = prefix + "cycle-positions";
    public const string select = prefix + "select";
    public const string triggerVictory = prefix + "trigger-victory";
    public const string triggerDefeat = prefix + "trigger-defeat";
    public const string useAbility = prefix + "use-ability";

    public static IEnumerator<StateChange> Run(Main main, bool turnStart = false) {

        // stop the ability
        var player = main.CurrentPlayer;
        if (player.abilityActivationTurn != null && main.turn != player.abilityActivationTurn)
            yield return StateChange.Push(nameof(StopAbility), Wait.ForCompletion(StopAbility(player)));

        // weird static variable issue
        PlayerView.globalVisibility = true;

        // 1 frame skip to let units' views to update to correct positions
        // yield return null;

        var unmovedUnits = main.units.Values
            .Where(unit => unit.Player == player && !unit.Moved)
            .ToList();

        var accessibleBuildings = main.buildings.Values
            .Where(building => building.Player == player &&
                               Rules.GetBuildableUnitTypes(building).Any() &&
                               !main.TryGetUnit(building.position, out _))
            .ToList();

        Sprite GetUnitThumbnail(Unit unit) {
            return unit.Player.co.unitTypesInfoOverride.TryGetValue(unit.type, out var @record) && record.thumbnail ||
                   UnitTypesInfo.TryGet(unit.type, out record) && record.thumbnail
                ? record.thumbnail
                : null;
        }
        // make thumbnails for buildings as well

        var positions = unmovedUnits.Select(unit => (
                priority: 1,
                coordinates: ((Vector2Int)unit.Position).Raycast(),
                thumbnail: GetUnitThumbnail(unit)))
            .Concat(accessibleBuildings.Select(building => (
                priority: 0,
                coordinates: building.position.Raycast(),
                thumbnail: (Sprite)null)))
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

            yield return main.levelLogic.OnTurnStart(main);

            //MusicPlayer.Instance.Queue = game.CurrentPlayer.co.themes.InfiniteSequence(game.settings.shuffleMusic);

            Debug.Log($"Start of turn #{main.turn}");

            player.view.visible = true;
        }

        CursorView.TryFind(out var cursor);
        if (cursor)
            cursor.show = true;

        main.MarkAsReadyForInput();
        
        while (true) {
            yield return StateChange.none;

            if (!player.IsAi) {

                if (Input.GetKeyDown(KeyCode.F2))
                    main.commands.Enqueue(endTurn);

                //else if ((Input.GetKeyDown(KeyCode.Escape)) && (!preselectionCursor || !preselectionCursor.Visible))
                //    main.commands.Enqueue(openGameMenu);

                else if (Input.GetKeyDown(KeyCode.F5) && main.inLevelEditor)
                    main.commands.Enqueue(exitToLevelEditor);

                else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right)) && preselectionCursor && preselectionCursor.Visible)
                    preselectionCursor.Hide();

                else if (Input.GetKeyDown(KeyCode.Tab)) {
                    main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                    main.commands.Enqueue(cyclePositions);
                }

                else if (Input.GetKeyDown(KeyCode.Space) && preselectionCursor.Visible) {

                    main.stack.Push(preselectionCursor.transform.position.ToVector2().RoundToInt());
                    main.commands.Enqueue(@select);
                }

                else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) &&
                         Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                    main.stack.Push(mousePosition);
                    main.commands.Enqueue(@select);
                }

                else if (Input.GetKeyDown(KeyCode.F6) && Rules.CanUseAbility(main.CurrentPlayer))
                    main.commands.Enqueue(useAbility);
            }

            while (main.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input)) {
                    switch (token) {

                        case @select: {

                            var position = main.stack.Pop<Vector2Int>();
                            var position3d = position.Raycast();
                            
                            var camera = Camera.main;
                            if (camera && cameraRig && preselectionCursor && !preselectionCursor.VisibleOnTheScreen(camera, position3d)) {
                                Debug.DrawLine(position3d, position3d + Vector3.up, Color.yellow, 3);
                                cameraRig.Jump(position3d);
                            }

                            if (main.TryGetUnit(position, out var unit)) {
                                if (unit.Player != player || unit.Moved)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    unit.view.Selected = true;
                                    if (preselectionCursor)
                                        preselectionCursor.Hide();
                                    yield return StateChange.ReplaceWith(nameof(PathSelectionState), PathSelectionState.Run(main, unit));
                                }
                            }

                            else if (main.TryGetBuilding(position, out var building) &&
                                     Rules.GetBuildableUnitTypes(building).Any()) {
                                if (building.Player != player)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    if (preselectionCursor)
                                        preselectionCursor.Hide();
                                    yield return StateChange.ReplaceWith(nameof(UnitBuildState), UnitBuildState.New(main, building));
                                }
                            }
                            break;
                        }

                        case endTurn: {

                            foreach (var unit in main.units.Values)
                                unit.Moved = false;

                            player.view.visible = false;
                            if (cursor)
                                cursor.show = false;
                            if (preselectionCursor)
                                preselectionCursor.Hide();

                            //MusicPlayer.Instance.source.Stop();
                            //MusicPlayer.Instance.queue = null;

                            main.turn = main.turn + 1;

                            yield return main.levelLogic.OnTurnEnd(main);
                            yield return StateChange.ReplaceWith(nameof(SelectionState), Run(main, true));
                            break;
                        }

                        case openGameMenu:
                            yield return StateChange.ReplaceWith(nameof(GameMenuState), GameMenuState.Run(main));
                            break;

                        case exitToLevelEditor:
                            yield return StateChange.Pop();
                            break;

                        case cyclePositions: {
                            var offset = main.stack.Pop<int>();
                            if (positions.Length > 0) {
                                positionIndex = (positionIndex + offset).PositiveModulo(positions.Length);
                                if (preselectionCursor) {
                                    var position = positions[positionIndex];
                                    if (preselectionCursor)
                                        preselectionCursor.ShowAt(position.coordinates, position.thumbnail);

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
                                yield return StateChange.Push(nameof(StartAbility), Wait.ForCompletion(StartAbility(player, main.turn)));
                            else
                                UiSound.Instance.notAllowed.PlayOneShot();
                            break;
                        }

                        case triggerVictory:
                            if (preselectionCursor)
                                preselectionCursor.Hide();
                            yield return StateChange.ReplaceWith(nameof(VictoryState), VictoryState.Run(main, null));
                            break;

                        case triggerDefeat:
                            if (preselectionCursor)
                                preselectionCursor.Hide();
                            yield return StateChange.ReplaceWith(nameof(DefeatState), DefeatState.Run(main, null));
                            break;

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
        player.AbilityMeter = 0;

        Debug.Log($"starting ability of {player}");

        yield return null;
    }

    public static IEnumerator StopAbility(Player player) {
        Assert.IsTrue(Rules.AbilityInUse(player));

        player.abilityActivationTurn = null;
        Debug.Log($"stopping ability of {player}");

        yield return null;
    }
}