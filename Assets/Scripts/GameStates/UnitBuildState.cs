using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class UnitBuildState : StateMachineState {

    public enum Command { Build, Close }

    public UnitBuildState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Entry {
        get {
            var (game, level, building, menu) = (FindState<GameSessionState>().game, FindState<LevelSessionState>().level, FindState<SelectionState>().building, FindObject<UnitBuildMenu>());

            Assert.IsTrue(building.Player == level.CurrentPlayer);
            PlayerView.globalVisibility = false;
            yield return StateChange.none;

            menu.Show(
                building,
                unitType => game.EnqueueCommand(Command.Build, unitType),
                () => game.EnqueueCommand(Command.Close));

            Debug.Log($"Building state at building: {building}");

            Assert.IsTrue(level.TryGetBuilding(building.position, out var check));
            Assert.AreEqual(building, check);
            Assert.IsFalse(level.TryGetUnit(building.position, out _));
            Assert.IsNotNull(building.Player);

            var availableTypes = Rules.GetBuildableUnitTypes(building.type);
            while (true) {
                yield return StateChange.none;

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Build, UnitType type): {

                            Assert.IsTrue(availableTypes.Contains(type));
                            Assert.IsTrue(building.Player.CanAfford(type));

                            var viewPrefab = UnitView.DefaultPrefab;
                            // if (building.Player.co.unitTypesInfoOverride.TryGetValue(type, out var info) && info.viewPrefab)
                            //     viewPrefab = info.viewPrefab;
                            // else if (UnitTypesInfo.TryGet(type, out info) && info.viewPrefab)
                            //     viewPrefab = info.viewPrefab;
                            Assert.IsTrue(viewPrefab, type.ToString());

                            var unit = new Unit(building.Player, type, building.position, moved: true, viewPrefab: viewPrefab);
                            unit.Moved = true;
                            Debug.Log($"Built unit {unit}");

                            building.Player.SetCredits(building.Player.Credits - Rules.Cost(type, building.Player), true);

                            menu.Hide();
                            yield return StateChange.ReplaceWith(new SelectionState(stateMachine));
                            break;
                        }

                        case (Command.Close, _): {
                            menu.Hide();
                            PlayerView.globalVisibility = true;
                            yield return StateChange.ReplaceWith(new SelectionState(stateMachine));
                            break;
                        }

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }
}