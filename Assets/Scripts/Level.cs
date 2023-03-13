using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using static BattleConstants;

public class Level : MonoBehaviour {

    [Command] public bool autoplay = true;
    public AiPlayerCommander aiPlayerCommander;

    protected virtual void Update() {
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

    [Command]
    public bool followLastUnit;

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
    public int Day(int turn) {
        Assert.IsTrue(turn >= 0);
        Assert.AreNotEqual(0, players.Count);
        return turn / players.Count;
    }
    public int Day() {
        return Day(turn);
    }
    public LevelLogic levelLogic = new();
    public Player localPlayer;

    public WarsStack stack = new();
    public Queue<string> commands = new();

    public MeshFilter tileAreaMeshFilter;
    public MeshFilter pathMeshFilter;

    public Camera mainCamera;
    public Camera[] battleCameras = { null, null };

    public virtual void Awake() {

        Player.undisposed.Clear();
        Building.undisposed.Clear();
        Unit.undisposed.Clear();
        UnitAction.undisposed.Clear();

        // Assert.IsTrue(mainCamera);
        // Assert.IsTrue(battleCameras.Length == right + 1);
        // Assert.IsTrue(battleCameras[left]);
        // Assert.IsTrue(battleCameras[right]);
    }

    public void ShouldEndTurn() {
        var peeked = StateRunner.Instance.states.TryPeek(out var state);
        Assert.IsTrue(peeked);
        Assert.IsTrue(StateRunner.Instance.Is<SelectionState2>(state, out var selectionState));
        selectionState.shouldEndTurn = true;
    }

    [Command]
    public bool ShowBattleAnimation {
        set {
            PersistentData.Get.gameSettings.showBattleAnimation = value;
            PersistentData.Save();
        }
    }

    protected virtual void OnApplicationQuit() {
        PersistentData.Save();
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