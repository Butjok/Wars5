using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public enum UnitActionType {

    Stay,
    Join, //
    Capture, //
    Attack, //
    GetIn,
    Drop,
    Supply, //
    LaunchMissile, //

    // AI extensions
    Gather, // move to a closest gathering point 
}

public class UnitAction : IDisposable {

    public static readonly HashSet<UnitAction> undisposed = new();

    public readonly UnitActionType type;
    public readonly Unit unit, targetUnit;
    public List<Vector2Int> path;
    public readonly WeaponName weaponName;
    public readonly Vector2Int targetPosition;
    public readonly UnitActionView view;
    public readonly Building targetBuilding;

    public UnitAction(
        UnitActionType type,
        Unit unit, IEnumerable<Vector2Int> path,
        Unit targetUnit = null, Building targetBuilding = null,
        WeaponName weaponName = default, Vector2Int targetPosition = default,
        bool spawnView = false) {

        undisposed.Add(this);

        this.type = type;
        this.unit = unit;

        this.targetUnit = targetUnit;
        this.weaponName = weaponName;
        this.targetPosition = targetPosition;
        this.targetBuilding = targetBuilding;

        this.path = path.ToList();
        Assert.AreNotEqual(0, this.path.Count);
        Assert.AreEqual(unit.Position, this.path[0]);

        if (spawnView)
            switch (type) {
                case UnitActionType.Attack: {
                    //var view = Object.Instantiate(UnitAttackActionView.Prefab, unit.Player.level.view.transform);
                    //view.action = this;
                    //this.view = view;
                    break;
                }
            }
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);
        if (view)
            Object.Destroy(view.gameObject);
    }

    public override string ToString() {
        var text = type.ToString();
        if (targetUnit != null)
            text += $" {targetUnit}";
        if (type == UnitActionType.Drop)
            text += $" to {targetPosition}";
        if (type == UnitActionType.Attack)
            text += $" with {weaponName}";
        return text;
    }
}