using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Mathf;

public static class Rules {

    public static Vector2Int[] offsets = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

    public static int Income(TileType buildingType) {
        return 1000;
    }
    public static UnitType BuildableUnits(TileType buildingType) {
        return buildingType switch {
            TileType.Factory => UnitType.Infantry | UnitType.AntiTank | UnitType.Artillery | UnitType.Apc,
            _ => 0
        };
    }

    public static bool Lost(Player player) {
        var hasUnits = player.main.FindUnitsOf(player).Any();
        var buildings = player.main.FindBuildingsOf(player).ToList();
        var hasIncome = buildings.Any(building => Income(building) > 0);
        var canBuildUnits = buildings.Any(building => BuildableUnits(building) != 0);
        var hasHq = buildings.Any(building => building.type == TileType.Hq);
        return !hasHq ||
               !hasUnits && (!canBuildUnits || !hasIncome);
    }
    public static IEnumerable<Player> Enemies(Player player) {
        return player.main.players.Where(other => AreEnemies(player, other));
    }
    public static bool Won(Player player) {
        return Enemies(player).All(Lost);
    }
    public static bool AreEnemies(Player p1, Player pl2) {
        return (p1.team & pl2.team) == 0;
    }
    public static bool AreAllies(Player p1, Player pl2) {
        return !AreEnemies(p1, pl2);
    }

    public static int MaxAbilityMeter(Player player) {
        return 6;
    }
    public static bool CanUseAbility(Player player) {
        return !AbilityInUse(player) && player.powerMeter == MaxAbilityMeter(player);
    }
    public static bool AbilityInUse(Player player) {
        return player.abilityActivationTurn != null;
    }
    
