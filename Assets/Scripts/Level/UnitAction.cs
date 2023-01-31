using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public enum UnitActionType {
    Stay,
    Join,
    Capture,
    Attack,
    GetIn,
    Drop,
    Supply,
    LaunchMissile
}

public class UnitAction : IDisposable {

    public static readonly Traverser traverser = new();

    public static readonly HashSet<UnitAction> undisposed = new();

    public readonly UnitActionType type;
    public readonly Unit unit, targetUnit;
    private List<Vector2Int> path;
    public readonly Vector2Int destination;
    private bool triedToFindPath;
    public IReadOnlyList<Vector2Int> Path {
        get {
            if (path != null || triedToFindPath)
                return path;
            traverser.Traverse(unit, destination);
            traverser.TryReconstructPath(destination, ref path);
            triedToFindPath = true;
            return path;
        }
    }
    public readonly WeaponName weaponName;
    public readonly Vector2Int targetPosition;
    public readonly UnitActionView view;
    public readonly Building targetBuilding;

    public UnitAction(
        UnitActionType type,
        Unit unit, IEnumerable<Vector2Int> path = null,
        Unit targetUnit = null, Building targetBuilding = null,
        WeaponName weaponName = default, Vector2Int targetPosition = default,
        bool spawnView = false, Vector2Int? destination = null) {

        undisposed.Add(this);

        this.type = type;
        this.unit = unit;

        this.targetUnit = targetUnit;
        this.weaponName = weaponName;
        this.targetPosition = targetPosition;
        this.targetBuilding = targetBuilding;

        Assert.IsTrue(path != null && destination == null ||
                      path == null && destination != null);

        if (path != null) {
            this.path = path.ToList();
            Assert.AreNotEqual(0, this.path.Count);
            Assert.AreEqual(unit.Position, this.path[0]);
            this.destination = this.path[^1];
        }
        else if (destination is { } actualDestination)
            this.destination = actualDestination;

        if (spawnView)
            switch (type) {
                case UnitActionType.Attack: {
                    var view = Object.Instantiate(UnitAttackActionView.Prefab);
                    view.action = this;
                    this.view = view;
                    break;
                }
            }
    }

    public (int attacker, int target) CalculateHpsAfterAttack() {
        Assert.AreEqual(UnitActionType.Attack, type);
        var isValid = Rules.TryGetDamage(unit, targetUnit, weaponName, out var attackDamage);
        Assert.IsTrue(isValid);
        var targetHp = Mathf.Max(0, targetUnit.Hp - (int)attackDamage);
        var attackerHp = unit.Hp;
        /*if (targetHp > 0 && Rules.CanAttackInResponse(unit, targetUnit, out var responseWeaponIndex)) {
            var responseDamage = Rules.Damage(targetUnit, unit, responseWeaponIndex, targetHp, attackerHp);
            Assert.IsTrue(responseDamage != null);
            attackerHp = Mathf.Max(0, attackerHp - (int)responseDamage);
        }*/
        return (attackerHp, targetHp);
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
        return text;
    }
}