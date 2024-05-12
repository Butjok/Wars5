using System;
using System.Collections.Generic;
using System.Linq;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public class Building : IDisposable {

    public class MissileSiloStats {
        public int lastLaunchDay = -99;
        public int launchCooldown = 3;
        public int ammo = 999;
        public Vector2Int range = new(0, 999);
        public Vector2Int blastRange = new(0, 3);
        public int unitDamage = 5;
        public int bridgeDamage = 10;
    }

    public static readonly HashSet<Building> undisposed = new();

    [DontSave] private TileType type;
    public TileType Type {
        get => type;
        set {
            type = value;
            if (value == TileType.MissileSilo && missileSilo == null)
                missileSilo = new MissileSiloStats();
            else if (value != TileType.MissileSilo && missileSilo != null)
                missileSilo = null;
        }
    }
    public Level level;
    public Vector2Int position;
    public Vector2Int lookDirection = Vector2Int.up;
    public MissileSiloStats missileSilo;

    public int Cooldown(int day) {
        return Max(0, missileSilo.lastLaunchDay + missileSilo.launchCooldown - day);
    }

    [DontSave] public BuildingView viewPrefab;
    public string ViewPrefabName {
        get => viewPrefab ? viewPrefab.name : null;
        set => viewPrefab = value.LoadAs<BuildingView>();
    }

    [DontSave] public BuildingView view;

    [DontSave] private Player player;
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

    [DontSave] private int cp;
    public int Cp {
        get => cp;
        set {
            if (initialized && cp == value)
                return;
            cp = Clamp(value, 0, initialized ? MaxCp(this) : MaxCp(Type));
        }
    }

    [DontSave] private bool moved;
    public bool Moved {
        get => moved;
        set {
            moved = value;
            if (view)
                view.Moved = value;
        }
    }

    [DontSave] private bool initialized;

    public void Initialize() {
        Assert.IsFalse(initialized);
        Assert.IsFalse(undisposed.Contains(this));
        undisposed.Add(this);

        if (viewPrefab) {
            view = Object.Instantiate(viewPrefab, level.view.transform);
            view.prefab = viewPrefab;
            view.Position = position;
            if (Type != TileType.Hq)
                view.LookDirection = lookDirection;
        }

        Player = Player;
        Cp = Cp;
        Moved = Moved;

        Assert.IsTrue(!level.buildings.ContainsKey(position) || level.buildings[position] == null);
        level.buildings[position] = this;
        level.tiles[position] = Type;

        initialized = true;
    }

    public static implicit operator TileType(Building building) {
        return building.Type;
    }

    public override string ToString() {
        return $"{Type}{position} {Player}";
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);

        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }

        level.tiles.Remove(position);
        level.buildings.Remove(position);

        initialized = false;
    }
}