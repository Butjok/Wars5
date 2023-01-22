using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class Building : IDisposable {

    public static readonly HashSet<Building> undisposed = new();

    public TileType type;
    public Main main;
    public Vector2Int position;
    public ChangeTracker<Player> player;
    public ChangeTracker<int> cp;
    public BuildingView view;
    
    public bool IsAccessible => !main.TryGetUnit(position, out _);

    public Building(Main main, Vector2Int position, TileType type = TileType.City, Player player = null, int? cp = null,
        BuildingView viewPrefab=null, Vector2Int? lookDirection=null) {

        undisposed.Add(this);
        
        this.player = new ChangeTracker<Player>(_ => {
            if (view)
                view.PlayerColor = this.player.v.color;
        });
        this.cp = new ChangeTracker<int>(_ => { });

        this.type = type;
        this.main = main;
        this.position = position;
        this.player.v = player;
        this.cp.v = cp ?? Rules.MaxCp(type);

        Assert.IsTrue(!main.buildings.ContainsKey(position) || main.buildings[position] == null);
        main.buildings[position] = this;
        main.tiles[position] = type;

        if (viewPrefab) {
            view = Object.Instantiate(viewPrefab, main.transform);
            view.prefab = viewPrefab;
            view.Position = position;
            view.LookDirection = lookDirection ?? Vector2Int.up;
            view.PlayerColor = player?.color ?? Palette.white;
        }
    }

    public static implicit operator TileType(Building building) {
        return building.type;
    }
    
    public Vector3 Position3d => position.ToVector3Int();

    public override string ToString() {
        return $"{type}{position} {player.v}";
    }
    public void Dispose() {
        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }
        main.tiles.Remove(position);
        main.buildings.Remove(position);
        undisposed.Remove(this);
    }
}