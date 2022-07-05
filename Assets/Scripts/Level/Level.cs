using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Level : SubstateMachine {

	public Dictionary<Vector2Int, Unit> units = new();
	public Dictionary<Vector2Int, TileType> tiles = new();
	public List<Player> players = new();
	public int? turn;
	public Dictionary<Vector2Int, Building> buildings = new();
	public LevelScript script;

	public Level() : base(typeof(LevelRunner), nameof(Level)) {
		CameraRig.Instance.enabled = true;
	}

	public override void Dispose() {
		base.Dispose();
		CameraRig.Instance.enabled = false;
	}

	public override void OnAfterPop() {
		CameraRig.Instance.enabled = true;
	}
	public override void OnBeforePush() {
		CameraRig.Instance.enabled = false;
	}

	public bool TryGetTile(Vector2Int position, out TileType tile) {
		return tiles.TryGetValue(position, out tile);
	}
	public bool TryGetUnit(Vector2Int position, out Unit unit) {
		return units.TryGetValue(position, out unit);
	}
	public bool TryGetBuilding(Vector2Int position, out Building building) {
		return buildings.TryGetValue(position, out building);
	}

	public IEnumerable<Vector2Int> AttackRange(Vector2Int position, Vector2Int range) {
		return range.Offsets().Select(offset => offset + position).Where(p => tiles.ContainsKey(p));
	}
}