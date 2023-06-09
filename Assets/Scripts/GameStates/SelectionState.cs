using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectionState : StateMachine.State {

    public enum Command { EndTurn, OpenGameMenu, ExitToLevelEditor, CyclePositions, Select, TriggerVictory, TriggerDefeat, UseAbility }

    public Unit unit;
    public Building building;

    public SelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var game = stateMachine.TryFind<GameSessionState>()?.game;
            var level = stateMachine.TryFind<PlayState>()?.level;
            Assert.IsNotNull(game);
            Assert.IsNotNull(level);

            var cameraRig = level.view.cameraRig;
            var cursor = level.view.cursorView;

            // stop the ability
            var player = level.CurrentPlayer;
            if (player.abilityActivationTurn != null && level.turn != player.abilityActivationTurn) {
                var enumerator = StopAbility(player);
                while (enumerator.MoveNext())
                    yield return StateChange.none;
            }

            // weird static variable issue
            PlayerView.globalVisibility = true;

            // 1 frame skip to let units' views to update to correct positions
            // yield return null;

            var unmovedUnits = level.units.Values
                .Where(unit => unit.Player == player && !unit.Moved)
                .ToList();

            var accessibleBuildings = level.buildings.Values
                .Where(building => building.Player == player &&
                                   Rules.GetBuildableUnitTypes(building).Any() &&
                                   !level.TryGetUnit(building.position, out _))
                .ToList();

            Sprite GetUnitThumbnail(Unit unit) {
                return null;
                // return unit.Player.co.unitTypesInfoOverride.TryGetValue(unit.type, out var @record) && record.thumbnail ||
                //        UnitTypesInfo.TryGet(unit.type, out record) && record.thumbnail
                //     ? record.thumbnail
                //     : null;
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

            positions = positions
                .OrderByDescending(position => position.priority)
                .ThenBy(position => Vector3.Distance(cameraRig.transform.position, position.coordinates)).ToArray();

            var positionIndex = -1;

            PreselectionCursor.TryFind(out var preselectionCursor);
            if (preselectionCursor)
                preselectionCursor.Hide();

            if (!game.autoplay && cursor)
                cursor.show = true;

            /*var turnButton = Object.FindObjectOfType<TurnButton>();
            if (turnButton) {
                turnButton.Color = player.Color;
                if (turnStart)
                    turnButton.PlayAnimation(level.Day(level.turn) + 1);
            }*/

            var issuedAiCommands = false;
            while (true) {
                yield return StateChange.none;

                if (game.autoplay) {
                    if (!issuedAiCommands) {
                        issuedAiCommands = true;
                        game.aiPlayerCommander.IssueCommandsForSelectionState();
                    }
                }
                else if (!level.CurrentPlayer.IsAi) {

                    if (Input.GetKeyDown(KeyCode.F2))
                        game.EnqueueCommand(Command.EndTurn);

                    //else if ((Input.GetKeyDown(KeyCode.Escape)) && (!preselectionCursor || !preselectionCursor.Visible))
                    //    main.commands.Enqueue(openGameMenu);

                    else if (Input.GetKeyDown(KeyCode.F5))
                        game.EnqueueCommand(Command.ExitToLevelEditor);

                    else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right)) && preselectionCursor && preselectionCursor.Visible)
                        preselectionCursor.Hide();

                    else if (Input.GetKeyDown(KeyCode.Tab))
                        game.EnqueueCommand(Command.CyclePositions, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                    else if (Input.GetKeyDown(KeyCode.Space) && preselectionCursor.Visible)
                        game.EnqueueCommand(Command.Select, preselectionCursor.transform.position.ToVector2().RoundToInt());

                    else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) && level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition))
                        game.EnqueueCommand(Command.Select, mousePosition);

                    else if (Input.GetKeyDown(KeyCode.F6) && Rules.CanUseAbility(level.CurrentPlayer))
                        game.EnqueueCommand(Command.UseAbility);
                }

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Select, Vector2Int position): {

                            var position3d = position.Raycast();

//                            Debug.Log($"selecting unit at {position}");

                            var camera = Camera.main;
                            /*if (camera && cameraRig && preselectionCursor && !preselectionCursor.VisibleOnTheScreen(camera, position3d)) {
                                Debug.DrawLine(position3d, position3d + Vector3.up, Color.yellow, 3);
                                cameraRig.Jump(position3d);
                            }*/

                            if (level.TryGetUnit(position, out unit)) {
                                if (unit.Player != player || unit.Moved)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    if (preselectionCursor)
                                        preselectionCursor.Hide();
                                    yield return StateChange.Push(new PathSelectionState(stateMachine));
                                }
                            }

                            else if (level.TryGetBuilding(position, out building) && Rules.GetBuildableUnitTypes(building).Any()) {
                                if (building.Player != player)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    if (preselectionCursor)
                                        preselectionCursor.Hide();
                                    yield return StateChange.Push(new UnitBuildState(stateMachine));
                                }
                            }
                            break;
                        }

                        case (Command.EndTurn, _): {

                            foreach (var unit in level.units.Values)
                                unit.Moved = false;

                            player.view.visible = false;
                            if (cursor)
                                cursor.show = false;
                            if (preselectionCursor)
                                preselectionCursor.Hide();

                            //MusicPlayer.Instance.source.Stop();
                            //MusicPlayer.Instance.queue = null;

                            var oldDay = level.Day();
                            level.turn++;
                            if (level.Day() != oldDay)
                                yield return StateChange.Push(new DayChangeState(stateMachine));
                            yield return StateChange.PopThenPush(2, new PlayerTurnState(stateMachine));
                            break;
                        }

                        case (Command.OpenGameMenu, _):
                            yield return StateChange.Push(new GameMenuState(stateMachine));
                            break;

                        case (Command.ExitToLevelEditor, _):
                            if (stateMachine.TryFind<PlayState>() != null)
                                yield return StateChange.Pop();
                            break;

                        case (Command.CyclePositions, int offset): {
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

                        case (Command.UseAbility, _): {
                            if (Rules.CanUseAbility(player)) {
                                var enumerator = StartAbility(player, level.turn);
                                while (enumerator.MoveNext())
                                    yield return StateChange.none;
                            }
                            else
                                UiSound.Instance.notAllowed.PlayOneShot();
                            break;
                        }

                        case (Command.TriggerVictory, _):
                            if (preselectionCursor)
                                preselectionCursor.Hide();
                            yield return StateChange.ReplaceWith(new VictoryState(stateMachine));
                            break;

                        case (Command.TriggerDefeat, _):
                            if (preselectionCursor)
                                preselectionCursor.Hide();
                            yield return StateChange.ReplaceWith(new DefeatState(stateMachine));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
        }
    }

    public static IEnumerator StartAbility(Player player, int turn) {
        Assert.IsTrue(Rules.CanUseAbility(player));
        Assert.IsTrue(!Rules.AbilityInUse(player));

        player.abilityActivationTurn = turn;
        player.SetAbilityMeter(0);

        Debug.Log($"starting ability of {player}");

        yield return null;
    }

    public static IEnumerator StopAbility(Player player) {


        yield return null;
    }
};