using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;

public class LevelEditorUnitsModeState : StateMachineState {

    public enum Command { CycleUnitType, CyclePlayer, PlaceUnit, RemoveUnit, PickUnit, InspectUnit }

    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.Recon, UnitType.LightTank, UnitType.MediumTank, UnitType.Rockets, };
    public UnitType unitType = UnitType.Infantry;
    public Player player;

    public Unit InspectedUnit { get; private set; }

    public LevelEditorUnitsModeState(StateMachine stateMachine) : base(stateMachine) { }

    public void SetInspectedUnit(LevelEditorGui gui, Unit unit) {

        InspectedUnit = unit;

        gui.Remove(name => name.StartsWith("InspectedUnit."));

        if (unit != null) {
            gui
                .Add("InspectedUnit.Type", () => unit.type)
                .Add("InspectedUnit.Player", () => unit.Player)
                .Add("InspectedUnit.Position", () => unit.Position)
                .Add("InspectedUnit.Moved", () => unit.Moved)
                .Add("InspectedUnit.Hp", () => $"{unit.Hp} / {Rules.MaxHp(unit)}")
                .Add("InspectedUnit.MoveCapacity", () => Rules.MoveCapacity(unit))
                .Add("InspectedUnit.Fuel", () => $"{unit.Fuel} / {Rules.MaxFuel(unit)}")
                .Add("InspectedUnit.BrainStates", () => unit.brain.states.Count == 0 ? "-" : string.Join(" / ", unit.brain.states.Reverse().Select(state => state.ToString())));
        }
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

            bool TryRemoveUnit(Vector2Int position) {
                if (!units.TryGetValue(position, out var unit))
                    return false;
                unit.Dispose();
                if (InspectedUnit == unit)
                    SetInspectedUnit(gui, null);
                return true;
            }

            gui
                .Push()
                .Add("UnitType", () => unitType)
                .Add("Player", () => player);

            player = level.players[0];

            while (true) {
                yield return StateChange.none;

                editorState.DrawBridges();

                {
                    Unit unit = null;
                    if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition))
                        level.TryGetUnit(mousePosition, out unit);
                    SetInspectedUnit(gui, unit);
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

                else if (Input.GetKeyDown(KeyCode.F5))
                    game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.Play);

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
                                _ => "WbLightTank"
                            }).LoadAs<UnitView>();

                            var unit = new Unit(player, unitType, position, player.unitLookDirection, viewPrefab: viewPrefab);
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
        gui.Pop();
    }
}