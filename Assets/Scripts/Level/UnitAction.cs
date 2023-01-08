using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public enum UnitActionType { Stay, Join, Capture, Attack, GetIn, Drop, Supply }
public class UnitAction : IDisposable {

    public static readonly HashSet<UnitAction> undisposed = new();
    
    public UnitActionType type;
    public Unit unit, targetUnit;
    public IReadOnlyList<Vector2Int> path;
    public int weaponIndex;
    public Vector2Int targetPosition;
    public UnitActionView view;

    public UnitAction(UnitActionType type, Unit unit, IReadOnlyList<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null, int weaponIndex = -1, Vector2Int targetPosition = default) {

        undisposed.Add(this);
        
        this.type = type;
        this.unit = unit;
        this.path = path;
        this.targetUnit = targetUnit;
        this.weaponIndex = weaponIndex;
        this.targetPosition = targetPosition;

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
        var attackDamage = Rules.Damage(unit, targetUnit, weaponIndex, unit.hp.v, targetUnit.hp.v);
        Assert.IsTrue(attackDamage != null);
        var targetHp = Mathf.Max(0, targetUnit.hp.v - (int)attackDamage);
        var attackerHp = unit.hp.v;
        if (targetHp > 0 && Rules.CanAttackInResponse(unit, targetUnit, out var responseWeaponIndex)) {
            var responseDamage = Rules.Damage(targetUnit, unit, responseWeaponIndex, targetHp, attackerHp);
            Assert.IsTrue(responseDamage != null);
            attackerHp = Mathf.Max(0, attackerHp - (int)responseDamage);
        }
        return (attackerHp, targetHp);
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);
        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }
    }

    public override string ToString() {
        var text = type.ToString();
        text += $" - {path[0]} -> {path.Last()}";
        if (targetUnit != null)
            text += $" - {targetUnit}";
        return text;
    }
}