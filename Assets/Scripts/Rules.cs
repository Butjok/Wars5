using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Mathf;

public static class Rules {

    public static Vector2Int[] gridOffsets = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

    public static int Income(TileType buildingType) {
        return 1000;
    }
    public static int Income(Building building) {
        return Income(building.type);
    }

    public static IEnumerable<UnitType> GetBuildableUnitTypes(TileType buildingType) {
        switch (buildingType) {
            case TileType.Factory:
                yield return UnitType.Infantry;
                yield return UnitType.AntiTank;
                yield return UnitType.Artillery;
                yield return UnitType.Apc;
                yield return UnitType.Recon;
                yield return UnitType.LightTank;
                yield return UnitType.MediumTank;
                yield return UnitType.Rockets;
                break;
        }
    }

    public static bool Lost(Player player) {
        var hasUnits = player.level.FindUnitsOf(player).Any();
        var buildings = player.level.FindBuildingsOf(player).ToList();
        var hasIncome = buildings.Any(building => Income(building) > 0);
        var canBuildUnits = buildings.Any(building => GetBuildableUnitTypes(building).Any() && !player.level.TryGetUnit(building.position, out _));
        var hasHq = buildings.Any(building => building.type == TileType.Hq);
        return !hasHq ||
               !hasUnits && (!canBuildUnits || !hasIncome);
    }
    public static IEnumerable<Player> Enemies(Player player) {
        return player.level.players.Where(other => AreEnemies(player, other));
    }
    public static bool Won(Player player) {
        return Enemies(player).All(Lost);
    }
    public static bool AreEnemies(Player p1, Player p2) {
        return p1 != p2 && (p1.team == Team.None || p2.team == Team.None || p1.team != p2.team);
    }
    public static bool AreAllies(Player p1, Player p2) {
        return !AreEnemies(p1, p2);
    }

    public const int defaultMaxCredits = 16000;
    public static int MaxCredits(Player player) {
        return player.maxCredits;
    }

    public const int defaultMaxAbilityMeter = 6;
    public static int MaxAbilityMeter(Player player) {
        return defaultMaxAbilityMeter;
    }
    public static bool CanUseAbility(Player player) {
        return !AbilityInUse(player) && player.AbilityMeter == MaxAbilityMeter(player);
    }
    public static bool AbilityInUse(Player player) {
        return player.abilityActivationTurn != null;
    }

    public static int MaxCp(Building building) {
        return MaxCp(building.type);
    }
    public static int MaxCp(TileType buildingType) {
        return 20;
    }
    public static int Cp(Unit unit) {
        return unit.Hp;
    }

    public static int MaxHp(Unit unit) {
        return MaxHp(unit.type);
    }
    public static int MaxHp(UnitType unitType) {
        return 10;
    }
    public static int Hp(Unit unit, int originalValue) {
        return originalValue;
    }

    public static int MaxFuel(UnitType unitType) {
        return UnitStats.Loaded.TryGetValue(unitType, out var entry) ? entry.fuel : 99;
    }
    public static int MaxFuel(Unit unit) {
        return MaxFuel(unit.type);
    }
    public static int Fuel(Unit unit, int originalValue) {
        return originalValue;
    }
    public static int Ammo(Unit unit, WeaponName weaponName, int originalAmount) {
        return originalAmount;
    }

    public static IEnumerable<WeaponName> GetWeaponNames(UnitType type) {
        return UnitStats.Loaded.TryGetValue(type, out var entry) ? entry.ammo.Keys : Enumerable.Empty<WeaponName>();
    }

    public static bool TryGetDamage(UnitType attackerType, UnitType targetType, WeaponName weaponName, out float damagePercentage) {
        return DamageTable.Loaded.TryGetValue((attackerType, targetType, weaponName), out damagePercentage);
    }
    public static bool TryGetDamage(Unit attacker, Unit target, WeaponName weaponName, out float damagePercentage) {
        damagePercentage = 0;
        if (!AreEnemies(attacker.Player, target.Player) || attacker.GetAmmo(weaponName) <= 0 || !TryGetDamage(attacker.type, target.type, weaponName, out var baseDamagePercentage))
            return false;
        damagePercentage = (float)(attacker.Hp) / MaxHp(attacker) * baseDamagePercentage;
        return true;
    }

    public static IEnumerable<(WeaponName weaponName, float damage)> GetDamageValues(Unit attacker, Unit target) {
        foreach (var weaponName in GetWeaponNames(attacker))
            if (TryGetDamage(attacker, target, weaponName, out var damage))
                yield return (weaponName, damage);
    }

