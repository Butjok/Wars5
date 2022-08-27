using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level : StateMachineState {

	public Dictionary<Vector2Int, Unit> units = new();
	public Dictionary<Vector2Int, TileType> tiles = new();
	public List<Player> players = new();
	public int? turn;
	public Dictionary<Vector2Int, Building> buildings = new();
	public LevelScript script;

	public Level(StateMachine game) : base(game,nameof(Level)) {
		CameraRig.Instance.enabled = true;
	}

	public override void Update() {
		base.Update();
		if (Input.GetKeyDown(KeyCode.Escape))
			Sm.Push(new InGameOverlayMenu(Sm));
	}

	public override void DrawGUI() {
		base.DrawGUI();
		GUILayout.Label(State?.ToString());
	}

	public override void Dispose() {
		base.Dispose();
		CameraRig.Instance.enabled = false;
	}

	public override void OnAfterPop() {
		base.OnAfterPop();
		CameraRig.Instance.enabled = true;
	}
	public override void OnBeforePush() {
		base.OnBeforePush();
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

	public IEnumerable<Unit> FindUnitsOf(Player player) {
		return units.Values.Where(unit => unit.player == player);
	}
	public IEnumerable<Building> FindBuildingsOf(Player player) {
		return buildings.Values.Where(building => building.player.v == player);
	}

	public IEnumerable<Vector2Int> AttackPositions(Vector2Int position, Vector2Int range) {
		return range.Offsets().Select(offset => offset + position).Where(p => tiles.ContainsKey(p));
	}
}