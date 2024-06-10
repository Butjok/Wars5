using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Bridge : IDisposable {

    public Dictionary<Vector2Int, TileType> tiles = new();
    public Level level;
    public BridgeView view;

    public Bridge(Level level, IEnumerable<Vector2Int> positions, BridgeView view, int hp = 20) {

        Assert.IsTrue(view);
        Assert.IsNull(view.bridge);

        foreach (var position in positions) {
            var found = level.tiles.TryGetValue(position, out var tileType);
            Assert.IsTrue(found, position.ToString());
            Assert.IsFalse(level.buildings.TryGetValue(position, out _), position.ToString());
            tiles.Add(position, tileType);
        }

        view.bridge = this;
        this.view = view;
        this.level = level;
        SetHp(hp);

        level.bridges.Add(this);
    }

    public void Dispose() {
        level.bridges.Remove(this);
        view.bridge = null;
        view = null;
    }

    public const int maxHp = 20;
    private int hp = maxHp;

    public void SetHp(int value, bool animateDestruction = false) {
        var oldHp = hp;
        hp = Mathf.Clamp(value, 0, maxHp);
        view.SetHp(hp, animateDestruction);
        if (oldHp > 0 && hp == 0)
            RemoveTiles();
        else if (oldHp == 0 && hp > 0) {
            Debug.Log("bridge's tiles were restored but units not");
            RestoreTiles();
        }
    }

    public int Hp => hp;

    public void RemoveTiles(bool removeBuilding = true, bool removeUnits = true) {
        foreach (var position in tiles.Keys) {
            level.tiles.Remove(position);
            if (removeBuilding && level.TryGetBuilding(position, out var building))
                building.Despawn();
            if (removeUnits && level.TryGetUnit(position, out var unit))
                unit.Despawn();
        }
    }

    public void RestoreTiles() {
        foreach (var (position, tileType) in tiles)
            level.tiles.Add(position, tileType);
    }
}