    public static int Cost(UnitType unitType, Player player = null) {
        var found = UnitStats.Loaded.TryGetValue(unitType, out var entry);
        Assert.IsTrue(found, unitType.ToString());
        return entry.cost;
    }
    public static bool CanAfford(this Player player, UnitType unitType) {
        return player.Credits >= Cost(unitType, player);
    }

    public static bool CanAttack(Unit attacker, Vector2Int attackerPosition, bool isMoveAttack, Unit target, Vector2Int targetPosition, WeaponName weaponName) {

        if (TryGetAttackRange(attacker, out var attackRange) && !MathUtils.ManhattanDistance(attackerPosition, targetPosition).IsInRange(attackRange))
            return false;

        return AreEnemies(attacker.Player, target.Player) &&
               TryGetDamage(attacker, target, weaponName, out _) &&
               (!IsArtillery(attacker) || !isMoveAttack);
    }
    public static bool IsArtillery(UnitType unitType) {
        return unitType is UnitType.Artillery or UnitType.Rockets;
    }
    public static bool CanAttackInResponse(UnitType unitType) {
        return !IsArtillery(unitType);
    }
    public static IEnumerable<WeaponName> GetWeaponNamesForResponseAttack(Unit attacker, Vector2Int attackerPosition, Unit target, Vector2Int targetPosition) {
        if (!CanAttackInResponse(target.type))
            yield break;
        foreach (var weaponName in GetWeaponNames(target)) {
            if (CanAttack(target, targetPosition, false, attacker, attackerPosition, weaponName))
                yield return weaponName;
        }
    }

    public static bool TryGetAttackRange(UnitType unitType, Player player, out Vector2Int attackRange) {
        attackRange = UnitStats.Loaded.TryGetValue(unitType, out var entry) ? entry.attackRange : Vector2Int.zero;
        return attackRange != Vector2Int.zero;
    }
    public static bool TryGetAttackRange(Unit unit, out Vector2Int attackRange) {
        return TryGetAttackRange(unit.type, unit.Player, out attackRange);
    }

    public static int MaxAmmo(Unit unit, WeaponName weaponName) {
        return MaxAmmo(unit.type, weaponName);
    }
    public static int MaxAmmo(UnitType type, WeaponName weaponName) {
        return UnitStats.Loaded.TryGetValue(type, out var entry) && entry.ammo.TryGetValue(weaponName, out var amount)
            ? amount
            : 99;
    }
    public static bool CanGetIn(UnitType unitType, UnitType carrierType) {
        return UnitStats.Loaded.TryGetValue(carrierType, out var entry) && entry.canCarry.Contains(unitType);
    }
    public static int Weight(UnitType unitType) {
        return UnitStats.Loaded.TryGetValue(unitType, out var entry) ? entry.weight : 1;
    }
    public static int CarryCapacity(UnitType unitType) {
        return UnitStats.Loaded.TryGetValue(unitType, out var entry) ? entry.carryCapacity : 0;
    }
    public static bool CanGetIn(Unit unit, Unit carrier) {
        var cargoSize = carrier.Cargo.Sum(u => Weight(u));
        return CanGetIn(unit.type, carrier.type) &&
               (unit.Carrier == null || unit.Carrier == carrier) &&
               AreAllies(carrier.Player, unit.Player) &&
               cargoSize + Weight(unit) <= CarryCapacity(carrier);
    }
    public static bool CanSupply(UnitType unitType) {
        return UnitStats.Loaded.TryGetValue(unitType, out var entry) && entry.specialCommands.HasFlag(UnitStats.SpecialCommand.CanSupply);
    }
    public static bool CanSupply(Unit unit, Unit target) {
        return CanSupply(unit.type) && AreAllies(unit.Player, target.Player) && (unit != target);
    }

    public static int MoveCapacity(Unit unit) {
        var moveDistance = MoveCapacity(unit.type, unit.Player);
        if (unit.Player.coName == PersonName.Natalie && unit.Player.abilityActivationTurn != null)
            moveDistance += 5;
        return Min(unit.Fuel, moveDistance);
    }

    public static int MoveCapacity(UnitType unitType, Player player) {
        return UnitStats.Loaded.TryGetValue(unitType, out var settings) ? settings.moveCapacity : 0;
    }
    
    /*
     * TODO: remove TryGetMoveCost by unit type, use TryGetMoveCost by move type
     */

