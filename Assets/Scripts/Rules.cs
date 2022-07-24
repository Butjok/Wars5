using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Mathf;

[Flags]
public enum TileType {
	Plain = 1 << 0,
	Road = 1 << 1,
	City = 1 << 2,
	Hq = 1 << 3,
	Factory = 1 << 4,
	Airport = 1 << 5,
	Sea = 1 << 6,
	Mountain = 1 << 7
}

[Flags]
public enum UnitType {
	Infantry = 1 << 0,
	AntiTank = 1 << 1,
	Artillery = 1 << 2,
	Apc = 1 << 3
}

[Flags]
public enum BuildingType {
	City = 1 << 0,
	Hq = 1 << 1,
	Factory = 1 << 2,
	Airport = 1 << 3
}

public static class Rules {

	public static Vector2Int[] offsets = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };


	public static bool Lost(Player player) {
		return player.level.units.Values.All(unit => unit.player != player);
	}
	public static bool AreEnemies(Player p1, Player pl2) {
		return (p1.team & pl2.team) == 0;
	}
	public static bool AreAllies(Player p1, Player pl2) {
		return !AreEnemies(p1, pl2);
	}

	public static int MaxHp(UnitType type) {
		return 10;
	}
	public static int MaxFuel(UnitType type) {
		return 99;
	}
	public static bool CanCapture(UnitType unitType, BuildingType buildingType) {
		return ((UnitType.Infantry | UnitType.AntiTank) & unitType) != 0;
	}
	public static bool CanCapture(Unit unit, Building building) {
		return CanCapture(unit.type, building.type) &&
		       (building.player == null || AreEnemies(unit.player, building.player));
	}
	public static int Cp(Building building) {
		return building.cp;
	}
	public static int? Damage(UnitType attacker, UnitType target, int weapon) {
		Assert.IsTrue(weapon < WeaponsCount(attacker));
		switch (attacker, target) {
			case (UnitType.Infantry, UnitType.Infantry): return 5;
			case (UnitType.Infantry, UnitType.AntiTank): return 5;
			case (UnitType.Infantry, UnitType.Artillery): return 3;
			case (UnitType.Infantry, UnitType.Apc): return 3;
		}
		return null;
	}
	public static bool CanAttack(UnitType attacker, UnitType target, int weapon) {
		Assert.IsTrue(weapon < WeaponsCount(attacker));
		return Damage(attacker, target, weapon) != null;
	}
	public static bool CanAttack(Unit attacker, Unit target, int weapon) {
		Assert.IsTrue(weapon < WeaponsCount(attacker));
		return CanAttack(attacker.type, target.type, weapon) &&
		       AreEnemies(attacker.player, target.player) &&
		       Ammo(attacker, weapon) > 0;
	}
	public static bool IsArtillery(UnitType type) {
		return (UnitType.Artillery & type) != 0;
	}
	public static Vector2Int AttackDistance(UnitType type) {
		if (((UnitType.Infantry | UnitType.AntiTank) & type) != 0)
			return new Vector2Int(1, 1);
		if (((UnitType.Artillery) & type) != 0)
			return new Vector2Int(2, 3);
		return Vector2Int.zero;
	}

	public static int WeaponsCount(UnitType type) {
		if (((UnitType.Infantry | UnitType.Artillery) & type) != 0)
			return 1;
		if (((UnitType.AntiTank) & type) != 0)
			return 2;
		return 0;
	}
	public static int MaxAmmo(UnitType type, int weapon) {
		return 99;
	}
	public static int Ammo(Unit unit, int weapon) {
		Assert.IsTrue(weapon < WeaponsCount(unit));
		Assert.IsTrue(weapon < unit.ammo.Count);
		return unit.ammo[weapon];
	}
	public static bool CanTake(UnitType receiver, UnitType target) {
		if (receiver == UnitType.Apc && ((UnitType.Infantry | UnitType.AntiTank) & target) != 0)
			return true;
		return false;
	}
	public static int Size(UnitType type) {
		return 1;
	}
	public static int CargoCapacity(UnitType type) {
		if (type == UnitType.Apc)
			return 1;
		return 0;
	}
	public static bool CanTake(Unit receiver, Unit target) {
		var cargoSize = receiver.cargo.Sum(u => Size(u));
		return CanTake(receiver.type, target.type) &&
		       AreAllies(receiver.player, target.player) &&
		       cargoSize + Size(target) <= CargoCapacity(receiver);
	}
	public static bool CanSupply(UnitType type) {
		return type == UnitType.Apc;
	}
	public static bool CanSupply(Unit unit, Unit target) {
		return CanSupply(unit.type) && AreAllies(unit.player, target.player);
	}

	public static int MoveDistance(UnitType type) {
		if (((UnitType.Infantry) & type) != 0)
			return 7;
		if (((UnitType.AntiTank) & type) != 0)
			return 5;
		if (((UnitType.Artillery | UnitType.Apc) & type) != 0)
			return 5;
		return 0;
	}
	public static int MoveDistance(Unit unit) {
		return Min(unit.fuel.v, MoveDistance(unit.type));
	}
	public static int? MoveCost(UnitType unit, TileType tile) {

		int? foot = (TileType.Sea & tile) != 0 ? null : 1;
		int? tires = null;
		int? tracks = ((TileType.Sea | TileType.Mountain) & tile) != 0 ? null : 1;
		int? air = null;
		int? sea = null;

		if (((UnitType.Infantry | UnitType.AntiTank) & unit) != 0)
			return foot;
		if (((UnitType.Artillery | UnitType.Apc) & unit) != 0)
			return tracks;
		
		return null;
	}
	public static bool CanPass(Unit unit,Unit other) {
		return AreAllies(unit.player,other.player);
	}
}