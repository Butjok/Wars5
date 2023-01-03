using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class Level : MonoBehaviour {

    private static Level instance;
    public static Level Instance {
        get {
            if (!instance)
                instance = FindObjectOfType<Level>();
            return instance;
        }
    }

    public Map2D<Unit> units;
    public Map2D<TileType> tiles;
    public Map2D<Building> buildings;
    public PlayerCollection players = new();
    public int? turn = 0;
    public LevelLogic levelLogic = new();
    public Player localPlayer;
    public GameSettings settings = new();

    public InputCommandsContext input = new();

    public void Awake() {
        UpdatePostProcessing();
        settings = GameSettings.Load();
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

    private void OnApplicationQuit() {
        if (Player.undisposed.Count != 0) { }
        if (Unit.undisposed.Count != 0) { }
        if (UnitAction.undisposed.Count != 0) { }
    }

    public float fadeDuration = 2;
    public Ease fadeEase = Ease.Unset;
    public void StartGame() {
        PostProcessing.ColorFilter = Color.black;
        PostProcessing.Fade(Color.white, fadeDuration, fadeEase);
        StartCoroutine(SelectionState.New(this, true));
    }
}