using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static BattleConstants;

public class StaticBattleViews : MonoBehaviour {

    private static StaticBattleViews instance;
    public static StaticBattleViews Instance {
        get {
            if (instance)
                return instance;
            var objects = FindObjectsOfType<StaticBattleViews>();
            Assert.IsTrue(objects.Length <= 1);
            return instance = objects.Length == 1 ? objects[0] : nameof(StaticBattleViews).LoadAs<StaticBattleViews>();
        }
    }

    public struct View {
        public Transform transform;
        public List<Transform> spawnPoints;
    }

    public Dictionary<TileType, View>[] views = { new(), new() };

    [ContextMenu(nameof(Awake))]
    private void Awake() {
        var leftTree = transform.Find("Left");
        var rightTree = transform.Find("Right");
        Assert.IsTrue(leftTree);
        Assert.IsTrue(rightTree);
        ParseTree(Side.Left, leftTree);
        ParseTree(Side.Right, rightTree);
    }

    public void ParseTree(Side side, Transform tree) {

        void FindSpawnPoints(Transform node, List<Transform> output) {
            for (var i = 0; i < node.childCount; i++) {
                var child = node.GetChild(i);
                if (child.name.StartsWith("SpawnPoint"))
                    output.Add(child);
                else
                    FindSpawnPoints(child, output);
            }
        }

        views[(int)side].Clear();

        for (var i = 0; i < tree.childCount; i++) {

            var child = tree.GetChild(i);
            if (!Enum.TryParse(child.name, out TileType tileType))
                continue;
            Assert.IsTrue(!views[(int)side].ContainsKey(tileType), $"{side}, {tileType}");

            var view = new View {
                transform = child,
                spawnPoints = new List<Transform>()
            };
            views[(int)side].Add(tileType, view);
            FindSpawnPoints(child, view.spawnPoints);
        }
    }

    public bool TryGet(Side side, TileType tileType, out View view) {
        return views[(int)side].TryGetValue(tileType, out view);
    }
}

public class BattleSideView : IDisposable {

    public StaticBattleViews.View view;

    public BattleSideView(Side side, TileType tileType = TileType.Plain) {

        var battleViews = StaticBattleViews.Instance;
        if (!battleViews.TryGet(side, tileType, out view) && !battleViews.TryGet(side, TileType.Plain, out view))
            throw new AssertionException($"{side} {tileType}", null);
        view.transform.gameObject.SetActive(true);
    }

    public void Arrange(IEnumerable<UnitView> units) {
        var queue = new Queue<Transform>(view.spawnPoints);
        foreach (var unit in units) {
            var valid = queue.TryDequeue(out var spawnPoint);
            Assert.IsTrue(valid);
            unit.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            unit.PlaceOnTerrain();
        }
    }

    public void Dispose() {
        view.transform.gameObject.SetActive(false);
    }
}

public static class BattleConstants {
    public const int before = 0, after = 1;
}

public enum Side { Left = 0, Right = 1 }

public class BattleView : IDisposable {

    public class Settings {
        public class SideSettings {
            public UnitView unitViewPrefab;
            public Vector2Int count;
            public Transform parent;
            public Color color;
            public static int Count(UnitType unitType, int hp) {
                if (unitType == UnitType.Apc)
                    return hp > 0 ? 1 : 0;
                return (hp + 1) / 2;
            }
            public static Vector2Int Count(UnitType unitType, int hpBefore, int hpAfter) {
                return new Vector2Int(Count(unitType, hpBefore), Count(unitType, hpAfter));
            }
        }
        public SideSettings left, right;
        public SideSettings this[Side side] {
            get => side switch {
                Side.Left => left,
                Side.Right => right,
                _ => throw new ArgumentOutOfRangeException()
            };
            set {
                switch (side) {
                    case Side.Left:
                        left = value;
                        break;
                    case Side.Right:
                        right = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public static readonly HashSet<BattleView> undisposed = new();

    public readonly Dictionary<Side, List<UnitView>> units = new() {
        [Side.Left] = new List<UnitView>(),
        [Side.Right] = new List<UnitView>()
    };
    public readonly Dictionary<UnitView, List<UnitView>> targets = new();

    public void AddTarget(UnitView attacker, UnitView target) {
        if (!targets.TryGetValue(attacker, out var list)) {
            list = new List<UnitView>();
            targets.Add(attacker, list);
        }
        list.Add(target);
    }
    public IEnumerable<UnitView> GetTargets(UnitView attacker) {
        return targets.TryGetValue(attacker, out var list) ? list : Enumerable.Empty<UnitView>();
    }
    public bool TryRemoveTarget(UnitView attacker, UnitView target) {
        return targets.TryGetValue(attacker, out var list) && list.Remove(target);
    }

    public BattleView(Settings settings) {

        Assert.IsTrue(settings.left.unitViewPrefab);
        Assert.IsTrue(settings.left.count[before] >= 0);
        Assert.IsTrue(settings.left.count[after] <= settings.left.count[before]);

        Assert.IsTrue(settings.right.unitViewPrefab);
        Assert.IsTrue(settings.right.count[before] >= 0);
        Assert.IsTrue(settings.right.count[after] <= settings.right.count[before]);

        undisposed.Add(this);

        for (var side = Side.Left; side <= Side.Right; side++)
        for (var i = 0; i < settings[side].count[before]; i++) {
            var view = Object.Instantiate(settings[side].unitViewPrefab, settings[side].parent);
            view.ResetWeapons();
            //Assert.IsTrue(view);
            view.PlayerColor = settings[side].color;
            units[side].Add(view);
            view.survives = i < settings[side].count[after];
        }

        for (var i = 0; i < Mathf.Max(units[Side.Left].Count, units[Side.Right].Count); i++) {
            AddTarget(units[Side.Left][i % units[Side.Left].Count], units[Side.Right][i % units[Side.Right].Count]);
            if (settings.right.count[after] > 0)
                AddTarget(units[Side.Right][i % settings.right.count[after]], units[Side.Left][i % units[Side.Left].Count]);
        }
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);

        foreach (var unitView in units[Side.Left].Concat(units[Side.Right]))
            Object.Destroy(unitView.gameObject);
    }
}