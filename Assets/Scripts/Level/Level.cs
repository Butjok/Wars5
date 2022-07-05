using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class Level : IDisposable
{

    public UnitMap unitMap = new();
    public Dictionary<Vector2Int, TileType> tiles = new();
    public List<Player> playerLoop = new();
    public Dictionary<Color, Player> players = new();
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
        unit = unitMap[position];
        return unit != null;
    }
    public bool TryGetBuilding<T>(Vector2Int position, out T result) where T : Building {
        var found = buildings.TryGetValue(position, out var building);
        result = found ? (T)building : default;
        return found;
    }
}

public abstract class LevelState : IDisposable
{

    public Level level;
    protected LevelState(Level level) {
        Assert.IsNotNull(level);
        this.level = level;
    }

    public Player CurrentPlayer {
        get {
            Assert.AreNotEqual(0, PlayerLoop.Count);
            return level.playerLoop[Turn % PlayerLoop.Count];
        }
    }
    public Dictionary<Vector2Int, TileType> Tiles => level.tiles;
    public UnitMap UnitMap => level.unitMap;
    public List<Player> PlayerLoop => level.playerLoop;
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