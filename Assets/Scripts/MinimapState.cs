using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class MinimapState : StateMachineState {
    
    public MinimapState(StateMachine stateMachine) : base(stateMachine) { }
    public MinimapUi ui;
    
    public override IEnumerator<StateChange> Enter {
        get {
            var level = FindState<LevelSessionState>().level;
            ui = level.view.minimap;
            Assert.IsTrue(ui);
            var tiles = level.tiles;
            var units = level.units;

            ui.Show(
                tiles.Keys.Select(position => (
                    position,
                    tiles[position],
                    level.TryGetBuilding(position, out var building) && building.Player != null ? building.Player.Color : Color.white)),
                units.Values.Select(unit => (
                    unit.view.transform,
                    unit.type,
                    unit.Player.Color)));

            while (true) {
                yield return StateChange.none;
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.CapsLock)) {
                    yield return StateChange.none;
                    break;
                }
            }
        }
    }
    
    public override void Exit() {
        ui.Hide();
        base.Exit();
    }
}