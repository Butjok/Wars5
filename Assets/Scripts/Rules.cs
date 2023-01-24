using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
        return 99;
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
        switch (type) {

            case UnitType.Infantry:
                yield return WeaponName.Rifle;
                break;

            case UnitType.AntiTank:
                yield return WeaponName.RocketLauncher;
                yield return WeaponName.Rifle;
                break;

            case UnitType.Artillery:
                yield return WeaponName.Cannon;
                break;

            case UnitType.Recon:
                yield return WeaponName.MachineGun;
                break;

            case UnitType.LightTank or UnitType.MediumTank:
                yield return WeaponName.Cannon;
                yield return WeaponName.MachineGun;
                break;

            case UnitType.Rockets:
                yield return WeaponName.RocketLauncher;
                break;
        }
    }

    public static bool TryGetDamage(UnitType attackerType, UnitType targetType, WeaponName weaponName, out int damage) {
        int? result = (attackerType, targetType, weaponName) switch {

            (UnitType.Infantry or UnitType.AntiTank, UnitType.Infantry or UnitType.AntiTank, WeaponName.Rifle) => 5,
            (UnitType.Infantry or UnitType.AntiTank, UnitType.Artillery or UnitType.Apc, WeaponName.Rifle) => 3,

            (UnitType.AntiTank, UnitType.Artillery or UnitType.Apc, WeaponName.RocketLauncher) => 5,

            _ => null
        };
        damage = result is { } value ? value : -1;
        return result != null;
    }
    public static bool TryGetDamage(Unit attacker, Unit target, WeaponName weaponName, out int damage) {
        damage = 0;
        if (!AreEnemies(attacker.Player,target.Player) || GetAmmo(attacker, weaponName) <= 0 || !TryGetDamage(attacker.type, target.type, weaponName, out var baseDamage))
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
        return unitType switch {
            UnitType.Infantry => 1000,
            UnitType.AntiTank => 2000,
            UnitType.Artillery => 5000,
            UnitType.Apc => 5000,
            UnitType.Recon => 3000,
            UnitType.LightTank => 5000,
            UnitType.MediumTank => 8000,
            UnitType.Rockets => 12000,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
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
        attackRange = unitType switch {
            UnitType.Infantry or UnitType.AntiTank or UnitType.Recon or UnitType.LightTank or UnitType.MediumTank => new Vector2Int(1, 1),
            UnitType.Artillery => new Vector2Int(2, 3),
            UnitType.Rockets => new Vector2Int(3, 5),
            _ => Vector2Int.zero
        };
        return attackRange != Vector2Int.zero;
    }
    public static bool TryGetAttackRange(Unit unit, out Vector2Int attackRange) {
        return TryGetAttackRange(unit.type, unit.Player, out attackRange);
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
    public static int MaxAmmo(Unit unit, WeaponName weaponName) {
        return MaxAmmo(unit.type, weaponName);
    }
    public static int MaxAmmo(UnitType type, WeaponName weaponName) {
        return 99;
    }
    public static int MaxAmmo(UnitType type, int weaponIndex) {
        return 99;
    }
    public static bool CanLoadAsCargo(UnitType receiverType, UnitType targetType) {
        switch (receiverType, targetType) {
            case (UnitType.Apc, UnitType.Infantry or UnitType.AntiTank):
                return true;
        }
        return false;
    }
    public static int CargoSize(UnitType unitType) {
        return 1;
    }
    public static int CargoCapacity(UnitType unitType) {
        return unitType switch {
            UnitType.Apc => 1,
            _ => 0
        };
    }
    public static bool CanLoadAsCargo(Unit receiver, Unit target) {
        var cargoSize = receiver.Cargo.Sum(u => CargoSize(u));
        return CanLoadAsCargo(receiver.type, target.type) &&
               (target.Carrier == null || target.Carrier == receiver) &&
               AreAllies(receiver.Player, target.Player) &&
               cargoSize + CargoSize(target) <= CargoCapacity(receiver);
    }
    public static bool CanSupply(UnitType unitType) {
        return unitType == UnitType.Apc;
    }
    public static bool CanSupply(Unit unit, Unit target) {
        return CanSupply(unit.type) && AreAllies(unit.Player, target.Player) && (unit != target);
    }

    public static int MoveDistance(UnitType unitType, Player player) {
        return unitType switch {
            UnitType.Infantry => 3,
            UnitType.AntiTank => 2,
            UnitType.LightTank => 5,
            UnitType.MediumTank => 4,
            UnitType.Artillery or UnitType.Apc or UnitType.Recon or UnitType.Rockets => 5,
            _ => 0
        };
    }
    public static int MoveDistance(Unit unit) {
        var moveDistance = MoveDistance(unit.type, unit.Player);
        if (unit.Player.co == Co.Natalie && unit.Player.abilityActivationTurn != null)
            moveDistance += 5;
        return Min(unit.Fuel, moveDistance);
    }
    public static bool TryGetMoveCost(UnitType unitType, TileType tileType, out int cost) {

        const int unreachable = -1;
        
        int foot = tileType switch {
            TileType.Sea => unreachable,
            TileType.Mountain => 2,
            _ => 1
        };
        int tracks = tileType switch {
            TileType.Sea or TileType.Mountain or TileType.River => unreachable,
            TileType.Forest => 2,
            _ => 1
        };
        int tires = tileType switch {
            TileType.Sea or TileType.Mountain or TileType.River => unreachable,
            TileType.Forest => 3,
            TileType.Plain => 2,
            _ => 1
        };
        int air = unreachable;
        int sea = unreachable;

        cost = unitType switch {
            UnitType.Infantry or UnitType.AntiTank => foot,
            UnitType.Artillery or UnitType.LightTank or UnitType.Apc or UnitType.MediumTank => tracks,
            UnitType.Recon or UnitType.Rockets => tires,
            _ => unreachable
        };
        return cost != unreachable;
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
        return unitType switch {
            UnitType.Infantry or UnitType.AntiTank => true,
            _ => false
        };
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
}