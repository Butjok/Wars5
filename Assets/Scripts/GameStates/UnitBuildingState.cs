using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class UnitBuildingState {

    public static IEnumerator New(Game2 game,Building building) {

        Assert.IsTrue(building.player.v==game.CurrentPlayer);

        Debug.Log($"Building state at building: {building}");

        Assert.IsTrue(game.TryGetBuilding(building.position, out var check));
        Assert.AreEqual(building, check);
        Assert.IsFalse(game.TryGetUnit(building.position, out _));
        Assert.IsNotNull(building.player.v);

        var types = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
        var availableTypes = types.Where(type => (Rules.BuildableUnits(building.type) & type) != 0).ToList();
        var index = -1;

        while (true) {
            yield return null;

            if (game.input.buildUnitType != 0) {

                Assert.IsTrue(availableTypes.Contains(game.input.buildUnitType));

                var unit = new Unit(building.player.v, true, game.input.buildUnitType, building.position, viewPrefab: Resources.Load<UnitView>("light-tank"));
                game.input.Reset();

                Debug.Log($"Built unit {unit}");

                yield return SelectionState.New(game);
                yield break;
            }

            if (game.CurrentPlayer.IsAi)
                continue;

            if (Input.GetKeyDown(KeyCode.Tab)) {
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

            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right)) {
                game.input.Reset();
                yield return SelectionState.New(game);
                yield break;
            }
        }
    }
}