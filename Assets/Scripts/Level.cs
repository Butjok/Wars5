using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class Level : IDisposable {

    public static readonly Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };
    
    public LevelView view;

    public LevelName name;
    public Mission mission;
    public List<Player> players = new();
    public Player localPlayer;
    public int turn = 0;
    public Dictionary<Vector2Int, TileType> tiles = new();
    public Dictionary<Vector2Int, Unit> units = new();
    public Dictionary<Vector2Int, Building> buildings = new();
    public List<Bridge> bridges = new();
    public Dictionary<TriggerName, HashSet<Vector2Int>> triggers = new() {
        [TriggerName.A] = new HashSet<Vector2Int>(),
        [TriggerName.B] = new HashSet<Vector2Int>(),
        [TriggerName.C] = new HashSet<Vector2Int>(),
        [TriggerName.D] = new HashSet<Vector2Int>(),
        [TriggerName.E] = new HashSet<Vector2Int>(),
        [TriggerName.F] = new HashSet<Vector2Int>(),
    };
    public Dictionary<(MoveType, Vector2Int, Vector2Int), int> precalculatedDistances;
    public Dictionary<(Zone, Vector2Int), int> zoneDistances;
    
    public IEnumerable<Building> Buildings=> buildings.Values;
    public IEnumerable<Unit> Units => units.Values;

    public class TutorialState {
        public bool startedCapturing;
        public bool askedToCaptureBuilding;
        public bool explainedTurnEnd;
        public bool explainedApc;
        public bool explainedActionSelection;
    }
    public TutorialState tutorialState = new();

    [Command]
    public static bool EnableTutorial {
        get => PlayerPrefs.GetInt(nameof(EnableTutorial), 1) != 0;
        set => PlayerPrefs.SetInt(nameof(EnableTutorial), value ? 1 : 0);
    }

    public void Dispose() {
        foreach (var player in players)
            player.Dispose();
        foreach (var unit in units.Values.ToList())
            unit.Dispose();
        foreach (var building in buildings.Values.ToList())
            building.Dispose();
    }

    public int Day(int turn) {
        Assert.IsTrue(turn >= 0);
        Assert.AreNotEqual(0, players.Count);
        return turn / players.Count;
    }
    public int Day() {
        return Day(turn);
    }

    public Player CurrentPlayer {
        get {
            Assert.AreNotEqual(0, players.Count);
            Assert.IsTrue(turn != null);
            Assert.IsTrue(turn >= 0);
            return players[(int)turn % players.Count];
        }
    }

    public bool ContainsTile(Vector2Int position) {
        return tiles.ContainsKey(position);
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
    public IEnumerable<Vector2Int> Neighbors(Vector2Int position) {
        return from offset in Rules.gridOffsets where tiles.ContainsKey(position + offset) select position + offset;
    }
    
    public void SetGui(object key, Action action) {
        view.guiCommands[key] = action;
    }
    public void RemoveGui(object key) {
        view.guiCommands.Remove(key);
    }
}

public enum TriggerName { A, B, C, D, E, F }