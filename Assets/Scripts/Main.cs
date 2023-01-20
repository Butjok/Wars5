using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Butjok.CommandLine;
using DG.Tweening;
using Shapes;
using UnityEngine;
using UnityEngine.Assertions;

public class Main : ImmediateModeShapeDrawer {

	
	
	public List<Bridge> bridges = new();

	[Command] public MissionName missionName;
	public Dictionary<Vector2Int, TileType> tiles = new();
	public Dictionary<Vector2Int, Unit> units = new();
	public Dictionary<Vector2Int, Building> buildings = new();
	public Dictionary<Vector2Int, Trigger> triggers = new();
	public List<Player> players = new();
	[Command] public int turn = 0;
	public LevelLogic levelLogic = new();
	public Player localPlayer;
	public GameSettings settings = new();

	public DebugStack stack = new();
	public Queue<string> commands = new();
	public GUISkin guiSkin;

	public MeshFilter tileAreaMeshFilter;

	public void Awake() {

		Player.undisposed.Clear();
		Building.undisposed.Clear();
		Unit.undisposed.Clear();
		UnitAction.undisposed.Clear();

		UpdatePostProcessing();

		settings = PersistentData.Read().gameSettings;

		guiSkin = Resources.Load<GUISkin>("CommandLine");
	}

	public void UpdatePostProcessing() {
		PostProcessing.Setup(
			settings.antiAliasing,
			settings.motionBlurShutterAngle,
			settings.enableBloom,
			settings.enableScreenSpaceReflections,
			settings.enableAmbientOcclusion);
	}

	public Player CurrentPlayer {
		get {
			Assert.AreNotEqual(0, players.Count);
			Assert.IsTrue(turn != null);
			Assert.IsTrue(turn >= 0);
			return players[(int)turn % players.Count];
		}
	}

	public bool TryGetTile(Vector2Int position, out TileType tile) {
		return tiles.TryGetValue(position, out tile) && tile != 0;
	}
	public bool TryGetUnit(Vector2Int position, out Unit unit) {
		return units.TryGetValue(position, out unit) && unit != null;
	}
	public bool TryGetBuilding(Vector2Int position, out Building building) {
		return buildings.TryGetValue(position, out building) && building != null;
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

	protected virtual void OnApplicationQuit() {
		//Clear();
		//Debug.Log(@$"UNDISPOSED: players: {Player.undisposed.Count} buildings: {Building.undisposed.Count} units: {Unit.undisposed.Count} unitActions: {UnitAction.undisposed.Count}");
	}

	protected virtual void OnGUI() {
		if (guiSkin)
			GUI.skin = guiSkin;
		// GUILayout.Label("");
		GUILayout.Label($"stack: {stack.Count}");
	}

	public float fadeDuration = 2;
	public Ease fadeEase = Ease.Unset;
	public void RestartGame() {
		PostProcessing.ColorFilter = Color.black;
		PostProcessing.Fade(Color.white, fadeDuration, fadeEase);
		StopAllCoroutines();
		StartCoroutine(SelectionState.Run(this, true));
	}

	[Command]
	public void EnqueueCommand(string command) {
		commands.Enqueue(command);
	}
}

[Flags]
public enum Trigger {
	A = 1 << 0,
	B = 1 << 1,
	C = 1 << 2,
	D = 1 << 3,
	E = 1 << 4,
	F = 1 << 5
}