using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Bridge:IDisposable {

    public Dictionary<Vector2Int, TileType> tiles = new();
    public Main main;
    public BridgeView view;

    public Bridge(Main main, IEnumerable<Vector2Int> positions, BridgeView view, int hp = 20) {

        Assert.IsTrue(view);
        Assert.IsNull(view.bridge);

        foreach (var position in positions) {
            var found = main.tiles.TryGetValue(position, out var tileType);
            Assert.IsTrue(found, position.ToString());
            Assert.IsFalse(main.buildings.TryGetValue(position, out _), position.ToString());
            tiles.Add(position, tileType);
        }
        
        view.bridge = this;
        this.view = view;
        this.main = main;
        SetHp(hp);

        main.bridges.Add(this);
    }

    public void Dispose() {
        main.bridges.Remove(this);
        view.bridge = null;
        view = null;
    }

    public const int maxHp = 20;
    private int hp = maxHp;

    public void SetHp(int value, bool animateDestruction=false) {
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
            main.tiles.Remove(position);
            if (removeBuilding && main.TryGetBuilding(position, out var building))
                building.Dispose();
            if (removeUnits && main.TryGetUnit(position, out var unit))
                unit.Dispose();
        }
        ((Main2)main).RebuildTilemapMesh();
    }

    public void RestoreTiles() {
        foreach (var (position, tileType) in tiles)
            main.tiles.Add(position, tileType);
        ((Main2)main).RebuildTilemapMesh();
    }
}