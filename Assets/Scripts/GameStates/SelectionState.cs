using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectionState : StateMachineState {

    public enum Command {
        EndTurn,
        OpenGameMenu,
        ExitToLevelEditor,
        CyclePositions,
        Select,
        TriggerVictory,
        TriggerDefeat,
        UseAbility,
        OpenMinimap,
        ShowAttackRange
    }

    public Unit unit;
    public Building building;

    public SelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var levelSession = stateMachine.Find<LevelSessionState>();
            var cameraRig = Level.view.cameraRig;

            // stop the ability
            var player = Level.CurrentPlayer;
            if (player.abilityActivationTurn != null && Level.turn != player.abilityActivationTurn) {
                var enumerator = StopAbility(player);
                while (enumerator.MoveNext())
                    yield return StateChange.none;
            }

            // weird static variable issue
            PlayerView.globalVisibility = true;

            // 1 frame skip to let units' views to update to correct positions
            // yield return null;

            var turnButton = Level.view.turnButton;

            void TrySetTurnButtonVisibility(bool visible) {
                if (turnButton)
                    turnButton.Visible = visible;
            }

            void TrySetTurnButtonInteractivity(bool interactable) {
                if (turnButton)
                    turnButton.Interactable = interactable;
            }

            TrySetTurnButtonInteractivity(true);

            var unmovedUnits = Level.units.Values
                .Where(unit => unit.Player == player && !unit.Moved)
                .ToList();

            var accessibleBuildings = Level.buildings.Values
                .Where(building => building.Player == player &&
                                   Rules.GetBuildableUnitTypes(building).Any() &&
                                   !Level.TryGetUnit(building.position, out _))
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
                    coordinates: unit.NonNullPosition.TryRaycast(out var hit) ? hit.point : unit.NonNullPosition.ToVector3(),
                    thumbnail: GetUnitThumbnail(unit)))
                .Concat(accessibleBuildings.Select(building => (
                    priority: 0,
                    coordinates: building.position.TryRaycast(out var hit) ? hit.point : building.position.ToVector3(),
                    thumbnail: (Sprite)null)))
                .ToArray();

            positions = positions
                .OrderByDescending(position => position.priority)
                .ThenBy(position => Vector3.Distance(cameraRig.transform.position, position.coordinates)).ToArray();

            var positionIndex = -1;

            PreselectionCursor.TryFind(out var preselectionCursor);
            if (preselectionCursor)
                preselectionCursor.Hide();

            if (Level.EnableTutorial && Level.name == LevelName.Tutorial) {
                if (Level.tutorialState.startedCapturing && !Level.tutorialState.explainedTurnEnd) {
                    Level.tutorialState.explainedTurnEnd = true;
                    yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.ExplainTurnEnd));
                }
            }

            var missileSilos = Level.Buildings.Where(b => b.Player == Level.CurrentPlayer && b.type == TileType.MissileSilo).ToList();
            foreach (var missileSilo in missileSilos)
                if (missileSilo.Moved && missileSilo.Cooldown(Level.Day()) == 0) {
                    if (Level.CurrentPlayer == Level.localPlayer && missileSilo.position.TryRaycast(out var hit)) {
                        var jumpCompleted = cameraRig.Jump(hit.point);
                        while (!jumpCompleted())
                            yield return StateChange.none;
                    }

                    missileSilo.Moved = false;
                }

            Level.SetGui("missile-silos", () => {
                foreach (var missileSilo in missileSilos)
                    if (missileSilo.position.TryRaycast(out var hit) && Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition) && mousePosition == hit.point.ToVector2Int()) {
                        var cooldown = missileSilo.Cooldown(Level.Day());
                        if (cooldown > 0)
                            WarsGui.CenteredLabel(Level, hit.point, $"Reloading: {cooldown} day(s)");
                    }
            });
            Level.SetGui("keys", () => {
                var text = $"Day {Level.Day() + 1} · [F2] End turn · [M] Minimap";
                var size = GUI.skin.label.CalcSize(new GUIContent(text));
                var padding = DefaultGuiSkin.padding;
                GUI.Label(new Rect(Screen.width - padding.x - size.x, Screen.height - padding.y - size.y, size.x, size.y), text);
            });

            var issuedAiCommands = false;
            while (true) {
                yield return StateChange.none;

                if (levelSession.autoplay) {
                    if (!issuedAiCommands) {
                        issuedAiCommands = true;
                        Game.aiPlayerCommander.IssueCommandsForSelectionState();
                    }
                }
                else {
                    if (Input.GetKeyDown(KeyCode.F2))
                        Game.EnqueueCommand(Command.EndTurn);

                    //else if ((Input.GetKeyDown(KeyCode.Escape)) && (!preselectionCursor || !preselectionCursor.Visible))
                    //    main.commands.Enqueue(openGameMenu);

                    else if (Input.GetKeyDown(KeyCode.F5))
                        Game.EnqueueCommand(Command.ExitToLevelEditor);

                    else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right)) && preselectionCursor && preselectionCursor.Visible)
                        preselectionCursor.Hide();

                    else if (Input.GetKeyDown(KeyCode.Tab))
                        Game.EnqueueCommand(Command.CyclePositions, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                    else if (Input.GetKeyDown(KeyCode.Space) && preselectionCursor.Visible)
                        Game.EnqueueCommand(Command.Select, preselectionCursor.transform.position.ToVector2().RoundToInt());

                    else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) && Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition))
                        Game.EnqueueCommand(Command.Select, mousePosition);

                    else if (Input.GetMouseButtonDown(Mouse.right) && Level.view.cameraRig.camera.TryGetMousePosition(out mousePosition) && Level.TryGetUnit(mousePosition, out var mouseUnit))
                        Game.EnqueueCommand(Command.ShowAttackRange, mouseUnit);

                    else if (Input.GetKeyDown(KeyCode.F6) && Rules.CanUseAbility(Level.CurrentPlayer))
                        Game.EnqueueCommand(Command.UseAbility);

                    else if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.CapsLock))
                        Game.EnqueueCommand(Command.OpenMinimap);

                    else if (Input.GetKeyDown(KeyCode.Escape))
                        Game.EnqueueCommand(Command.OpenGameMenu);

                    else if (Input.GetKey(KeyCode.F7) && !issuedAiCommands) {
                        issuedAiCommands = true;
                        Level.CurrentPlayer.unitBrainController?.MakeMove();
                    }
                }


                while (Game.TryDequeueCommand(out var command)) {
                    // Tutorial logic
                    if (Level.EnableTutorial && Level.name == LevelName.Tutorial)
                        if (!Level.tutorialState.startedCapturing)
                            switch (command) {
                                case (Command.OpenGameMenu or Command.CyclePositions or Command.OpenMinimap or Command.ShowAttackRange or Command.ExitToLevelEditor, _):
                                    break;
                                case (Command.Select, Vector2Int position):
                                    if (Level.TryGetUnit(position, out var unit) && unit.Player == Level.localPlayer && !unit.Moved && unit.type == UnitType.Infantry)
                                        break;
                                    goto default;
                                default:
                                    yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.WrongSelectionPleaseSelectInfantry));
                                    continue;
                            }

                    switch (command) {
                        case (Command.Select, Vector2Int position): {
                            StateMachineState nextState = null;

                            if (Level.TryGetUnit(position, out unit)) {
                                if (unit.Player != player || unit.Moved)
                                    UiSound.Instance.notAllowed.PlayOneShot();
                                else {
                                    if (preselectionCursor)
                                        preselectionCursor.Hide();
                                    yield return StateChange.Push(new PathSelectionState(stateMachine));
                                }
                            }

                            else if (Level.TryGetBuilding(position, out building) && Rules.GetBuildableUnitTypes(building).Any()) {
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
                            foreach (var unit in Level.units.Values)
                                unit.Moved = false;

                            player.view.Hide();
                            if (preselectionCursor)
                                preselectionCursor.Hide();

                            //MusicPlayer.Instance.source.Stop();
                            //MusicPlayer.Instance.queue = null;

                            Level.turn++;
                            yield return StateChange.PopThenPush(2, new PlayerTurnState(stateMachine));
                            break;
                        }

                        case (Command.OpenGameMenu, _):
                            yield return StateChange.Push(new GameMenuState(stateMachine));
                            break;

                        case (Command.ExitToLevelEditor, _): {
                            var levelEditorSessionState = stateMachine.TryFind<LevelEditorSessionState>();
                            if (levelEditorSessionState != null)
                                yield return StateChange.Pop();
                            break;
                        }

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
                                var enumerator = StartAbility(player, Level.turn);
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

                        case (Command.OpenMinimap, _):
                            yield return StateChange.Push(new MinimapState(stateMachine));
                            break;

                        case (Command.ShowAttackRange, Unit mouseUnit): {
                            var attackPositions = new HashSet<Vector2Int>();
                            if (Rules.TryGetAttackRange(mouseUnit, out var attackRange)) {
                                if (Rules.IsIndirect(mouseUnit))
                                    attackPositions.UnionWith(Level.PositionsInRange(mouseUnit.NonNullPosition, attackRange));
                                else if (attackRange == new Vector2Int(1, 1)) {
                                    var pathFinder = new PathFinder();
                                    pathFinder.FindShortPaths(mouseUnit, allowStayOnFriendlyUnits: true);
                                    var movePositions = pathFinder.validShortPathDestinations;
                                    foreach (var position in movePositions)
                                    foreach (var offset in Rules.gridOffsets)
                                        attackPositions.Add(position + offset);
                                }
                            }

                            TileMask.ReplaceGlobal(attackPositions);
                            while (Input.GetMouseButton(Mouse.right))
                                yield return StateChange.none;
                            TileMask.UnsetGlobal();
                            break;
                        }

                        case (BorderIncidentScenario.Command.StartRedRocketeersDialogue, DialogueState dialogueState):
                            yield return StateChange.Push(dialogueState);
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
                }

                UpdateTilemapCursor();
            }
        }
    }

    public override void Exit() {
        var level = stateMachine.Find<LevelSessionState>().level;
        if (level.view.turnButton)
            level.view.turnButton.Interactable = false;
        level.view.tilemapCursor.Hide();

        Level.RemoveGui("missile-silos");
        Level.RemoveGui("keys");
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
}

public static class WarsGui {
    public static void CenteredLabel(Level level, Vector3 worldPosition, string text, Vector2 offset) {
        var screenPosition = level.view.cameraRig.camera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z > 0) {
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            screenPosition.y = Screen.height - screenPosition.y;
            GUI.Label(new Rect(screenPosition.x - size.x / 2 + offset.x, screenPosition.y - size.y / 2 + offset.y, size.x, size.y), text);
        }
    }
    public static void CenteredLabel(Level level, Vector3 worldPosition, string text) {
        CenteredLabel(level, worldPosition, text, new Vector2(0, 20));
    }
}