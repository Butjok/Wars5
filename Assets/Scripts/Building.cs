using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Building : IDisposable {

    public TileType type;
    public Level level;
    public Vector2Int position;
    public ChangeTracker<Player> player;
    public ChangeTracker<int> cp;
    
    public bool IsAccessible => !level.TryGetUnit(position, out _);

    public Building(Level level, Vector2Int position, TileType type = TileType.City, Player player = null, int? cp = null) {

        this.player = new ChangeTracker<Player>(_ => { });
        this.cp = new ChangeTracker<int>(_ => { });

        this.type = type;
        this.level = level;
        this.position = position;
        this.player.v = player;
        this.cp.v = cp ?? Rules.MaxCp(type);

        Assert.IsTrue(!level.buildings.ContainsKey(position) || level.buildings[position] == null);
        level.buildings[position] = this;
        level.tiles[position] = type;
    }

    public static implicit operator TileType(Building building) {
        return building.type;
    }

    public override string ToString() {
        return $"{type}{position} {player.v}";
    }
    public void Dispose() { }
}