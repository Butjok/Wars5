using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;

public class LevelEditorUnitsModeState : StateMachineState {

    public enum Command {
        CycleUnitType,
        CyclePlayer,
        PlaceUnit,
        RemoveUnit,
        PickUnit,
        InspectUnit
    }

    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.Recon, UnitType.LightTank, UnitType.MediumTank, UnitType.Rockets, };
    public UnitType unitType = UnitType.Infantry;
    public Player player;
    public Unit inspectedUnit;

    public LevelEditorUnitsModeState(StateMachine stateMachine) : base(stateMachine) { }

    [Command]
    public static void FixPlayers() {
        var levelEditorSessionState = Game.Instance.stateMachine.Find<LevelEditorSessionState>();
        levelEditorSessionState.level.players.Clear();
        var foundPlayers = new HashSet<Player>();
        foreach (var unit in levelEditorSessionState.level.units.Values)
            foundPlayers.Add(unit.Player);
        foreach (var player in foundPlayers)
            levelEditorSessionState.level.players.Add(player);
    }
    
    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.TryFind<GameSessionState>().game;
            var editorState = stateMachine.TryFind<LevelEditorSessionState>();
            var level = editorState.level;
            var gui = editorState.gui;
            var tiles = level.tiles;
            var units = level.units;
            var camera = level.view.cameraRig.camera;

            //while (true)
            //  yield return StateChange.none;

            bool TryRemoveUnit(Vector2Int position) {
                if (!units.TryGetValue(position, out var unit))
                    return false;
                unit.Despawn();
                if (inspectedUnit == unit)
                    inspectedUnit = null;
                return true;
            }

            gui.layerStack.Push(() => {
                GUILayout.Label($"Level editor > Units [{player} {unitType}]");
                if (inspectedUnit != null) {
                    GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
                    //GUILayout.Label($"  Type: {inspectedUnit.type}");
                    //GUILayout.Label($"  Player: {inspectedUnit.Player}");
                    //GUILayout.Label($"  Position: {inspectedUnit.Position}");
                    //GUILayout.Label($"  Moved: {inspectedUnit.Moved}");
                    //GUILayout.Label($"  Hp: {inspectedUnit.Hp} / {Rules.MaxHp(inspectedUnit)}");
                    //GUILayout.Label($"  Move capacity: {Rules.MoveCapacity(inspectedUnit)}");
                    GUILayout.Label($"  Fuel: {inspectedUnit.Fuel} / {Rules.MaxFuel(inspectedUnit)}");
                    //GUILayout.Label($"  Brain states: {(inspectedUnit.brain.states.Count == 0 ? "-" : string.Join(" / ", inspectedUnit.brain.states.Reverse().Select(state => state.ToString())))}");
                    if (inspectedUnit.Ammo.Count>0) {
                        GUILayout.Label($"  Ammo:");
                        foreach (var (weapon, ammo) in inspectedUnit.Ammo)
                            GUILayout.Label($"    {weapon}: {ammo}");
                    }
                }
            });

            player = level.players[0];

            while (true) {
                yield return StateChange.none;

                editorState.DrawBridges();

                {
                    Unit unit = null;
                    if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition))
                        level.TryGetUnit(mousePosition, out unit);
                    inspectedUnit = unit;
                }

                if (TryEnqueueModeSelectionCommand()) { }

                else if (Input.GetKeyDown(KeyCode.Tab))
                    game.EnqueueCommand(Command.CycleUnitType, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetKeyDown(KeyCode.F2))
                    game.EnqueueCommand(Command.CyclePlayer, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetMouseButton(Mouse.left) && camera.TryGetMousePosition(out Vector2Int mousePosition) && tiles.ContainsKey(mousePosition))
                    game.EnqueueCommand(Command.PlaceUnit, (mousePosition, unitType, player));

                else if (Input.GetMouseButton(Mouse.right) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.RemoveUnit, mousePosition);

                else if (Input.GetKeyDown(KeyCode.LeftAlt) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.PickUnit, mousePosition);

                else if (Input.GetKeyDown(KeyCode.L))
                    game.aiPlayerCommander.DrawPotentialUnitActions();

                while (game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (LevelEditorSessionState.SelectModeCommand, _):
                            yield return HandleModeSelectionCommand(command);
                            break;

                        case (Command.CyclePlayer, int offset):
                            player = player.Cycle(level.players, offset);
                            break;

                        case (Command.CycleUnitType, int offset):
                            unitType = unitType.Cycle(unitTypes, offset);
                            break;

                        case (Command.PlaceUnit, (Vector2Int position, UnitType unitType, Player player)): {
                            if (units.ContainsKey(position))
                                TryRemoveUnit(position);

                            var viewPrefab = (unitType switch {
                                UnitType.Artillery => "WbHowitzer",
                                UnitType.Apc => "WbApc",
                                UnitType.Recon => "WbRecon",
                                UnitType.LightTank => "WbLightTank",
                                UnitType.Rockets => "WbRockets",
                                UnitType.MediumTank => "WbMdTank",
                                UnitType.Infantry or UnitType.AntiTank => "WbInfantry",
                                _ => "WbLightTank"
                            }).LoadAs<UnitView>();

                            Debug.Log("placing");

                            var unit = new Unit {
                                Player = player,
                                type = unitType,
                                Position = position,
                                lookDirection = player.unitLookDirection,
                                ViewPrefab = viewPrefab
                            };
                            units[position] = unit;
                            unit.Spawn();
                            break;
                        }

                        case (Command.RemoveUnit, Vector2Int position):
                            TryRemoveUnit(position);
                            break;

                        case (Command.PickUnit, Vector2Int position): {
                            if (units.TryGetValue(position, out var unit)) {
                                unitType = unit.type;
                                player = unit.Player;
                            }

                            break;
                        }

                        default:
                            yield return HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }

    public override void Exit() {
        var gui = stateMachine.TryFind<LevelEditorSessionState>().gui;
        gui.layerStack.Pop();
    }
}