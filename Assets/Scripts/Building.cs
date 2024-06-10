using System;
using System.Collections.Generic;
using System.Linq;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public class Building : ISpawnable {

    public class MissileSiloStats {
        public Building building;
        public int lastLaunchDay = -99;
        public int launchCooldown = 3;
        public int ammo = 999;
        public Vector2Int range = new(0, 999);
        public Vector2Int blastRange = new(0, 3);
        public int unitDamage = 5;
        public int bridgeDamage = 10;
        public bool hasMissile = true;
    }
    public class MissileStorageStats {
        public Building building;
        public int rocketsLeft = 3;
        public int lastRechargeDay = -99;
        public int rechargeCooldown = 3;
        [DontSave] public bool HasMissile => lastRechargeDay + rechargeCooldown <= building.level.Day();
    }

    public static readonly HashSet<Building> spawned = new();

    public TileType type;
    public Level level;
    public Vector2Int position;
    public Vector2Int lookDirection = Vector2Int.up;
    public MissileSiloStats missileSilo;
    public MissileStorageStats missileStorage;

    public int Cooldown(int day) {
        return Max(0, missileSilo.lastLaunchDay + missileSilo.launchCooldown - day);
    }

    private string viewPrefabName = "City";
    [DontSave] public BuildingView ViewPrefab {
        get => viewPrefabName.LoadAs<BuildingView>();
        set => viewPrefabName = value.name;
    }
    [DontSave] public BuildingView view;

    private Player player;
    [DontSave] public Player Player {
        get => player;
        set {
            if (IsSpawned && player == value)
                return;
            player = value;

            if (view) {
                view.PlayerColor = player?.Color ?? BuildingView.unownedColor;
                view.LightsColor = player?.Color ?? BuildingView.unownedLightsColor;
            }
        }
    }

    private int cp;
    [DontSave] public int Cp {
        get => cp;
        set {
            if (IsSpawned && cp == value)
                return;
            cp = Clamp(value, 0, IsSpawned ? MaxCp(this) : MaxCp(type));
        }
    }

    private bool moved;
    [DontSave] public bool Moved {
        get => moved;
        set {
            moved = value;
            if (view)
                view.Moved = value;
        }
    }

    [DontSave] public bool IsSpawned { get; private set; }

    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        spawned.Add(this);

        if (!ViewPrefab)
            ViewPrefab = BuildingView.GetPrefab(type);
        Assert.IsTrue(ViewPrefab);
        view = Object.Instantiate(ViewPrefab, level.view.transform);
        view.prefab = ViewPrefab;
        view.Position = position;
        if (type != TileType.Hq)
            view.LookDirection = lookDirection;

        Player = Player;
        Cp = Cp;
        Moved = Moved;

        IsSpawned = true;
    }

    public static implicit operator TileType(Building building) {
        return building.type;
    }

    public override string ToString() {
        return $"{type}{position} {Player}";
    }

    public void Despawn() {
        Assert.IsTrue(spawned.Contains(this));
        spawned.Remove(this);

        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }

        level.tiles.Remove(position);
        level.buildings.Remove(position);

        IsSpawned = false;
    }
}