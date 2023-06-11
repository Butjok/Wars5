using System;
using System.Collections.Generic;
using Drawing;
using UnityEngine;

public class LevelEditorUnitsModeState : StateMachineState {

    public enum Command { CycleUnitType, CyclePlayer, PlaceUnit, RemoveUnit, PickUnit, InspectUnit }

    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.Recon, UnitType.LightTank, UnitType.MediumTank, UnitType.Rockets, };
    public UnitType unitType = UnitType.Infantry;
    public Player player;

    private Unit inspectedUnit;
    public void SetInspectedUnit(LevelEditorGui gui, Unit unit) {

        inspectedUnit = unit;

        gui.Remove(name => name.StartsWith("InspectedUnit."));

        if (unit != null)
            gui
                .Add("InspectedUnit.Type", () => unit.type)
                .Add("InspectedUnit.Player", () => unit.Player)
                .Add("InspectedUnit.Position", () => unit.Position)
                .Add("InspectedUnit.Moved", () => unit.Moved)
                .Add("InspectedUnit.Hp", () => $"{unit.Hp} / {Rules.MaxHp(unit)}")
                .Add("InspectedUnit.MoveCapacity", () => Rules.MoveCapacity(unit))
                .Add("InspectedUnit.Fuel", () => $"{unit.Fuel} / {Rules.MaxFuel(unit)}");
    }

    public LevelEditorUnitsModeState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Entry {
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
                if (inspectedUnit == unit)
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

                if (inspectedUnit is { Position: { } unitPosition })
                    Draw.ingame.CircleXZ(unitPosition.ToVector3Int().ToVector3(), .5f, Color.black);

                if (Input.GetKeyDown(KeyCode.F8))
                    game.EnqueueCommand(LevelEditorSessionState.Command.SelectTriggersMode);

                else if (Input.GetKeyDown(KeyCode.Tab))
                    game.EnqueueCommand(Command.CycleUnitType, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetKeyDown(KeyCode.F2))
                    game.EnqueueCommand(Command.CyclePlayer, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetMouseButton(Mouse.left) && camera.TryGetMousePosition(out Vector2Int mousePosition) && tiles.ContainsKey(mousePosition))
                    game.EnqueueCommand(Command.PlaceUnit, (mousePosition, unitType, player));

                else if (Input.GetMouseButton(Mouse.right) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.RemoveUnit, mousePosition);

                else if (Input.GetKeyDown(KeyCode.F5))
                    game.EnqueueCommand(LevelEditorSessionState.Command.Play);

                else if (Input.GetKeyDown(KeyCode.LeftAlt) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.PickUnit, mousePosition);

                else if (Input.GetKeyDown(KeyCode.I) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.InspectUnit, mousePosition);

                else if (Input.GetKeyDown(KeyCode.L))
                    game.aiPlayerCommander.DrawPotentialUnitActions();

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (LevelEditorSessionState.Command.SelectTriggersMode, _):
                            yield return StateChange.ReplaceWith(new LevelEditorTriggersModeState(stateMachine));
                            break;

                        case (Command.CyclePlayer, int offset):
                            player = player.Cycle(level.players, offset);
                            break;

                        case (Command.CycleUnitType, int offset):
                            unitType = unitType.Cycle(unitTypes, offset);
                            break;

                        case (LevelEditorSessionState.Command.Play, _):
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
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }

    public override void Exit() {
        stateMachine.TryFind<LevelEditorSessionState>().gui.Pop();
    }
}