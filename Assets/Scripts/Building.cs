using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Building : IDisposable {

    public TileType type;
    public Game game;
    public Vector2Int position;
    public ChangeTracker<Player> player;
    public ChangeTracker<int> cp;
    
    public bool IsAccessible => !game.TryGetUnit(position, out _);

    public Building(Game game, Vector2Int position, TileType type = TileType.City, Player player = null, int? cp = null) {

        this.player = new ChangeTracker<Player>(_ => { });
        this.cp = new ChangeTracker<int>(_ => { });

        this.type = type;
        this.game = game;
        this.position = position;
        this.player.v = player;
        this.cp.v = cp ?? Rules.MaxCp(type);

        Assert.IsTrue(!game.buildings.ContainsKey(position) || game.buildings[position] == null);
        game.buildings[position] = this;
        game.tiles[position] = type;
    }

    public static implicit operator TileType(Building building) {
        return building.type;
    }

    public override string ToString() {
        return $"{type}{position} {player.v}";
    }
    public void Dispose() { }
}