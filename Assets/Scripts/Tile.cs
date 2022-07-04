using UnityEngine;
using UnityEngine.Assertions;

public enum TileType { Plain }

public class Tile {
	public TileType type;
	public Vector2Int position;

	public Tile(Level level, TileType type, Vector2Int position) {
		Assert.IsFalse(level.tileAt.ContainsKey(position));
		this.type = type;
		level.tileAt.Add(position, this);
		this.position = position;
	}
}