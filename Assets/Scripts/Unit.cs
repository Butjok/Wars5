using System;
using System.Collections.Generic;
using System.Linq;
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

public class Unit : IDisposable {

    public static readonly HashSet<Unit> undisposed = new();

    public UnitType type;
    [DontSave] public UnitView viewPrefab;
    public string ViewPrefabName {
        get => viewPrefab ? viewPrefab.name : null;
        set => viewPrefab = Resources.Load<UnitView>(value);
    }
    [DontSave] public UnitView view;
    [DontSave] public UnitBrain brain;
    [DontSave] private bool initialized;

    public Stack<UnitAiState> aiStates = new();

    [DontSave] private Vector2Int? position;
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

    [DontSave] private bool moved;
    public bool Moved {
        get => moved;
        set {
            if (initialized && moved == value)
                return;
            moved = value;
            if (view)
                view.Moved = moved;
        }
    }

    [DontSave] private int hp = 10;
    public int Hp {
        get => Hp(this, hp);
        set => SetHp(value);
    }

    public void SetHp(int value, bool animateDeath = false) {
        if (initialized && hp == value)
            return;

        hp = Clamp(value, 0, initialized ? MaxHp(this) : MaxHp(type));

        if (hp <= 0) {
            Debug.Log("ded");
            if (animateDeath && view)
                view.DieOnMap();
            Dispose();
        }
        else if (view) {
            view.Hp = hp;
            view.ui.SetHp(hp, MaxHp(this));
        }
    }

    [DontSave] private int fuel = 999;
    public int Fuel {
        get => Fuel(this, fuel);
        set {
            if (initialized && fuel == value)
                return;
            fuel = Clamp(value, 0, initialized ? MaxFuel(this) : MaxFuel(type));

            //if (view)
            //    view.LowFuel = fuel < MaxFuel(this) / 4;
        }
    }

    [DontSave] private Player player;
    public Player Player {
        get => player;
        set {
            if (initialized && player == value)
                return;
            player = value;

            // alpha = 0!
            if (view)
                view.PlayerColor = player?.Color ?? new Color(0, 0, 0, 0);
        }
    }

    [DontSave] private Unit carrier;
    public Unit Carrier {
        get => carrier;
        set {
            if (initialized && carrier == value)
                return;
            carrier = value;
        }
    }

    public Vector2Int? lookDirection;

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

    public void Initialize() {
        Assert.IsFalse(initialized);
        Assert.IsFalse(undisposed.Contains(this));
        undisposed.Add(this);

        if (!viewPrefab)
            viewPrefab = UnitView.DefaultPrefabFor(type);
        Assert.IsTrue(viewPrefab);

        view = Object.Instantiate(viewPrefab, player.level.view.transform);
        view.prefab = viewPrefab;
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

        foreach (var weaponName in GetWeaponNames(type)) {
            ammo.Add(weaponName, 0);
            SetAmmo(weaponName, int.MaxValue);
        }

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
        view = null;

        initialized = false;
    }

    [DontSave] public bool Disposed => !undisposed.Contains(this);

    public override string ToString() {
        return $"{type}{Position} {Player}";
    }
}