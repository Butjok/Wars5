using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Int2 {
	public int x, y;
	public override string ToString() {
		return $"({x}, {y})";
	}
	public static implicit operator Vector2Int(Int2 int2) {
		return new Vector2Int(int2.x, int2.y);
	}
	public static implicit operator Int2(Vector2Int vector2Int) {
		return new Int2 { x=vector2Int.x,y=vector2Int.y};
	}
}

public class IdCollection {
	public Dictionary<object, int> ids = new();
	public int this[object reference] {
		get {
			if (reference == null)
				return -1;
			if (!ids.TryGetValue(reference, out var id))
				id = ids[reference] = ids.Count;
			return id;
		}
	}
}

public class SerializedPlayer {

	public int id;
	public Team team;
	public Color32 color;
	public string coName;
	public PlayerType type;
	public AiDifficulty difficulty;

	public SerializedPlayer() { }
	public SerializedPlayer(Player player,IdCollection id) {
		this.id = id[player];
		team = player.team;
		color = player.color;
		if (player.co)
			coName = player.co.name;
		type = player.type;
		difficulty = player.difficulty;
	}

	public override string ToString() {
		return color.Name();
	}
}

public class SerializedUnit {

	public int id;
	public UnitType type;
	public int playerId;
	public string viewPrefabName;
	public Int2? position;
	public bool moved;
	public int hp;
	public int fuel;
	public int[] ammo;
	public int[] cargo;

	public SerializedUnit() { }
	public SerializedUnit(Unit unit, IdCollection id) {
		this.id = id[unit];
		type = unit.type;
		playerId = id[unit.player];
		if (unit.view && unit.view.prefab)
			viewPrefabName = unit.view.prefab.name;
		position = unit.position.v;
		moved = unit.moved.v;
		hp = unit.hp.v;
		fuel = unit.fuel.v;
		ammo = unit.ammo?.ToArray();
		cargo = unit.cargo?.Select(u => id[u]).ToArray();
	}

	public override string ToString() {
		return $"{type}{position} {playerId}";
	}
}

public class SerializedBuilding {

	public int id;
	public TileType type;
	public Int2 position;
	public int playerId;
	public int cp;

	public SerializedBuilding() { }
	public SerializedBuilding(Building building, IdCollection id) {
		this.id = id[building];
		type = building.type;
		position = building.position;
		playerId = id[building.player];
		cp = building.cp.v;
	}

	public override string ToString() {
		return $"{type}{position} {playerId}";
	}
}

public struct SerializedTile {

	public Int2 position;
	public TileType type;

	public SerializedTile(Vector2Int position, TileType type) {
		this.position = position;
		this.type = type;
	}

	public override string ToString() {
		return $"{type}{position}";
	}
}

public class SerializedLevel {

	public SerializedUnit[] units;
	public SerializedTile[] tiles;
	public SerializedPlayer[] players;
	public int? turn;
	public SerializedBuilding[] buildings;
	public string scriptTypeName;

	public SerializedLevel() { }
	public SerializedLevel(Level level) {

		var units = level.units.Values.ToList();
		void addCargo(Unit unit) {
			if (unit.cargo == null)
				return;
			foreach (var cargo in unit.cargo.Where(cargo => !units.Contains(cargo))) {
				units.Add(cargo);
				addCargo(cargo);
			}
		}
		foreach (var unit in level.units.Values)
			addCargo(unit);

		var id = new IdCollection();
		this.units = units.Select(unit => new SerializedUnit(unit, id)).ToArray();
		tiles = level.tiles.Select(kv => new SerializedTile(kv.Key, kv.Value)).ToArray();
		players = level.players.Select(player => new SerializedPlayer(player,id)).ToArray();
		turn = level.Turn;
		buildings = level.buildings.Values.Select(building => new SerializedBuilding(building, id)).ToArray();
		scriptTypeName = level.script?.GetType().Name;
	}
}