    public static bool TryGetMoveCost(Unit unit, Vector2Int position, out int cost) {
        cost = default;
        return unit.Player.level.TryGetTile(position, out var tileType) &&
               TryGetMoveCost(unit.type, tileType, out cost);
    }
    public static bool TryGetMoveCost(UnitType unitType, TileType tileType, out int cost) {
        const int unreachable = -1;
        if (!UnitStats.Loaded.TryGetValue(unitType, out var entry) || !TryGetMoveCost(entry.moveType, tileType, out cost))
            cost = unreachable;
        return cost != unreachable;
    }
    public static bool TryGetMoveCost(MoveType moveType,TileType tileType, out int cost) {
        const int unreachable = -1;
        cost = moveType switch {
            MoveType.Foot => tileType switch {
                TileType.Sea => unreachable,
                TileType.Forest or TileType.Mountain or TileType.River => 2,
                _ => 1
            },
            MoveType.Tires => tileType switch {
                TileType.Sea or TileType.Mountain or TileType.River => unreachable,
                TileType.Forest => 3,
                TileType.Plain => 2,
                _ => 1
            },
            MoveType.Tracks => tileType switch {
                TileType.Sea or TileType.Mountain or TileType.River => unreachable,
                TileType.Forest => 2,
                _ => 1
            },
            MoveType.Air => 1,
            _ => unreachable
        };
        return cost != unreachable;
    }

    public static MoveType GetMoveType(UnitType unitType) {
        return unitType switch {
            UnitType.Infantry or UnitType.AntiTank => MoveType.Foot,
            UnitType.Artillery or UnitType.Apc or UnitType.LightTank or UnitType.MediumTank => MoveType.Tracks,
            UnitType.Recon or UnitType.Rockets => MoveType.Tires,
            UnitType.TransportHelicopter or UnitType.AttackHelicopter or UnitType.FighterJet or UnitType.Bomber => MoveType.Air,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }

    public static bool CanStay(UnitType unitType, TileType tileType) {
        return TryGetMoveCost(unitType, tileType, out _);
    }
    public static bool CanStay(Unit unit, Vector2Int position) {
        return unit.Player.level.TryGetTile(position, out var tile) &&
               CanStay(unit, tile) &&
               (!unit.Player.level.TryGetUnit(position, out var other) || other == unit);
    }
    public static bool CanCapture(UnitType unitType, TileType buildingType) {
        return UnitStats.Loaded.TryGetValue(unitType, out var entry) && entry.specialCommands.HasFlag(UnitStats.SpecialCommand.CanCapture);
    }
    public static bool CanCapture(Unit unit, Building building) {
        return CanCapture(unit.type, building.type) &&
               (building.Player == null || AreEnemies(unit.Player, building.Player));
    }
    public static bool CanPass(Unit unit, Vector2Int position) {
        return !unit.Player.level.TryGetUnit(position, out var other) || unit.Player == other.Player;
    }
    public static bool CanPass(Unit unit, Unit other) {
        return AreAllies(unit.Player, other.Player);
    }
    public static bool CanJoin(Unit unit, Unit other) {
        return other != unit && unit.type == other.type && unit.Player == other.Player && other.Hp < MaxHp(other);
    }
    public static bool CanLaunchMissile(UnitType unitType) {
        return UnitStats.Loaded.TryGetValue(unitType, out var entry) && entry.specialCommands.HasFlag(UnitStats.SpecialCommand.CanLaunchMissile);
    }
    public static bool CanLaunchMissile(Unit unit, Building missileSilo) {
        Assert.AreEqual(TileType.MissileSilo, missileSilo.type);
        return CanLaunchMissile(unit.type) &&
               AreAllies(unit.Player, missileSilo.Player) &&
               CanLaunchMissile(missileSilo);
    }
    public static bool CanLaunchMissile(Building missileSilo) {
        Assert.AreEqual(TileType.MissileSilo, missileSilo.type);
        return missileSilo.level.turn >= missileSilo.missileSiloLastLaunchTurn + missileSilo.missileSiloLaunchCooldown * missileSilo.level.players.Count &&
               missileSilo.missileSiloAmmo > 0;
    }

    public static int VisionCapacity(UnitType unitType) {
        return unitType switch {
            UnitType.Infantry or UnitType.AntiTank => 3,
            UnitType.Artillery => 3,
            UnitType.Apc => 3,
            UnitType.Recon => 10,
            UnitType.LightTank => 5,
            UnitType.Rockets => 2,
            UnitType.MediumTank => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }

    public static int VisionCost(TileType tileType) {
        if ((TileType.Buildings & tileType) != 0)
            return 2;
        return tileType switch {
            TileType.Plain or TileType.Road or TileType.Sea or TileType.River => 1,
            TileType.Forest => 2,
            TileType.Mountain => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null)
        };
    }

    public static bool IsAirborne(UnitType unitType) {
        return unitType is UnitType.TransportHelicopter or UnitType.AttackHelicopter or UnitType.FighterJet or UnitType.Bomber;
    }
}