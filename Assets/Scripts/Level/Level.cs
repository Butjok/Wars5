using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level : SubStateMachine {

	public readonly Map2D<Unit> units;
	public readonly Map2D<TileType> tiles;
	public readonly Map2D<Building> buildings;
	public readonly List<Player> players = new();
	private int? turn;
	public LevelScript script;

	public List<Unit> unitsBuffer=new();
	public List<Building> buildingsBuffer=new();

	public int? Turn {
		get => turn;
		set {
			turn = value;
			if (turn is { } integer && players.Count > 0) {
				var currentPlayer = players[integer.PositiveModulo(players.Count)];
				foreach (var player in players)
					player.view.Visible = player == currentPlayer;
			}
		}
	}

	public Level(Vector2Int min, Vector2Int max, StateMachine game) : base(game, nameof(Level)) {
		
		units = new Map2D<Unit>(min, max);
		tiles = new Map2D<TileType>(min, max);
		buildings = new Map2D<Building>(min, max);

		if (CameraRig.Instance)
			CameraRig.Instance.enabled = true;
	}

	public override void Update() {
		base.Update();
		//if (Input.GetKeyDown(KeyCode.Escape))
		//	Sm.Push(new InGameOverlayMenu(Sm));
	}

	public override void DrawGUI() {
		base.DrawGUI();
		//var text = State?.ToString();
		//var size = GUI.skin.label.CalcSize(new GUIContent(text));
		//GUI.Label(new Rect(new Vector2(Screen.width-size.x,0), size), text);
		//GUILayout.Label(State?.ToString());
	}

	public override void Dispose() {
		base.Dispose();
		if (CameraRig.Instance)
			CameraRig.Instance.enabled = false;
	}

	public override void OnAfterPop() {
		base.OnAfterPop();
		if (CameraRig.Instance)
			CameraRig.Instance.enabled = true;
	}
	public override void OnBeforePush() {
		base.OnBeforePush();
		if (CameraRig.Instance)
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