using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class UnitBuildState : StateMachineState {

    public enum Command { Build, Close }

    public UnitBuildState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var (game, level, building) = (stateMachine.Find<GameSessionState>().game, stateMachine.Find<LevelSessionState>().level, stateMachine.Find<SelectionState>().building);
            var menu = level.view.unitBuildMenu;

            Assert.IsTrue(menu);
            Assert.IsTrue(building.Player == level.CurrentPlayer);
            PlayerView.globalVisibility = false;
            yield return StateChange.none;
            
            level.view.tilemapCursor.Hide();

            var success = Persons.TryGetFaction(level.CurrentPlayer.coName, out var factionName);
            Assert.IsTrue(success);

            int GetCost(UnitType unitType) {
                return Rules.Cost(unitType, level.CurrentPlayer);
            }
            menu.Show(
                unitType => game.EnqueueCommand(Command.Build, unitType),
                () => game.EnqueueCommand(Command.Close),
                factionName, level.CurrentPlayer.ColorName, level.CurrentPlayer.Credits, 
                unitType => GetCost(unitType) <= level.CurrentPlayer.Credits, GetCost, 
                unitType => UnitInfo.GetFullName(factionName, unitType), unitType => UnitInfo.GetDescription(factionName, unitType),
                unitType => UnitInfo.TryGetThumbnail(factionName, unitType),
                building.view);

            Debug.Log($"Building state at building: {building}");

            Assert.IsTrue(level.TryGetBuilding(building.position, out var check));
            Assert.AreEqual(building, check);
            Assert.IsFalse(level.TryGetUnit(building.position, out _));
            Assert.IsNotNull(building.Player);

            var availableTypes = Rules.GetBuildableUnitTypes(building.Type);
            while (true) {
                yield return StateChange.none;

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Build, UnitType type): {

                            Assert.IsTrue(availableTypes.Contains(type));
                            Assert.IsTrue(building.Player.CanAfford(type));

                            var viewPrefab = UnitView.DefaultPrefabFor(type);
                            // if (building.Player.co.unitTypesInfoOverride.TryGetValue(type, out var info) && info.viewPrefab)
                            //     viewPrefab = info.viewPrefab;
                            // else if (UnitTypesInfo.TryGet(type, out info) && info.viewPrefab)
                            //     viewPrefab = info.viewPrefab;
                            Assert.IsTrue(viewPrefab, type.ToString());

                            var unit = new Unit {
                                Player = building.Player,
                                type = type,
                                Position = building.position,
                                Moved = true,
                                viewPrefab = viewPrefab
                            };
                            unit.Initialize();
                            Debug.Log($"Built unit {unit}");

                            building.Player.SetCredits(building.Player.Credits - Rules.Cost(type, building.Player), true);

                            menu.Hide();
                            yield return StateChange.Pop();
                            break;
                        }

                        case (Command.Close, _): {
                            menu.Hide();
                            PlayerView.globalVisibility = true;
                            yield return StateChange.Pop();
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