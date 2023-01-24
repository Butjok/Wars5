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
    public WeaponName weaponName;
    public Vector2Int targetPosition;
    public UnitActionView view;

    public UnitAction(UnitActionType type, Unit unit, IReadOnlyList<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null, WeaponName weaponName=default, Vector2Int targetPosition = default) {

        undisposed.Add(this);
        
        this.type = type;
        this.unit = unit;
        this.path = path;
        this.targetUnit = targetUnit;
        this.weaponName = weaponName;
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