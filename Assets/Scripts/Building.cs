using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Building : IDisposable {

    public static readonly HashSet<Building> undisposed = new();

    public TileType type;
    public Main main;
    public Vector2Int position;
    public ChangeTracker<Player> player;
    public ChangeTracker<int> cp;
    
    public bool IsAccessible => !main.TryGetUnit(position, out _);

    public Building(Main main, Vector2Int position, TileType type = TileType.City, Player player = null, int? cp = null) {

        undisposed.Add(this);
        
        this.player = new ChangeTracker<Player>(_ => { });
        this.cp = new ChangeTracker<int>(_ => { });

        this.type = type;
        this.main = main;
        this.position = position;
        this.player.v = player;
        this.cp.v = cp ?? Rules.MaxCp(type);

        Assert.IsTrue(!main.buildings.ContainsKey(position) || main.buildings[position] == null);
        main.buildings[position] = this;
        main.tiles[position] = type;
    }

    public static implicit operator TileType(Building building) {
        return building.type;
    }

    public override string ToString() {
        return $"{type}{position} {player.v}";
    }
    public void Dispose() {
        undisposed.Remove(this);
    }
}