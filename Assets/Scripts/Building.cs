using UnityEngine;
using UnityEngine.Assertions;

public class Building {

	public BuildingType type;
	public Level level;
	public Vector2Int position;
	public Player player;
	public int cp = 20;

	protected Building(Level level, Vector2Int position, BuildingType type = BuildingType.City, Player player = null) {
		this.type = type;
		this.level = level;
		this.position = position;
		this.player = player;
		Assert.IsFalse(level.buildings.ContainsKey(position));
		level.buildings.Add(position, this);
	}

	public override string ToString() {
		return $"{type}{position} {player}";
	}
}