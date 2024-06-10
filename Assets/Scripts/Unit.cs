using System.Collections.Generic;
using Butjok.CommandLine;
using JetBrains.Annotations;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public enum WeaponName {
    [UsedImplicitly] Rifle,
    [UsedImplicitly] RocketLauncher,
    [UsedImplicitly] Cannon,
    [UsedImplicitly] MachineGun,
}

public class Unit : ISpawnable {

    public static readonly HashSet<Unit> spawned = new();

    private bool hasMissile;
    [DontSave] public bool HasMissile {
        get => hasMissile;
        set {
            hasMissile = value;
            if (view)
                view.ShowMissileIcon = value;
        }
    }

    public UnitType type;
    [DontSave] public UnitView ViewPrefab {
        get => viewPrefabName.LoadAs<UnitView>();
        set => viewPrefabName = value.name;
    }
    public string viewPrefabName = "WbInfantry";
    [DontSave] public UnitView view;
    [DontSave] public UnitBrain brain;
    [DontSave] public bool IsSpawned { get; private set; }

    public List<UnitState> states2 = new();

    private Vector2Int? position;
    [DontSave] public Vector2Int? Position {
        get => position;
        set {
            if (IsSpawned) {
                if (position == value)
                    return;
                if (position is { } oldPosition)
                    player.level.units.Remove(oldPosition);
            }

            position = value;

            if (position is { } newPosition) {
                Assert.IsTrue(!player.level.units.ContainsKey(newPosition) || player.level.units[newPosition] == this, newPosition.ToString());
                player.level.units[newPosition] = this;
                if (view) {
                    view.Visible = true;
                    view.Position = newPosition;
                }
            }
            else if (view)
                view.Visible = false;
        }
    }

    [DontSave] public Vector2Int NonNullPosition {
        get {
            if (Position is not { } value)
                throw new AssertionException($"Unit {this} position is null.", null);
            return value;
        }
    }

    private bool moved;
    [DontSave] public bool Moved {
        get => moved;
        set {
            if (IsSpawned && moved == value)
                return;
            moved = value;
            if (view)
                view.Moved = moved;
        }
    }

    private int hp = 10;
    [DontSave] public int Hp {
        get => Hp(this, hp);
        set => SetHp(value);
    }

    public void SetHp(int value, bool animateDeath = false, bool spawnExplosionDecalUponDeath = true) {
        if (hp <= 0)
            return;

        hp = Clamp(value, 0, IsSpawned ? MaxHp(this) : MaxHp(type));

        if (hp <= 0) {
            if (animateDeath && view) {
                Position = null;
                view.Visible = true;
                Effects.SpawnExplosion(view.body.position, parent: player.level.view.transform);
                Sounds.PlayOneShot(Sounds.explosion);
                if (spawnExplosionDecalUponDeath)
                    ExplosionCrater.SpawnDecal(view.body.position.ToVector2(), parent: player.level.view.transform);
                player.level.view.cameraRig.Shake();
                view.AnimateDeath(false, Despawn);
            }
            else
                Despawn();
        }
        else if (view) {
            view.Hp = hp;
            view.ui.SetHp(hp, MaxHp(this));
        }
    }

    private int fuel = 999;
    [DontSave] public int Fuel {
        get => Fuel(this, fuel);
        set {
            if (IsSpawned && fuel == value)
                return;
            fuel = Clamp(value, 0, IsSpawned ? MaxFuel(this) : MaxFuel(type));

            //if (view)
            //    view.LowFuel = fuel < MaxFuel(this) / 4;
        }
    }

    private Player player;
    [DontSave] public Player Player {
        get => player;
        set {
            if (IsSpawned && player == value)
                return;
            player = value;

            // alpha = 0!
            if (view)
                view.PlayerColor = player?.Color ?? new Color(0, 0, 0, 0);
        }
    }

    private Unit carrier;
    [DontSave] public Unit Carrier {
        get => carrier;
        set {
            if (IsSpawned && carrier == value)
                return;
            carrier = value;
        }
    }

    public Vector2Int? lookDirection;

    private Dictionary<WeaponName, int> ammo = new();
    [DontSave] public IReadOnlyDictionary<WeaponName, int> Ammo => ammo;

    public void SetAmmo(WeaponName weaponName, int value) {
        ammo[weaponName] = Clamp(value, 0, IsSpawned ? MaxAmmo(this, weaponName) : MaxAmmo(type, weaponName));

        // complete later
    }

    public int GetAmmo(WeaponName weaponName) {
        var found = ammo.TryGetValue(weaponName, out var amount);
        Assert.IsTrue(found, weaponName.ToString());
        return Ammo(this, weaponName, amount);
    }

    private List<Unit> cargo = new();
    [DontSave] public IReadOnlyList<Unit> Cargo => cargo;

    public void AddCargo(Unit unit) {
        Assert.IsTrue(CanGetIn(unit, this), $"{unit} -> {this}");
        cargo.Add(unit);
        view.HasCargo = true;
    }

    public void RemoveCargo(Unit unit) {
        var index = cargo.IndexOf(unit);
        Assert.AreNotEqual(-1, index, unit.ToString());
        cargo.RemoveAt(index);
        view.HasCargo = cargo.Count > 0;
    }


    public static implicit operator UnitType(Unit unit) => unit.type;

    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        spawned.Add(this);

        if (!ViewPrefab)
            ViewPrefab = UnitView.DefaultPrefabFor(type);
        Assert.IsTrue(ViewPrefab);

        view = Object.Instantiate(ViewPrefab, player.level.view.transform);
        view.prefab = ViewPrefab;
        view.LookDirection = lookDirection ?? player.unitLookDirection;
        view.TrySpawnUi(UnitUi.Prefab, player.level.view);
        view.ConvertToSkinnedMesh();

        Player = Player;
        Moved = Moved;
        Assert.IsTrue(hp > 0);
        SetHp(Hp);
        Fuel = Fuel;

        if (Position is { } actualPosition) {
            view.Visible = true;
            view.Position = actualPosition;
        }
        else
            view.Visible = false;

        foreach (var weaponName in GetWeaponNames(type))
            if (!ammo.ContainsKey(weaponName))
                SetAmmo(weaponName, int.MaxValue);

        brain = new UnitBrain(this);

        lock (states2)
            states2 ??= new List<UnitState>();

        IsSpawned = true;
    }

    public void Despawn() {
        Assert.IsTrue(spawned.Contains(this));

        foreach (var unit in cargo)
            unit.Despawn();
        spawned.Remove(this);

        Position = null;
        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }

        IsSpawned = false;
    }

    public override string ToString() {
        return $"{type}{Position} {Player}";
    }
}