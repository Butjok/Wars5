using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class Bridge2 : ISpawnable {
    public static readonly List<Bridge2> spawned = new();

    public Level level;
    private int hp = 10;
    private List<Vector2Int> positions = new();
    public Vector2Int? forward;

    [DontSave] public BridgeView2 view;
    [DontSave] public bool IsSpawned { get; private set; }

    [DontSave] public IReadOnlyList<Vector2Int> Positions => positions;
    public void SetPositions(List<Vector2Int> positions) {
        Assert.IsTrue(positions.Count > 0);
        for (var i = 1; i < positions.Count - 1; i++)
            Assert.IsTrue(positions[i + 1] - positions[i] == positions[1] - positions[0]);
        this.positions = positions;
        if (IsSpawned)
            view.SetPositions(this.positions, forward);
    }

    [DontSave] public Vector2Int? Forward {
        get => forward;
        set {
            forward = value;
            if (IsSpawned)
                view.SetPositions(positions, forward);
        }
    }

    public int Hp {
        get => hp;
        set {
            hp = value;
            if (hp > 0) {
                if (view)
                    view.Hp = value == Rules.MaxHp(this) ? null : value;
            }
            else
                DestroyAt(positions[0]);
        }
    }

    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        Assert.IsTrue(positions.Count > 0);
        spawned.Add(this);

        Assert.IsTrue(hp > 0);

        var prefab = "BridgeView2".LoadAs<BridgeView2>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.transform.position = positions[0].ToVector3();
        view.SetPositions(positions, forward);
        Hp = hp;

        IsSpawned = true;
    }

    public void Despawn() {
        Assert.IsTrue(spawned.Contains(this));
        spawned.Remove(this);

        if (view) {
            foreach (var piece in view.pieces.Values)
                Object.Destroy(piece.meshFilter.gameObject);
            Object.Destroy(view.gameObject);
            view = null;
        }

        IsSpawned = false;
    }

    public void Destroy() {
        foreach (var position in positions) {
            if (level.TryGetUnit(position, out var unit))
                unit.SetHp(0, true, false);
            level.bridges2.Remove(position);
            if (level.tiles.TryGetValue(position, out var tileType))
                level.tiles[position] = tileType == TileType.Bridge ? TileType.River : TileType.Sea;
        }
        Despawn();
    }
    
    [Command]
    public static void SetForwardFor(Vector2Int position, Vector2Int forward) {
        var level = Game.Instance.TryGetLevel;
        if (level == null || !level.TryGetBridge2(position, out var bridge))
            return;
        bridge.Forward = forward;
    }

    [Command]
    public static void DestroyAt(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level == null || !level.TryGetBridge2(position, out var bridge))
            return;
        bridge.Destroy();
    }

    [Command]
    public static bool TryAddPosition(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level == null || level.TryGetBridge2(position, out var bridge) || level.TryGetTile(position, out var tileType) && tileType is not (TileType.River or TileType.Sea))
            return false;
        var nearbyBridges = level.PositionsInRange(position, Vector2Int.one).Select(p => level.TryGetBridge2(p, out var b) ? b : null).Where(b => b != null).Distinct().ToList();
        if (nearbyBridges.Count > 1)
            return false;
        var needsToBeMaterialized = false;
        if (nearbyBridges.Count == 1)
            bridge = nearbyBridges[0];
        else {
            bridge = new Bridge2 {
                level = level,
                positions = new List<Vector2Int>()
            };
            needsToBeMaterialized = true;
        }
        if (bridge.positions.Count > 1) {
            Assert.IsTrue((bridge.positions[1] - bridge.positions[0]).ManhattanLength() == 1);
            var direction = bridge.positions[1] - bridge.positions[0];
            Assert.IsTrue(position == bridge.positions[^1] + direction || position == bridge.positions[0] - direction);
        }
        var newPositions = new List<Vector2Int>(bridge.positions) { position };
        bridge.SetPositions(newPositions);
        level.bridges2.Add(position, bridge);
        level.tiles[position] = tileType == TileType.River ? TileType.Bridge : TileType.BridgeSea;
        if (needsToBeMaterialized)
            bridge.Spawn();
        return true;
    }
}