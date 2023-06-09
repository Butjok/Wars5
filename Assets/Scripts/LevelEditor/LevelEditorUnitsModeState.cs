using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorUnitsModeState : StateMachine.State {

    public enum Command { CycleUnitType, CyclePlayer, PlaceUnit, RemoveUnit, PickUnit, InspectUnit }

    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.Recon, UnitType.LightTank, UnitType.MediumTank, UnitType.Rockets, };
    public UnitType unitType = UnitType.Infantry;
    public Vector2Int lookDirection = Vector2Int.up;
    public Player player;

    private Unit inspectedUnit;
    public void SetInspectedUnit(LevelEditorGui gui, Unit unit) {

        inspectedUnit = unit;
        
        gui.Remove(name => name.StartsWith("inspected-unit."));

        if (unit != null)
            gui
                .Add("inspected-unit.type", () => unit.type)
                .Add("inspected-unit.player", () => unit.Player)
                .Add("inspected-unit.position", () => unit.Position)
                .Add("inspected-unit.moved", () => unit.Moved)
                .Add("inspected-unit.hp", () => $"{unit.Hp} / {Rules.MaxHp(unit)}")
                .Add("inspected-unit.move-capacity", () => Rules.MoveCapacity(unit))
                .Add("inspected-unit.fuel", () => $"{unit.Fuel} / {Rules.MaxFuel(unit)}");
    }

    public LevelEditorUnitsModeState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var game = stateMachine.TryFind<GameSessionState>().game;
            var editorState = stateMachine.TryFind<LevelEditorState>();
            var level = editorState.level;
            var gui = editorState.gui;
            var tiles = level.tiles;
            var units = level.units;
            var camera = level.view.cameraRig.camera;

            bool TryRemoveUnit(Vector2Int position) {
                if (!units.TryGetValue(position, out var unit))
                    return false;
                unit.Dispose();
                if (inspectedUnit == unit)
                    SetInspectedUnit(gui, null);
                return true;
            }

            gui
                .Push()
                .Add("unit-type", () => unitType)
                .Add("look-direction", () => lookDirection)
                .Add("player", () => player);

            level.view.cursorView.show = true;

            player = level.players[0];

            while (true) {
                yield return StateChange.none;

                editorState.DrawBridges();

                if (Input.GetKeyDown(KeyCode.F8))
                    game.EnqueueCommand(LevelEditorState.Command.SelectTriggersMode);

                else if (Input.GetKeyDown(KeyCode.Tab))
                    game.EnqueueCommand(Command.CycleUnitType, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetKeyDown(KeyCode.F2))
                    game.EnqueueCommand(Command.CyclePlayer, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetMouseButton(Mouse.left) && camera.TryGetMousePosition(out Vector2Int mousePosition) && tiles.ContainsKey(mousePosition))
                    game.EnqueueCommand(Command.PlaceUnit, (mousePosition, unitType, player));

                else if (Input.GetMouseButton(Mouse.right) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.RemoveUnit, mousePosition);

                else if (Input.GetKeyDown(KeyCode.F5))
                    game.EnqueueCommand(LevelEditorState.Command.Play);

                else if (Input.GetKeyDown(KeyCode.LeftAlt) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.PickUnit, mousePosition);

                else if (Input.GetKeyDown(KeyCode.I) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.InspectUnit, mousePosition);
                
                else if (Input.GetKeyDown(KeyCode.L))
                    game.aiPlayerCommander.DrawPotentialUnitActions();

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (LevelEditorState.Command.SelectTriggersMode, _):
                            yield return StateChange.ReplaceWith(new LevelEditorTriggersModeState(stateMachine));
                            break;

                        case (Command.CyclePlayer, int offset):
                            player = player.Cycle(level.players, offset);
                            break;

                        case (Command.CycleUnitType, int offset):
                            unitType = unitType.Cycle(unitTypes, offset);
                            break;

                        case (LevelEditorState.Command.Play, _):
                            yield return StateChange.Push(new LevelEditorPlayState(stateMachine));
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

                            new Unit(player, unitType, position, player.unitLookDirection, viewPrefab: viewPrefab);
                            break;
                        }

                        case (Command.RemoveUnit, Vector2Int position):
                            TryRemoveUnit(position);
                            break;

                        case (Command.PickUnit, Vector2Int position):
                            if (units.TryGetValue(position, out var unit)) {
                                unitType = unit.type;
                                player = unit.Player;
                            }
                            break;

                        case (Command.InspectUnit, Vector2Int position):
                            units.TryGetValue(position, out unit);
                            SetInspectedUnit(gui, unit);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(command.ToString());
                    }
            }
        }
    }

    public override void Dispose() {
        stateMachine.TryFind<LevelEditorState>().gui.Pop();
    }
}