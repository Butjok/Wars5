using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public class Building : IDisposable {

    public static readonly HashSet<Building> undisposed = new();

    public readonly TileType type;
    public readonly Main main;
    public readonly Vector2Int position;
    public readonly BuildingView view;
    
    public int missileSiloLastLaunchTurn = -99;
    public int missileSiloLaunchCooldown = 1;
    public int missileSiloAmmo = 999;
    public Vector2Int missileSiloRange = new(0, 999);
    public Vector2Int missileBlastRange = new(0, 3);
    public int missileUnitDamage = 5;
    public int missileBridgeDamage = 10;

    private Player player;
    public Player Player {
        get => player;
        set {
            if (initialized && player == value)
                return;
            player = value;

            if (view)
                view.PlayerColor = player?.Color ?? new Color(0, 0, 0, 0);
        }
    }

    private int cp;
    public int Cp {
        get => cp;
        set {
            if (initialized && cp == value)
                return;
            cp = Clamp(value, 0, initialized ? MaxCp(this) : MaxCp(type));

            // do nothing yet
        }
    }

    private bool initialized;

    public Building(Main main, Vector2Int position, TileType type = TileType.City, Player player = null, int cp = int.MaxValue,
        BuildingView viewPrefab = null, Vector2Int? lookDirection = null) {

        undisposed.Add(this);

        if (viewPrefab) {
            view = Object.Instantiate(viewPrefab, main.transform);
            view.prefab = viewPrefab;
            view.Position = position;
            view.LookDirection = lookDirection ?? Vector2Int.up;
        }

        this.type = type;
        this.main = main;
        this.position = position;
        Player = player;
        Cp = cp;

        Assert.IsTrue(!main.buildings.ContainsKey(position) || main.buildings[position] == null);
        main.buildings[position] = this;
        main.tiles[position] = type;

        initialized = true;
    }

    public static implicit operator TileType(Building building) {
        return building.type;
    }

    public Vector3 Position3d => position.ToVector3Int();

    public override string ToString() {
        return $"{type}{position} {Player}";
    }
    public void Dispose() {
        
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);
        
        if (view)
            Object.Destroy(view.gameObject);
        
        main.tiles.Remove(position);
        main.buildings.Remove(position);
    }
}