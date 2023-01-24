using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class UnitBuildState {

    public const string prefix = "unit-build-state.";
    public const string build = prefix + "build";
    public const string close = prefix + "close";
    
    public static IEnumerator New(Main main, Building building) {

        Assert.IsTrue(building.Player == main.CurrentPlayer);

        var menuView = Object.FindObjectOfType<UnitBuildMenu>(true);
        Assert.IsTrue(menuView);

        PlayerView.globalVisibility = false;
        yield return null;
        menuView.Show(building);

        Debug.Log($"Building state at building: {building}");

        Assert.IsTrue(main.TryGetBuilding(building.position, out var check));
        Assert.AreEqual(building, check);
        Assert.IsFalse(main.TryGetUnit(building.position, out _));
        Assert.IsNotNull(building.Player);

        var availableTypes = Rules.GetBuildableUnitTypes(building.type);
        var index = -1;

        while (true) {
            yield return null;
            
            while (main.commands.TryDequeue(out var input))
                foreach (var token in input.Tokenize())
                    switch (token) {

                        case build: {

                            var type = main.stack.Pop<UnitType>();
                            Assert.IsTrue(availableTypes.Contains(type));
                            Assert.IsTrue(building.Player.CanAfford(type));

                            var viewPrefab = UnitView.DefaultPrefab;
                            if (building.Player.co.unitTypesInfoOverride.TryGetValue(type, out var info) && info.viewPrefab)
                                viewPrefab = info.viewPrefab;
                            else if (UnitTypesInfo.TryGet(type, out info) && info.viewPrefab)
                                viewPrefab = info.viewPrefab;
                            Assert.IsTrue(viewPrefab, type.ToString());
                            
                            var unit = new Unit(building.Player, type,  building.position, moved: true, viewPrefab:viewPrefab);
                            unit.Moved = true;
                            Debug.Log($"Built unit {unit}");

                            building.Player.credits -= Rules.Cost(type, building.Player);

                            menuView.Hide();
                            yield return  SelectionState.Run(main);
                            yield break;            
                        }

                        case close: {
                            menuView.Hide();
                            PlayerView.globalVisibility = true;
                            yield return SelectionState.Run(main);
                            yield break;
                        }
                        
                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}