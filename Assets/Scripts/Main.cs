using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using static BattleConstants;

public class Main : MonoBehaviour {

    private static Main instance;
    public static bool TryFind(out Main main) {
        if (!instance)
            instance = FindObjectOfType<Main>();
        main = instance;
        return instance;
    }

    [Command] public bool autoplay = true;
    public AiPlayerCommander aiPlayerCommander;

    virtual  protected void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha8))
            autoplay = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    public void IssueAiCommandsForSelectionState() {
        aiPlayerCommander.IssueCommandsForSelectionState();
    }
    public void IssueAiCommandsForPathSelectionState() {
        aiPlayerCommander.IssueCommandsForPathSelectionState();
    }
    public void IssueAiCommandsForActionSelectionState() {
        aiPlayerCommander.IssueCommandsForActionSelectionState();
    }

    public Traverser traverser = new();

    [Command]
    public bool followLastUnit;
    public bool inLevelEditor = true;

    public List<Bridge> bridges = new();

    [Command] public MissionName missionName;
    public Dictionary<Vector2Int, TileType> tiles = new();
    public Dictionary<Vector2Int, Unit> units = new();
    public Dictionary<Vector2Int, Building> buildings = new();
    public Dictionary<TriggerName, HashSet<Vector2Int>> triggers = new() {
        [TriggerName.A] = new HashSet<Vector2Int>(),
        [TriggerName.B] = new HashSet<Vector2Int>(),
        [TriggerName.C] = new HashSet<Vector2Int>(),
        [TriggerName.D] = new HashSet<Vector2Int>(),
        [TriggerName.E] = new HashSet<Vector2Int>(),
        [TriggerName.F] = new HashSet<Vector2Int>(),
    };
    public List<Player> players = new();
    [Command] public int turn = 0;
    public LevelLogic levelLogic = new();
    public Player localPlayer;
    public PersistentData persistentData;

    public DebugStack stack = new();
    public Queue<string> commands = new();

    public MeshFilter tileAreaMeshFilter;

    public Camera mainCamera;
    public Camera[] battleCameras = { null, null };

    public virtual void Awake() {

        Player.undisposed.Clear();
        Building.undisposed.Clear();
        Unit.undisposed.Clear();
        UnitAction.undisposed.Clear();

        ReloadPersistentData();

        Assert.IsTrue(mainCamera);
        Assert.IsTrue(battleCameras.Length == right + 1);
        Assert.IsTrue(battleCameras[left]);
        Assert.IsTrue(battleCameras[right]);
    }

    [Command]
    public void ReloadPersistentData() {
        persistentData = PersistentData.Read();
        UpdatePostProcessing();
    }

    [Command]
    public bool ShowBattleAnimation {
        set {
            persistentData.gameSettings.showBattleAnimation = value;
            persistentData.Save();
        }
    }

    protected virtual void OnApplicationQuit() {
        persistentData.Save();
    }

    public void UpdatePostProcessing() {
        PostProcessing.Setup(
            persistentData.gameSettings.antiAliasing,
            persistentData.gameSettings.motionBlurShutterAngle,
            persistentData.gameSettings.enableBloom,
            persistentData.gameSettings.enableScreenSpaceReflections,
            persistentData.gameSettings.enableAmbientOcclusion);
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
    public bool TryGetBridge(Vector2Int position, out Bridge bridge) {
        bridge = null;
        foreach (var b in bridges)
            if (b.tiles.ContainsKey(position)) {
                bridge = b;
                return true;
            }
        return false;
    }

    public IEnumerable<Unit> FindUnitsOf(Player player) {
        return units.Values.Where(unit => unit.Player == player);
    }
    public IEnumerable<Building> FindBuildingsOf(Player player) {
        return buildings.Values.Where(building => building.Player == player);
    }

    public IEnumerable<Vector2Int> PositionsInRange(Vector2Int position, Vector2Int range) {
        return range.Offsets().Select(offset => offset + position).Where(p => tiles.ContainsKey(p));
    }

    [Command]
    public void EnqueueCommand(string command) {
        commands.Enqueue(command);
    }
}

public enum TriggerName { A, B, C, D, E, F }