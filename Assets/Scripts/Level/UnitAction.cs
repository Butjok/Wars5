using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public enum UnitActionType { Stay, Join, Capture, Attack, GetIn, Drop, Supply }
public class UnitAction : IDisposable {

    public UnitActionType type;
    public Unit unit, targetUnit;
    public MovePath path;
    public int weaponIndex;
    public Vector2Int targetPosition;
    public UnitActionView view;

    public static readonly HashSet<UnitAction> undisposed = new();

    public UnitAction(UnitActionType type, Unit unit, MovePath path, Unit targetUnit = null, Building targetBuilding = null, int weaponIndex = -1, Vector2Int targetPosition = default) {

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

    public IEnumerator Execute() {

        Assert.IsTrue(path.Count >= 1);

        unit.player.game.TryGetUnit(path.Destination, out var unitAtPathEnd);
        unit.player.game.TryGetBuilding(path.Destination, out var buildingAtPathEnd);

        unit.moved.v = true;

        Debug.Log($"EXECUTING: {unit} {type} {targetUnit}");

        switch (type) {

            case UnitActionType.Stay: {
                unit.position.v = path.Destination;
                break;
            }

            case UnitActionType.Join: {
                unitAtPathEnd.hp.v = Mathf.Min(Rules.MaxHp(unitAtPathEnd), unitAtPathEnd.hp.v + unit.hp.v);
                unit.Dispose();
                unit = null;
                break;
            }

            case UnitActionType.Capture: {
                unit.position.v = path.Destination;
                buildingAtPathEnd.cp.v -= Rules.Cp(unit);
                if (buildingAtPathEnd.cp.v <= 0) {
                    buildingAtPathEnd.player.v = unit.player;
                    buildingAtPathEnd.cp.v = Rules.MaxCp(buildingAtPathEnd);
                }
                break;
            }

            case UnitActionType.Attack:
                return BattleAnimationState.New(this);

            case UnitActionType.GetIn: {
                unit.position.v = null;
                unit.carrier.v = unitAtPathEnd;
                unitAtPathEnd.cargo.Add(unit);
                break;
            }

            case UnitActionType.Drop: {
                unit.position.v = path.Destination;
                unit.cargo.Remove(targetUnit);
                targetUnit.position.v = targetPosition;
                targetUnit.carrier.v = null;
                break;
            }

            case UnitActionType.Supply: {
                unit.position.v = path.Destination;
                targetUnit.fuel.v = Rules.MaxFuel(targetUnit);
                foreach (var weaponIndex in Rules.Weapons(targetUnit))
                    targetUnit.ammo[weaponIndex] = Rules.MaxAmmo(targetUnit, weaponIndex);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public override string ToString() {
        var text = type.ToString();
        text += $" - {path[0]} -> {path.Destination}";
        if (targetUnit != null)
            text += $" - {targetUnit}";
        return text;
    }
}