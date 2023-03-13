using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class UnitBuildState {

    public const string prefix = "unit-build-state.";
    public const string build = prefix + "build";
    public const string close = prefix + "close";

    public static IEnumerator<StateChange> New(Level level, Building building) {

        Assert.IsTrue(building.Player == level.CurrentPlayer);

        var menuView = Object.FindObjectOfType<UnitBuildMenu>(true);
        Assert.IsTrue(menuView);

        PlayerView.globalVisibility = false;
        yield return StateChange.none;
        menuView.Show(building);

        Debug.Log($"Building state at building: {building}");

        Assert.IsTrue(level.TryGetBuilding(building.position, out var check));
        Assert.AreEqual(building, check);
        Assert.IsFalse(level.TryGetUnit(building.position, out _));
        Assert.IsNotNull(building.Player);

        var availableTypes = Rules.GetBuildableUnitTypes(building.type);
        var index = -1;

        while (true) {
            yield return StateChange.none;

            while (level.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {

                        case build: {

                            var type = level.stack.Pop<UnitType>();
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
                            yield return StateChange.ReplaceWith(nameof(SelectionState), SelectionState.Run(level));
                            break;
                        }

                        case close: {
                            menuView.Hide();
                            PlayerView.globalVisibility = true;
                            yield return StateChange.ReplaceWith(nameof(SelectionState), SelectionState.Run(level));
                            break;
                        }

                        default:
                            level.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}