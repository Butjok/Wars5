using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public class Building : IDisposable {

    public static readonly HashSet<Building> undisposed = new();

    public readonly TileType type;
    public readonly Level level;
    public readonly Vector2Int position;
    public readonly BuildingView view;

    public int missileSiloLastLaunchDay = -99;
    public int missileSiloLaunchCooldown = 3;
    public int missileSiloAmmo = 999;
    public Vector2Int missileSiloRange = new(0, 999);
    public Vector2Int missileBlastRange = new(0, 3);
    public int missileUnitDamage = 5;
    public int missileBridgeDamage = 10;

    public int Cooldown(int day) {
        return Max(0, missileSiloLastLaunchDay + missileSiloLaunchCooldown - day);
    }

    private Player player;

    public Player Player {
        get => player;
        set {
            if (initialized && player == value)
                return;
            player = value;

            if (view) {
                view.PlayerColor = player?.Color ?? BuildingView.unownedColor;
                view.LightsColor = player?.Color ?? BuildingView.unownedLightsColor;
            }
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

    public Building(Level level, Vector2Int position, TileType type = TileType.City, Player player = null, int cp = int.MaxValue,
        BuildingView viewPrefab = null, Vector2Int? lookDirection = null) {
        undisposed.Add(this);

        //var views = Object.FindObjectsOfType<BuildingView>();
        //viewPrefab = views.FirstOrDefault(v => v.transform.position.ToVector2Int() == position);

        if (viewPrefab) {
            view = Object.Instantiate(viewPrefab, level.view.transform);
            view.prefab = viewPrefab;
            view.Position = position;
            if (type != TileType.Hq)
                view.LookDirection = lookDirection ?? Vector2Int.up;
        }

        this.type = type;
        this.level = level;
        this.position = position;
        Player = player;
        Cp = cp;

        Assert.IsTrue(!level.buildings.ContainsKey(position) || level.buildings[position] == null);
        level.buildings[position] = this;
        level.tiles[position] = type;

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

        level.tiles.Remove(position);
        level.buildings.Remove(position);
    }

    private bool moved;
    public bool Moved {
        get => moved;
        set => moved = view.Moved = value;
    }
}