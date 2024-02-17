using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class LevelEditorZoneModeState : StateMachineState {

    public enum Command {
        SelectZone, PlaceTile, RemoveTile, PickZone, CyclePlayer,
        ToggleConnection
    }

    public Player player;
    public string[] zoneNames = { "Zone0", "Zone1", "Zone2", "Zone3", "Zone4", "Zone5", "Zone6", "Zone7", "Zone8", "Zone9" };
    public string zoneName = "Zone0";

    public static Color[] colors = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };

    public IEnumerable<Zone> ConnectedZones => player?.rootZone == null ? Enumerable.Empty<Zone>() : Zone.GetConnected(player.rootZone);

    [Command] public static MoveType moveType;

    public override void Exit() {
        stateMachine.TryFind<LevelEditorSessionState>().gui.layerStack.Pop();
        base.Exit();
    }

    public LevelEditorZoneModeState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.TryFind<GameSessionState>().game;
            var editorState = stateMachine.TryFind<LevelEditorSessionState>();
            var level = editorState.level;
            var gui = editorState.gui;
            var camera = level.view.cameraRig.camera;

            bool TryRemoveTile(Vector2Int position) {
                foreach (var zone in ConnectedZones) {
                    zone.tiles.Remove(position);
                    if (zone.tiles.Count == 0) {
                        foreach (var neighbor in zone.neighbors)
                            neighbor.neighbors.Remove(zone);
                        if (player.rootZone == zone)
                            player.rootZone = null;
                    }
                }
                return true;
            }

            gui.layerStack.Push(() => {
                GUILayout.Label($"Level editor > Zones [{player} {zoneName}]");
            });

            player = level.players[0];

            while (true) {
                yield return StateChange.none;

                editorState.DrawBridges();

                if (TryEnqueueModeSelectionCommand()) { }

                else if (Input.GetKeyDown(KeyCode.F2))
                    game.EnqueueCommand(Command.CyclePlayer, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetMouseButton(Mouse.left) && camera.TryGetMousePosition(out Vector2Int mousePosition))
                    game.EnqueueCommand(Command.PlaceTile, mousePosition);

                else if (Input.GetMouseButton(Mouse.right) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.RemoveTile, mousePosition);

                else if (Input.GetKeyDown(KeyCode.LeftAlt) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.PickZone, mousePosition);

                else if (Input.GetKeyDown(KeyCode.F7)) {
                    if (player.rootZone != null) {
                        foreach (var zone in Zone.GetConnected(player.rootZone))
                        foreach (var moveType in Enum.GetValues(typeof(MoveType)).Cast<MoveType>()) {
                            var distances = ZoneDistances.Calculate(level.tiles, zone, moveType);
                            foreach (var (position, distance) in distances)
                                zone.distances.Add((moveType, position), distance);
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.F6)) {
                    var zone = ConnectedZones.SingleOrDefault(zone => zone.name == zoneName);
                    if (zone != null)
                        ScalarFieldDrawer.Draw(new ScalarField(
                            zone.distances.Keys.Select(key => key.position),
                            position => zone.distances[(moveType, position)]));
                }

                else if (Input.GetKeyDown(KeyCode.L) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.ToggleConnection, mousePosition);

                else
                    for (var key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
                        if (Input.GetKeyDown(key) && Input.GetKey(KeyCode.LeftShift))
                            game.EnqueueCommand(Command.SelectZone, zoneNames[key - KeyCode.Alpha0]);

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (LevelEditorSessionState.SelectModeCommand, _):
                            yield return HandleModeSelectionCommand(command);
                            break;

                        case (Command.SelectZone, string zoneName):
                            this.zoneName = zoneName;
                            break;

                        case (Command.PickZone, Vector2Int position): {
                            var zone = ConnectedZones.SingleOrDefault(zone => zone.tiles.Contains(position));
                            if (zone != null)
                                zoneName = zone.name;
                            break;
                        }

                        case (Command.PlaceTile, Vector2Int position): {
                            TryRemoveTile(position);
                            var zone = ConnectedZones.SingleOrDefault(zone => zone.name == zoneName);
                            if (zone == null) {
                                zone = new Zone { name = zoneName };
                                player.rootZone ??= zone;
                                if (player.rootZone != null && player.rootZone != zone) {
                                    player.rootZone.neighbors.Add(zone);
                                    zone.neighbors.Add(player.rootZone);
                                }
                            }
                            if (level.tiles.ContainsKey(position))
                                zone.tiles.Add(position);
                            break;
                        }

                        case (Command.RemoveTile, Vector2Int position):
                            TryRemoveTile(position);
                            break;

                        case (Command.ToggleConnection, Vector2Int position): {
                            var zone = ConnectedZones.SingleOrDefault(zone => zone.name == zoneName);
                            var other = ConnectedZones.SingleOrDefault(zone => zone.tiles.Contains(position));
                            if (zone != null && other != null && zone != other) {
                                if (zone.neighbors.Contains(other) && zone.neighbors.Count > 1 && other.neighbors.Count > 1) {
                                    zone.neighbors.Remove(other);
                                    other.neighbors.Remove(zone);
                                }
                                else {
                                    zone.neighbors.Add(other);
                                    other.neighbors.Add(zone);
                                }
                            }
                            break;
                        }

                        case (Command.CyclePlayer, int offset):
                            player = player.Cycle(level.players, offset);
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }

                if (player.rootZone != null) {
                    foreach (var zone in Zone.GetConnected(player.rootZone)) {
                        if (zone.tiles.Count > 0) {
                            var color = colors.GetWrapped(Array.IndexOf(zoneNames, zone.name));
                            var text = zone.name;
                            if (zone == player.rootZone)
                                text += " (root)";
                            Draw.ingame.Label2D(zone.GetCenter(), text, 14, LabelAlignment.Center, Color.black);
                            foreach (var position in zone.tiles)
                                Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, color);
                            foreach (var neighbor in zone.neighbors)
                                Draw.ingame.Line(zone.GetCenter(), neighbor.GetCenter(), Color.black);
                        }
                    }
                }
            }
        }
    }
}