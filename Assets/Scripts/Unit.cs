using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;

public class Unit : IDisposable {

    public static readonly HashSet<Unit> undisposed = new();

    public UnitType type;
    public Player player;
    public UnitView view;

    public ChangeTracker<Vector2Int?> position;
    public ChangeTracker<bool> moved;
    public ChangeTracker<int> hp;
    public ChangeTracker<int> fuel;
    public ListChangeTracker<int> ammo;
    public ListChangeTracker<Unit> cargo;
    public ChangeTracker<Unit> carrier;

    public static implicit operator UnitType(Unit unit) => unit.type;

    public Unit(Player player, UnitType type = UnitType.Infantry, Vector2Int? position = null, Vector2Int? lookDirection = null, int hp = int.MaxValue, int fuel = int.MaxValue, bool moved = false, UnitView viewPrefab = null) {

        undisposed.Add(this);

        if (!viewPrefab)
            viewPrefab = UnitView.DefaultPrefab;
        Assert.IsTrue(viewPrefab);

        view = Object.Instantiate(viewPrefab, player.main.transform);
        view.unit = this;
        view.prefab = viewPrefab;
        view.LookDirection = lookDirection ?? player.unitLookDirection;
        view.PlayerColor = player.color;

        this.position = new ChangeTracker<Vector2Int?>(old => {

            if (old is { } oldPosition)
                player.main.units.Remove(oldPosition);

            if (this.position.v is { } newPosition) {
                Assert.IsFalse(player.main.units.ContainsKey(newPosition), newPosition.ToString());
                player.main.units.Add(newPosition, this);
                view.Visible = true;
                view.Position = newPosition;
            }
            else
                view.Visible = false;
        });

        this.moved = new ChangeTracker<bool>(_ => view.Moved = this.moved.v);

        this.hp = new ChangeTracker<int>(_ => {
            if (this.hp.v <= 0) {
                view.Die();
                Assert.IsTrue(this.position.v != null);
                player.main.units.Remove((Vector2Int)this.position.v);
            }
            else
                view.Hp = this.hp.v;
        });

        this.fuel = new ChangeTracker<int>(_ => view.Fuel = this.fuel.v);
        carrier = new ChangeTracker<Unit>(_ => view.Carrier = carrier.v);

        this.type = type;
        this.player = player;
        this.moved.v = moved;
        Assert.AreNotEqual(0, hp);
        this.hp.v = Clamp(hp, 0, Rules.MaxHp(type));
        this.fuel.v = Clamp(fuel, 0, Rules.MaxFuel(type));

        ammo = new ListChangeTracker<int>(onChange: (_, _) => view.LowAmmo = ammo.Any(count => count <= 3));
        for (var weapon = 0; weapon < Rules.WeaponsCount(type); weapon++)
            ammo.Add(Rules.MaxAmmo(type, weapon));

        cargo = new ListChangeTracker<Unit>(
            onAdd: (_, _) => view.HasCargo = cargo.Count > 0,
            onRemove: (_, _) => view.HasCargo = cargo.Count > 0);

        this.position.v = position;
    }

    public void Dispose() {

        foreach (var unit in cargo)
            unit.Dispose();

        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);

        position.v = null;
        if (view && view.gameObject) {
            Object.Destroy(view.gameObject);
            view = null;
        }
    }

    public override string ToString() {
        return $"{type}{position.v} {player}";
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