    public static int MaxCp(TileType buildingType) {
        return 20;
    }
    public static int Cp(Unit unit) {
        return (unit.hp.v);
    }
    public static int MaxHp(UnitType unitType) {
        return 10;
    }
    public static int MaxFuel(UnitType unitType) {
        return 99;
    }
    public static int? Damage(UnitType attackerType, UnitType targetType, int weaponIndex) {
        Assert.IsTrue(weaponIndex < WeaponsCount(attackerType));
        return (attacker: attackerType, target: targetType) switch {
            (UnitType.Infantry, UnitType.Infantry) => 5,
            (UnitType.Infantry, UnitType.AntiTank) => 5,
            (UnitType.Infantry, UnitType.Artillery) => 3,
            (UnitType.Infantry, UnitType.Apc) => 3,
            _ => null
        };
    }
    public static int Cost(UnitType unitType, Player player) {
        return unitType switch {
            UnitType.Infantry => 1000,
            UnitType.AntiTank => 2000,
            UnitType.Artillery => 5000,
            UnitType.Apc => 5000,
            UnitType.Recon => 3000,
            UnitType.LightTank => 5000,
            UnitType.MediumTank => 8000,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }
    public static bool CanAfford(this Player player, UnitType unitType) {
        return player.credits >= Cost(unitType, player);
    }
    public static int? Damage(Unit attacker, Unit target, int weaponIndex, int? attackerHp = null, int? targetHp = null) {
        if (Ammo(attacker, weaponIndex) <= 0 || Damage(attacker.type, target.type, weaponIndex) is not { } baseDamage)
            return null;
        return CeilToInt((float)(attackerHp ?? attacker.hp.v) / MaxHp(attacker) * baseDamage);
    }
    public static int? PreferredWeapon(Unit attacker, Unit target) {
        if (WeaponsCount(attacker) == 0)
            return null;
        var bestWeapon = 0;
        var bestDamage = 0;
        foreach (var weaponIndex in Weapons(attacker)) {
            if (Damage(attacker, target, weaponIndex) is not { } damage)
                continue;
            if (damage > bestDamage) {
                bestDamage = damage;
                bestWeapon = weaponIndex;
            }
        }
        return bestWeapon;
    }
    public static bool CanAttack(UnitType attackerType, UnitType targetType, int weaponIndex) {
        Assert.IsTrue(weaponIndex < WeaponsCount(attackerType));
        return Damage(attackerType, targetType, weaponIndex) != null;
    }
    public static bool CanAttack(Unit attacker, Unit target, IReadOnlyList<Vector2Int> path, int weaponIndex) {

        Assert.IsTrue(path.Count >= 1);
        Assert.IsTrue(weaponIndex < WeaponsCount(attacker));
        var _targetPosiiton = target.position.v;
        Assert.IsTrue(_targetPosiiton != null);
        var targetPosition = (Vector2Int)_targetPosiiton;
        Assert.IsTrue(MathUtils.ManhattanDistance(path.Last(), targetPosition).IsIn(AttackRange(attacker)));

        return CanAttack(attacker.type, target.type, weaponIndex) &&
               AreEnemies(attacker.player, target.player) &&
               Ammo(attacker, weaponIndex) > 0 &&
               (!IsArtillery(attacker) || path.Count == 1);
    }
    public static bool IsArtillery(UnitType unitType) {
        return (UnitType.Artillery & unitType) != 0;
    }
    
    public static bool CanAttackInResponse(UnitType unitType) {
        return !IsArtillery(unitType);
    }
    
    public static bool CanAttackInResponse(Unit attacker, Unit target, out int weaponIndex) {

        if (!CanAttackInResponse(target.type)) {
            weaponIndex = -1;
            return false;
        }

        int? maxDamage = null;
        weaponIndex = -1;

        for (var i = 0; i < WeaponsCount(attacker); i++)
            if (Damage(attacker, target, i, attacker.hp.v, target.hp.v) is { } damage && (maxDamage == null || maxDamage < damage)) {
                maxDamage = damage;
                weaponIndex = i;
            }
        return false;
    }
    public static Vector2Int AttackRange(UnitType unitType, Player player) {
        return unitType switch {
            UnitType.Infantry or UnitType.AntiTank or UnitType.LightTank or UnitType.Recon => new Vector2Int(1,1),
            UnitType.Artillery => new Vector2Int(2,3),
            _ => Vector2Int.zero
        };
    }
    public static Vector2Int AttackRange(Unit unit) {
        return AttackRange(unit.type, unit.player);
    }

    public static int WeaponsCount(UnitType unitType) {
        return unitType switch {
            UnitType.Infantry or UnitType.Artillery => 1,
            UnitType.AntiTank => 2,
            _ => 0
        };
    }
    public static IEnumerable<int> Weapons(UnitType unitType) {
        for (var i = 0; i < WeaponsCount(unitType); i++)
            yield return i;
    }
    public static int MaxAmmo(UnitType type, int weaponIndex) {
        return 99;
    }
    public static int Ammo(Unit unit, int weaponIndex) {
        Assert.IsTrue(weaponIndex >= 0);
        Assert.IsTrue(weaponIndex < WeaponsCount(unit));
        Assert.IsTrue(weaponIndex < unit.ammo.Count);
        return unit.ammo[weaponIndex];
    }
    public static bool CanLoadAsCargo(UnitType receiverType, UnitType targetType) {
        if (receiverType == UnitType.Apc && ((UnitType.Infantry | UnitType.AntiTank) & targetType) != 0)
            return true;
        return false;
    }
    public static int Size(UnitType unitType) {
        return 1;
    }
    public static int CargoCapacity(UnitType unitType) {
        return unitType switch {
            UnitType.Apc => 1,
            _ => 0
        };
    }
    public static bool CanLoadAsCargo(Unit receiver, Unit target) {
        var cargoSize = receiver.cargo.Sum(u => Size(u));
        return CanLoadAsCargo(receiver.type, target.type) &&
               AreAllies(receiver.player, target.player) &&
               cargoSize + Size(target) <= CargoCapacity(receiver);
    }
    public static bool CanSupply(UnitType unitType) {
        return unitType == UnitType.Apc;
    }
    public static bool CanSupply(Unit unit, Unit target) {
        return CanSupply(unit.type) && AreAllies(unit.player, target.player) && (unit!=target);
    }

    public static int MoveDistance(UnitType unitType, Player player) {
        return unitType switch {
            UnitType.Infantry => 3,
            UnitType.AntiTank => 2,
            UnitType.LightTank => 5,
            UnitType.MediumTank => 4,
            UnitType.Artillery  or UnitType.Apc or UnitType.Recon or UnitType.Rockets => 5,
            _ => 0
        };
    }
    public static int MoveDistance(Unit unit) {
        return Min(unit.fuel.v, MoveDistance(unit.type,unit.player));
    }
    public static int? MoveCost(UnitType unitType, TileType tileType) {

        int? foot = tileType switch {
            TileType.Sea => null,
            TileType.Mountain => 2,
            _ => 1
        };
        int? tracks = tileType switch {
            TileType.Sea or TileType.Mountain or TileType.River => null,
            TileType.Forest => 2,
            _ => 1
        };
        int? tires = tileType switch {
            TileType.Sea or TileType.Mountain or TileType.River => null,
            TileType.Forest => 3,
            TileType.Plain => 2,
            _ => 1
        };
        int? air = null;
        int? sea = null;

        return unitType switch {
            UnitType.Infantry or UnitType.AntiTank => foot,
            UnitType.Artillery or UnitType.LightTank or UnitType.Apc or UnitType.MediumTank => tracks,
            UnitType.Recon or UnitType.Rockets => tires,
            _ => null
        };
    }
    public static bool CanStay(UnitType unitType, TileType tileType) {
        return MoveCost(unitType, tileType) != null;
    }
    public static bool CanStay(Unit unit, Vector2Int position) {
        return unit.player.main.TryGetTile(position, out var tile) &&
               CanStay(unit, tile) &&
               (!unit.player.main.TryGetUnit(position, out var other) || other == unit);
    }
    public static bool CanCapture(UnitType unitType, TileType buildingType) {
        return unitType switch {
            UnitType.Infantry or UnitType.AntiTank => true,
            _ => false
        };
    }
    public static bool CanCapture(Unit unit, Building building) {
        return CanCapture(unit.type, building.type) &&
               (building.player.v == null || AreEnemies(unit.player, building.player.v));
    }
    public static bool CanPass(Unit unit, Unit other) {
        return AreAllies(unit.player, other.player);
    }
    public static bool CanJoin(Unit unit, Unit other) {
        return other != unit && unit.player == other.player && other.hp.v < MaxHp(other);
    }
}