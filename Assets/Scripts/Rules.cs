using System;
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
        var hasUnits = player.main.FindUnitsOf(player).Any();
        var buildings = player.main.FindBuildingsOf(player).ToList();
        var hasIncome = buildings.Any(building => Income(building) > 0);
        var canBuildUnits = buildings.Any(building => GetBuildableUnitTypes(building).Any());
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
    public static int ModifiedHp(Unit unit, int originalValue) {
        return originalValue;
    }

    public static int MaxFuel(UnitType unitType) {
        return UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry) ? entry.fuel : 99;
    }
    public static int MaxFuel(Unit unit) {
        return MaxFuel(unit.type);
    }
    public static int ModifiedFuel(Unit unit, int originalValue) {
        return originalValue;
    }

    public static int GetAmmo(Unit unit, WeaponName weaponName) {
        var found = unit.Ammo.TryGetValue(weaponName, out var amount);
        Assert.IsTrue(found, weaponName.ToString());
        return amount;
    }

    public static IEnumerable<WeaponName> GetWeapons(UnitType type) {
        return UnitTypeSettings.Loaded.TryGetValue(type, out var entry) ? entry.ammo.Keys : Enumerable.Empty<WeaponName>();
    }

    public static bool TryGetDamage(UnitType attackerType, UnitType targetType, WeaponName weaponName, out int damage) {
        return DamageTable.Loaded.TryGetValue((attackerType, targetType, weaponName), out damage);
    }
    public static bool TryGetDamage(Unit attacker, Unit target, WeaponName weaponName, out int damage) {
        damage = 0;
        if (!AreEnemies(attacker.Player, target.Player) || GetAmmo(attacker, weaponName) <= 0 || !TryGetDamage(attacker.type, target.type, weaponName, out var baseDamage))
            return false;
        damage = CeilToInt((float)(attacker.Hp) / MaxHp(attacker) * baseDamage);
        return true;
    }

    public static IEnumerable<(WeaponName weaponName, int damage)> GetDamageValues(Unit attacker, Unit target) {
        foreach (var weaponName in attacker.Ammo.Keys)
            if (TryGetDamage(attacker, target, weaponName, out var damage))
                yield return (weaponName, damage);
    }

    public static int Cost(UnitType unitType, Player player) {
        var found = UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry);
        Assert.IsTrue(found, unitType.ToString());
        return entry.cost;
    }
    public static bool CanAfford(this Player player, UnitType unitType) {
        return player.Credits >= Cost(unitType, player);
    }

    public static bool CanAttack(UnitType attackerType, UnitType targetType, WeaponName weaponName) {
        return TryGetDamage(attackerType, targetType, weaponName, out _);
    }
    public static bool CanAttack(Unit attacker, Unit target, WeaponName weaponName) {
        return AreEnemies(attacker.Player, target.Player) &&
               TryGetDamage(attacker, target, weaponName, out _);
    }
    public static bool CanAttack(Unit attacker, Unit target, IReadOnlyList<Vector2Int> path, WeaponName weaponName) {

        Assert.IsTrue(path.Count >= 1);
        if (target.Position is not { } targetPosition)
            throw new AssertionException("target.Position == null", "");
        if (TryGetAttackRange(attacker, out var attackRange))
            Assert.IsTrue(MathUtils.ManhattanDistance(path.Last(), targetPosition).IsIn(attackRange));

        return CanAttack(attacker, target, weaponName) &&
               (!IsArtillery(attacker) || path.Count == 1);
    }
    public static bool IsArtillery(UnitType unitType) {
        return unitType is UnitType.Artillery or UnitType.Rockets;
    }

    public static bool CanAttackInResponse(UnitType unitType) {
        return !IsArtillery(unitType);
    }

    /*public static bool CanAttackInResponse(Unit attacker, Unit target, out int weaponIndex) {

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
    }*/

    public static bool TryGetAttackRange(UnitType unitType, Player player, out Vector2Int attackRange) {
        attackRange = UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry) ? entry.attackRange : Vector2Int.zero;
        return attackRange != Vector2Int.zero;
    }
    public static bool TryGetAttackRange(Unit unit, out Vector2Int attackRange) {
        return TryGetAttackRange(unit.type, unit.Player, out attackRange);
    }

    public static int MaxAmmo(Unit unit, WeaponName weaponName) {
        return MaxAmmo(unit.type, weaponName);
    }
    public static int MaxAmmo(UnitType type, WeaponName weaponName) {
        return UnitTypeSettings.Loaded.TryGetValue(type, out var entry) && entry.ammo.TryGetValue(weaponName, out var amount)
            ? amount
            : 99;
    }
    public static bool CanLoadAsCargo(UnitType receiverType, UnitType targetType) {
        return UnitTypeSettings.Loaded.TryGetValue(receiverType, out var entry) && entry.canCarry.Contains(targetType);
    }
    public static int Weight(UnitType unitType) {
        return UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry) ? entry.weight : 1;
    }
    public static int CarryCapacity(UnitType unitType) {
        return UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry) ? entry.carryCapacity : 0;
    }
    public static bool CanLoadAsCargo(Unit receiver, Unit target) {
        var cargoSize = receiver.Cargo.Sum(u => Weight(u));
        return CanLoadAsCargo(receiver.type, target.type) &&
               (target.Carrier == null || target.Carrier == receiver) &&
               AreAllies(receiver.Player, target.Player) &&
               cargoSize + Weight(target) <= CarryCapacity(receiver);
    }
    public static bool CanSupply(UnitType unitType) {
        return UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry) && entry.specialCommands.HasFlag(UnitTypeSettings.SpecialCommand.CanSupply);
    }
    public static bool CanSupply(Unit unit, Unit target) {
        return CanSupply(unit.type) && AreAllies(unit.Player, target.Player) && (unit != target);
    }

    public static int MoveDistance(Unit unit) {
        var moveDistance = MoveDistance(unit.type, unit.Player);
        if (unit.Player.co == Co.Natalie && unit.Player.abilityActivationTurn != null)
            moveDistance += 5;
        return Min(unit.Fuel, moveDistance);
    }

    public static int MoveDistance(UnitType unitType, Player player) {
        return UnitTypeSettings.Loaded.TryGetValue(unitType, out var settings) ? settings.moveDistance : 0;
    }

    public static bool TryGetMoveCost(UnitType unitType, TileType tileType, out int cost) {

        const int unreachable = -1;

        if (UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry))
            cost = entry.moveCostType switch {
                MoveCostType.Foot => tileType switch {
                    TileType.Sea => unreachable,
                    TileType.Forest or TileType.Mountain or TileType.River => 2,
                    _ => 1
                },
                MoveCostType.Tires => tileType switch {
                    TileType.Sea or TileType.Mountain or TileType.River => unreachable,
                    TileType.Forest => 3,
                    TileType.Plain => 2,
                    _ => 1
                },
                MoveCostType.Tracks => tileType switch {
                    TileType.Sea or TileType.Mountain or TileType.River => unreachable,
                    TileType.Forest => 2,
                    _ => 1
                },
                _ => unreachable
            };
        else
            cost = unreachable;

        return cost != unreachable;
    }

    public static Traverser.TryGetCostDelegate GetMoveCostFunction(Unit unit) {

        bool TryGetCost(Vector2Int position, int distance, out int cost) {
            cost = 0;

            if (distance >= MoveDistance(unit) ||
                !unit.Player.main.TryGetTile(position, out var tile) ||
                unit.Player.main.TryGetUnit(position, out var other) && !CanPass(unit, other))
                return false;

            return TryGetMoveCost(unit, tile, out cost);
        }

        return TryGetCost;
    }

    public static bool CanStay(UnitType unitType, TileType tileType) {
        return TryGetMoveCost(unitType, tileType, out _);
    }
    public static bool CanStay(Unit unit, Vector2Int position) {
        return unit.Player.main.TryGetTile(position, out var tile) &&
               CanStay(unit, tile) &&
               (!unit.Player.main.TryGetUnit(position, out var other) || other == unit);
    }
    public static bool CanCapture(UnitType unitType, TileType buildingType) {
        return UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry) && entry.specialCommands.HasFlag(UnitTypeSettings.SpecialCommand.CanCapture);
    }
    public static bool CanCapture(Unit unit, Building building) {
        return CanCapture(unit.type, building.type) &&
               (building.Player == null || AreEnemies(unit.Player, building.Player));
    }
    public static bool CanPass(Unit unit, Unit other) {
        return AreAllies(unit.Player, other.Player);
    }
    public static bool CanJoin(Unit unit, Unit other) {
        return other != unit && unit.Player == other.Player && other.Hp < MaxHp(other);
    }
    public static bool CanLaunchMissile(UnitType unitType) {
        return UnitTypeSettings.Loaded.TryGetValue(unitType, out var entry) && entry.specialCommands.HasFlag(UnitTypeSettings.SpecialCommand.CanLaunchMissile);
    }
    public static bool CanLaunchMissile(Unit unit, Building missileSilo) {
        Assert.AreEqual(TileType.MissileSilo, missileSilo.type);
        return CanLaunchMissile(unit.type) &&
               AreAllies(unit.Player, missileSilo.Player) &&
               CanLaunchMissile(missileSilo);
    }
    public static bool CanLaunchMissile(Building missileSilo) {
        Assert.AreEqual(TileType.MissileSilo, missileSilo.type);
        return missileSilo.main.turn >= missileSilo.missileSiloLastLaunchTurn + missileSilo.missileSiloLaunchCooldown * missileSilo.main.players.Count &&
               missileSilo.missileSiloAmmo > 0;
    }
}