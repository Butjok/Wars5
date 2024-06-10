using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class Bridge2 : IMaterialized {
    public static readonly List<Bridge2> toDematerialize = new();

    public Level level;
    public int hp = 10;
    private List<Vector2Int> positions = new();

    [DontSave] public BridgeView2 view;
    [DontSave] public bool IsMaterialized { get; private set; }

    [DontSave] public IReadOnlyList<Vector2Int> Positions => positions;
    public void SetPositions(List<Vector2Int> positions) {
        Assert.IsTrue(positions.Count > 0);
        for (var i = 1; i < positions.Count - 1; i++)
            Assert.IsTrue(positions[i + 1] - positions[i] == positions[1] - positions[0]);
        this.positions = positions;
        if (IsMaterialized)
            view.SetPositions(this.positions);
    }

    public void Materialize() {
        Assert.IsFalse(IsMaterialized);
        Assert.IsFalse(toDematerialize.Contains(this));
        Assert.IsTrue(positions.Count > 0);
        toDematerialize.Add(this);

        var prefab = "BridgeView2".LoadAs<BridgeView2>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.transform.position = positions[0].ToVector3();
        view.SetPositions(positions);

        IsMaterialized = true;
    }

    public void Dematerialize() {
        Assert.IsTrue(toDematerialize.Contains(this));
        toDematerialize.Remove(this);

        if (view) {
            foreach (var piece in view.pieces.Values)
                Object.Destroy(piece.meshFilter.gameObject);
            Object.Destroy(view.gameObject);
            view = null;
        }

        IsMaterialized = false;
    }
    
    public void Destroy() {
        foreach (var position in positions) {
            if (level.TryGetUnit(position, out var unit))
                unit.SetHp(0, true);
            level.bridges2.Remove(position);
            level.tiles[position] = level.tiles[position] == TileType.Bridge ? TileType.River : TileType.Sea;
        }
        Dematerialize();
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
        var newPositions = new List<Vector2Int>(bridge.positions) {position};
        bridge.SetPositions(newPositions);
        level.bridges2.Add(position, bridge);
        level.tiles[position] = tileType == TileType.River ? TileType.Bridge : TileType.BridgeSea;
        if (needsToBeMaterialized)
            bridge.Materialize();
        return true;
    }
}