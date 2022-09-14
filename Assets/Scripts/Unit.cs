using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;

public class Unit : IDisposable {

    public UnitType type;
    public Player player;
    public UnitView viewPrefab;
    public UnitView view;

    public ChangeTracker<Vector2Int?> position;
    public ChangeTracker<bool> moved;
    public ChangeTracker<int> hp;
    public ChangeTracker<int> fuel;
    public ListChangeTracker<int> ammo;
    public ListChangeTracker<Unit> cargo;
    public ChangeTracker<Unit> carrier;

    public static implicit operator UnitType(Unit unit) => unit.type;

    public Unit(Player player, bool moved = false, UnitType type = UnitType.Infantry, Vector2Int? position
        = null, Vector2Int? rotation = null, int hp = int.MaxValue, int fuel = int.MaxValue, UnitView viewPrefab = null) {

        if (!viewPrefab)
            viewPrefab = Resources.Load<UnitView>("test");
        Assert.IsTrue(viewPrefab);

        view = Object.Instantiate(viewPrefab);
        Object.DontDestroyOnLoad(view.gameObject);
        view.unit = this;
        view.prefab = viewPrefab;

        this.position = new ChangeTracker<Vector2Int?>(old => {

            if (old is { } oldPosition)
                player.game.units.Remove(oldPosition);

            if (this.position.v is { } newPosition) {
                player.game.units[newPosition] = this;
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
                if (this.position.v is not { } position)
                    throw new Exception();
                player.game.units.Remove(position);
            }
            else
                view.Hp = this.hp.v;
        });
        this.fuel = new ChangeTracker<int>(_ => view.Fuel = this.fuel.v);
        this.carrier = new ChangeTracker<Unit>(_ => view.Carrier = this.carrier.v);

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