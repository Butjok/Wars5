using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class UnitBuildState : StateMachine.State {

    public enum Command { Build, Close }

    public UnitBuildState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var game = stateMachine.TryFind<GameSessionState>()?.game;
            var level = stateMachine.TryFind<PlayState>()?.level;
            var building = stateMachine.TryFind<SelectionState>()?.building;
            
            Assert.IsNotNull(game);
            Assert.IsNotNull(level);
            Assert.IsNotNull(building);

            Assert.IsTrue(building.Player == level.CurrentPlayer);

            var menuView = Object.FindObjectOfType<UnitBuildMenu>(true);
            Assert.IsTrue(menuView);

            PlayerView.globalVisibility = false;
            yield return StateChange.none;

            menuView.Show(
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

                            menuView.Hide();
                            yield return StateChange.ReplaceWith(new SelectionState(stateMachine));
                            break;
                        }

                        case (Command.Close, _): {
                            menuView.Hide();
                            PlayerView.globalVisibility = true;
                            yield return StateChange.ReplaceWith(new SelectionState(stateMachine));
                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
        }
    }
}