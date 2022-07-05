using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class Level : IDisposable {

	public Dictionary<Vector2Int, Unit> units = new();
	public Dictionary<Vector2Int, TileType> tiles = new();
	public List<Player> players = new();
	public int? turn;
	public LevelRunner runner;
	public Dictionary<Vector2Int, Building> buildings = new();

	public ChangeTracker<LevelState> state = new(old => old?.Dispose());

	public Level(string name = null) {
		var go = new GameObject(name ?? nameof(Level));
		Object.DontDestroyOnLoad(go);
		runner = go.AddComponent<LevelRunner>();
		runner.level = this;
	}
	public void Dispose() {
		state.v?.Dispose();
		if (runner && runner.gameObject)
			Object.Destroy(runner.gameObject);
	}

	public bool TryGetTile(Vector2Int position, out TileType tile) {
		return tiles.TryGetValue(position, out tile);
	}
	public bool TryGetUnit(Vector2Int position, out Unit unit) {
		return units.TryGetValue(position, out unit);
	}
	public bool TryGetBuilding(Vector2Int position, out Building building)  {
		return buildings.TryGetValue(position, out building);
	}

	public IEnumerable<Vector2Int> AttackRange(Vector2Int position, Vector2Int range) {
		return range.Offsets().Select(offset => offset + position).Where(p => tiles.ContainsKey(p));
	}
}

public abstract class LevelState : IDisposable {

	public Level level;
	protected LevelState(Level level) {
		Assert.IsNotNull(level);
		this.level = level;
	}

	public Player CurrentPlayer {
		get {
			Assert.AreNotEqual(0, Players.Count);
			return level.players[Turn % Players.Count];
		}
	}
	public Dictionary<Vector2Int, TileType> Tiles => level.tiles;
	public Dictionary<Vector2Int, Unit> Units => level.units;
	public List<Player> Players => level.players;
	public int Turn {
		get {
			Assert.AreNotEqual(null, level.turn);
			return (int)level.turn;
		}
	}

	public virtual void Update() { }
	public virtual void DrawGUI() { }
	public virtual void Dispose() { }
	public virtual void DrawGizmos() { }
	public virtual void DrawGizmosSelected() { }
}