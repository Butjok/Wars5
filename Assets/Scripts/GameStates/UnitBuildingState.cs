using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class UnitBuildingState {

    public static bool cancel;
    
    public static IEnumerator New(Main main, Building building) {

        Assert.IsTrue(building.player.v == main.CurrentPlayer);

        var menuView = Object.FindObjectOfType<UnitBuildMenu>(true);
        Assert.IsTrue(menuView);

        PlayerView.globalVisibility = false;
        yield return null;
        menuView.Show(building);

        Debug.Log($"Building state at building: {building}");

        Assert.IsTrue(main.TryGetBuilding(building.position, out var check));
        Assert.AreEqual(building, check);
        Assert.IsFalse(main.TryGetUnit(building.position, out _));
        Assert.IsNotNull(building.player.v);

        var types = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
        var availableTypes = types.Where(type => (Rules.BuildableUnits(building.type) & type) != 0).ToList();
        var index = -1;

        while (true) {
            yield return null;

            if (main.input.buildUnitType != 0) {

                Assert.IsTrue(availableTypes.Contains(main.input.buildUnitType));

                var unit = new Unit(building.player.v, true, main.input.buildUnitType, building.position, viewPrefab: Resources.Load<UnitView>("light-tank"));
                main.input.Reset();

                Debug.Log($"Built unit {unit}");

                menuView.Hide();
                yield return SelectionState.New(main);
                yield break;
            }

            if (main.CurrentPlayer.IsAi)
                continue;

            if (cancel) {
                cancel = false;
                menuView.Hide();
                PlayerView.globalVisibility = true;
                yield return SelectionState.New(main);
                yield break;
            }

            /*if (Input.GetKeyDown(KeyCode.Tab)) {
                if (availableTypes.Count == 0)
                    UiSound.Instance.notAllowed.Play();
                else {
                    index = (index + 1) % availableTypes.Count;
                    Debug.Log(availableTypes[index]);
                }
            }

            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
                if (index == -1)
                    UiSound.Instance.notAllowed.Play();
                else
                    game.input.buildUnitType = availableTypes[index];
            }

            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right) || game.input.cancel) {
                game.input.Reset();
                menuView.Hide();
                yield return SelectionState.New(game);
                yield break;
            }*/
        }
    }
}