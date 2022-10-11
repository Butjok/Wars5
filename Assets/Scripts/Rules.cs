using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Mathf;

[Flags]
public enum TileType {

    Plain = 1 << 0,
    Road = 1 << 1,
    Sea = 1 << 2,
    Mountain = 1 << 3,

    City = 1 << 4,
    Hq = 1 << 5,
    Factory = 1 << 6,
    Airport = 1 << 7,
    Shipyard = 1 << 8,
}

[Flags]
public enum UnitType {
    Infantry = 1 << 0,
    AntiTank = 1 << 1,
    Artillery = 1 << 2,
    Apc = 1 << 3,
    TransportHelicopter = 1 << 4,
    AttackHelicopter = 1 << 5,
    FighterJet = 1 << 6,
    Bomber = 1 << 7,
    Recon = 1 << 8,
    LightTank = 1 << 9,
}

public static class Rules {

    public static Vector2Int[] offsets = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

    public static int Income(TileType buildingType) {
        return 1000;
    }
    public static UnitType BuildableUnits(TileType buildingType) {
        return buildingType switch {
            TileType.Factory => UnitType.Infantry | UnitType.AntiTank | UnitType.Artillery | UnitType.Apc,
            TileType.Airport => UnitType.TransportHelicopter | UnitType.AttackHelicopter | UnitType.FighterJet | UnitType.FighterJet | UnitType.Bomber,
            _ => 0
        };
    }

    public static bool Lost(Player player) {
        var hasUnits = player.game.FindUnitsOf(player).Any();
        var buildings = player.game.FindBuildingsOf(player).ToList();
        var hasIncome = buildings.Any(building => Income(building) > 0);
        var canBuildUnits = buildings.Any(building => BuildableUnits(building) != 0);
        var hasHq = buildings.Any(building => building.type == TileType.Hq);
        return !hasHq ||
               !hasUnits && (!canBuildUnits || !hasIncome);
    }
    public static IEnumerable<Player> Enemies(Player player) {
        return player.game.players.Where(other => AreEnemies(player, other));
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
        switch (attacker: attackerType, target: targetType) {
            case (UnitType.Infantry, UnitType.Infantry):
                return 5;
            case (UnitType.Infantry, UnitType.AntiTank):
                return 5;
            case (UnitType.Infantry, UnitType.Artillery):
                return 3;
            case (UnitType.Infantry, UnitType.Apc):
                return 3;
        }
        return null;
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
    public static bool CanAttack(Unit attacker, Unit target, List<Vector2Int> path, int weaponIndex) {

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
    public static Vector2Int AttackRange(UnitType unitType) {
        if (((UnitType.Infantry | UnitType.AntiTank) & unitType) != 0)
            return new Vector2Int(1, 1);
        if (((UnitType.Artillery) & unitType) != 0)
            return new Vector2Int(2, 3);
        return Vector2Int.zero;
    }

    public static int WeaponsCount(UnitType unitType) {
        if (((UnitType.Infantry | UnitType.Artillery) & unitType) != 0)
            return 1;
        if (((UnitType.AntiTank) & unitType) != 0)
            return 2;
        return 0;
    }
    public static IEnumerable<int> Weapons(UnitType unitType) {
        for (var i = 0; i < WeaponsCount(unitType); i++)
            yield return i;
    }
    public static int MaxAmmo(UnitType type, int weaponIndex) {
        return 99;
    }
    public static int Ammo(Unit unit, int weaponIndex) {
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
        if (unitType == UnitType.Apc)
            return 1;
        return 0;
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
        return CanSupply(unit.type) && AreAllies(unit.player, target.player);
    }

    public static int MoveDistance(UnitType unitType) {
        if (((UnitType.Infantry) & unitType) != 0)
            return 7;
        if (((UnitType.AntiTank) & unitType) != 0)
            return 5;
        if (((UnitType.Artillery | UnitType.Apc) & unitType) != 0)
            return 5;
        return 0;
    }
    public static int MoveDistance(Unit unit) {
        return Min(unit.fuel.v, MoveDistance(unit.type));
    }
    public static int? MoveCost(UnitType unitType, TileType tileType) {

        int? foot = (TileType.Sea & tileType) != 0 ? null : 1;
        int? tires = null;
        int? tracks = ((TileType.Sea | TileType.Mountain) & tileType) != 0 ? null : 1;
        int? air = null;
        int? sea = null;

        if (((UnitType.Infantry | UnitType.AntiTank) & unitType) != 0)
            return foot;
        if (((UnitType.Artillery | UnitType.Apc) & unitType) != 0)
            return tracks;

        return null;
    }
    public static bool CanStay(UnitType unitType, TileType tileType) {
        return MoveCost(unitType, tileType) != null;
    }
    public static bool CanStay(Unit unit, Vector2Int position) {
        return unit.player.game.TryGetTile(position, out var tile) &&
               CanStay(unit, tile) &&
               (!unit.player.game.TryGetUnit(position, out var other) || other == unit);
    }
    public static bool CanCapture(UnitType unitType, TileType buildingType) {
        return ((UnitType.Infantry | UnitType.AntiTank) & unitType) != 0;
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