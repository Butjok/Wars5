using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

public class Unit : IDisposable {

    public static readonly HashSet<Unit> undisposed = new();

    public readonly UnitType type;
    public readonly UnitView view;
    public readonly UnitBrain brain;
    private readonly bool initialized;

    private Vector2Int? position;
    public Vector2Int? Position {
        get => position;
        set {
            if (initialized) {
                if (position == value)
                    return;
                if (position is { } oldPosition)
                    player.level.units.Remove(oldPosition);
            }
            position = value;

            if (position is { } newPosition) {
                Assert.IsFalse(player.level.units.ContainsKey(newPosition), newPosition.ToString());
                player.level.units.Add(newPosition, this);
                view.Visible = true;
                view.Position = newPosition;
            }
            else
                view.Visible = false;
        }
    }
    public Vector2Int NonNullPosition {
        get {
            if (Position is not { } value)
                throw new AssertionException($"Unit {this} position is null.", null);
            return value;
        }
    }

    private bool moved;
    public bool Moved {
        get => moved;
        set {
            if (initialized && moved == value)
                return;
            moved = value;

            view.Moved = moved;
        }
    }

    private int hp;
    public int Hp => Hp(this, hp);

    public void SetHp(int value, bool animateDeath = false) {
        if (initialized && hp == value)
            return;

        hp = Clamp(value, 0, initialized ? MaxHp(this) : MaxHp(type));

        if (hp <= 0) {
            if (animateDeath)
                view.Die();
            Dispose();
        }
        else {
            view.Hp = hp;
            view.ui.SetHp(hp, MaxHp(this));
        }
    }

    private int fuel;
    public int Fuel {
        get => Fuel(this, fuel);
        set {
            if (initialized && fuel == value)
                return;
            fuel = Clamp(value, 0, initialized ? MaxFuel(this) : MaxFuel(type));

            //view.Fuel = fuel;
        }
    }

    private Player player;
    public Player Player {
        get => player;
        set {
            if (initialized && player == value)
                return;
            player = value;

            // alpha = 0!
            view.PlayerColor = player?.Color ?? new Color(0, 0, 0, 0);
        }
    }

    private Unit carrier;
    public Unit Carrier {
        get => carrier;
        set {
            if (initialized && carrier == value)
                return;
            carrier = value;
        }
    }

    private Dictionary<WeaponName, int> ammo = new();

    public void SetAmmo(WeaponName weaponName, int value) {

        var found = ammo.TryGetValue(weaponName, out var amount);
        Assert.IsTrue(found, weaponName.ToString());

        if (initialized && amount == value)
            return;
        ammo[weaponName] = Clamp(value, 0, initialized ? MaxAmmo(this, weaponName) : MaxAmmo(type, weaponName));

        // complete later
    }

    public int GetAmmo(WeaponName weaponName) {
        var found = ammo.TryGetValue(weaponName, out var amount);
        Assert.IsTrue(found, weaponName.ToString());
        return Ammo(this, weaponName, amount);
    }

    private List<Unit> cargo = new();
    public IReadOnlyList<Unit> Cargo => cargo;
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

    public Unit(Player player, UnitType type = UnitType.Infantry, Vector2Int? position = null, Vector2Int? lookDirection = null, int hp = int.MaxValue, int fuel = int.MaxValue, bool moved = false, UnitView viewPrefab = null
        ) {
        
        undisposed.Add(this);

        if (!viewPrefab)
            viewPrefab = UnitView.DefaultPrefab;
        Assert.IsTrue(viewPrefab);

        view = Object.Instantiate(viewPrefab, player.level.view.transform);
        view.prefab = viewPrefab;
        view.LookDirection = lookDirection ?? player.unitLookDirection;
        view.TrySpawnUi(UnitUi.Prefab, player.level.view);
        
        this.type = type;
        Player = player;
        Moved = moved;
        Assert.AreNotEqual(0, hp);
        SetHp(hp);
        Fuel = fuel;

        foreach (var weaponName in GetWeaponNames(type)) {
            ammo.Add(weaponName, 0);
            SetAmmo(weaponName, int.MaxValue);
        }

        Position = position;

        brain = new UnitBrain(this);

        
        initialized = true;
    }

    public void Dispose() {

        Assert.IsTrue(undisposed.Contains(this));

        foreach (var unit in cargo)
            unit.Dispose();
        undisposed.Remove(this);

        Position = null;
        Object.Destroy(view.gameObject);
    }

    public bool Disposed => !undisposed.Contains(this);

    public override string ToString() {
        return $"{type}{Position} {Player}";
    }
}

public static class EntityExtensions {
    public static Vector3 Raycast(this Vector2Int position2d) {
        return PlaceOnTerrain.TryRaycast(position2d, out var hit) ? hit.point : position2d.ToVector3Int();
    }
    public static Vector3 Raycast(this Vector2 position2d) {
        return PlaceOnTerrain.TryRaycast(position2d, out var hit) ? hit.point : position2d.ToVector3();
    }
}