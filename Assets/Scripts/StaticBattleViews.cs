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
        ParseTree(left, leftTree);
        ParseTree(right, rightTree);
    }

    public void ParseTree(int side, Transform tree) {

        void FindSpawnPoints(Transform node, List<Transform> output) {
            for (var i = 0; i < node.childCount; i++) {
                var child = node.GetChild(i);
                if (child.name.StartsWith("SpawnPoint"))
                    output.Add(child);
                else
                    FindSpawnPoints(child, output);
            }
        }

        views[side].Clear();

        for (var i = 0; i < tree.childCount; i++) {

            var child = tree.GetChild(i);
            if (!Enum.TryParse(child.name, out TileType tileType))
                continue;
            Assert.IsTrue(!views[side].ContainsKey(tileType), $"{side}, {tileType}");

            var view = new View {
                transform = child,
                spawnPoints = new List<Transform>()
            };
            views[side].Add(tileType, view);
            FindSpawnPoints(child, view.spawnPoints);
        }
    }

    public bool TryGet(int side, TileType tileType, out View view) {
        return views[side].TryGetValue(tileType, out view);
    }
}

public class BattleView2 : IDisposable {

    public StaticBattleViews.View view;

    public BattleView2(int side, TileType tileType = TileType.Plain) {

        var battleViews = StaticBattleViews.Instance;
        if (!battleViews.TryGet(side, tileType, out view) && !battleViews.TryGet(side, TileType.Plain, out view))
            throw new AssertionException($"{side} {tileType}", null);
        view.transform.gameObject.SetActive(true);
    }

    public void Arrange(IEnumerable<BattleAnimationPlayer> units) {
        var queue = new Queue<Transform>(view.spawnPoints);
        foreach (var unit in units) {
            var valid = queue.TryDequeue(out var spawnPoint);
            Assert.IsTrue(valid);
            unit.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }
    }

    public void Dispose() {
        view.transform.gameObject.SetActive(false);
    }
}

public static class BattleConstants {
    public const int before = 0, after = 1;
    public const int left = 0, right = 1;
}

public class Battle : IDisposable {

    public class Setup {
        public class Side {
            public BattleAnimationPlayer unitViewPrefab;
            public Vector2Int count;
            public Transform parent;
            public Color color;
            public static int Count(int hp) => (hp + 1) / 2;
        }
        public Side left, right;
        public Side this[int index] => index switch {
            0 => left, 1 => right, _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static readonly HashSet<Battle> undisposed = new();

    public readonly List<BattleAnimationPlayer>[] units = { new(), new() };
    public readonly Dictionary<BattleAnimationPlayer, List<BattleAnimationPlayer>> targets = new();
    public readonly Dictionary<BattleAnimationPlayer, int> incomingRoundsLeft = new();

    public void AddTarget(BattleAnimationPlayer attacker, BattleAnimationPlayer target, int shotsCount) {
        if (!targets.TryGetValue(attacker, out var list)) {
            list = new List<BattleAnimationPlayer>();
            targets.Add(attacker, list);
        }
        incomingRoundsLeft[target] = (incomingRoundsLeft.TryGetValue(target, out var count) ? count : 0) + shotsCount;
        list.Add(target);
    }
    public IEnumerable<BattleAnimationPlayer> GetTargets(BattleAnimationPlayer attacker) {
        return targets.TryGetValue(attacker, out var list) ? list : Enumerable.Empty<BattleAnimationPlayer>();
    }
    public bool TryRemoveTarget(BattleAnimationPlayer attacker, BattleAnimationPlayer target) {
        return targets.TryGetValue(attacker, out var list) && list.Remove(target);
    }

    public Battle(Setup setup) {

        Assert.IsTrue(setup.left.unitViewPrefab);
        Assert.IsTrue(setup.left.count[before] >= 0);
        Assert.IsTrue(setup.left.count[after] <= setup.left.count[before]);

        Assert.IsTrue(setup.right.unitViewPrefab);
        Assert.IsTrue(setup.right.count[before] >= 0);
        Assert.IsTrue(setup.right.count[after] <= setup.right.count[before]);

        undisposed.Add(this);

        for (var side = left; side <= right; side++)
        for (var i = 0; i < setup[side].count[before]; i++) {
            var unit = Object.Instantiate(setup[side].unitViewPrefab, setup[side].parent);
            var view = unit.GetComponent<UnitView>();
            Assert.IsTrue(view);
            view.PlayerColor = setup[side].color;
            units[side].Add(unit);
            unit.survives = i < setup[side].count[after];
        }

        var shotsCount = new Vector2Int(
            setup.left.unitViewPrefab.ShotsCount,
            setup.right.unitViewPrefab.ShotsCount);

        for (var i = 0; i < Mathf.Max(units[left].Count, units[right].Count); i++) {
            AddTarget(units[left][i % units[left].Count], units[right][i % units[right].Count], shotsCount[left]);
            if (setup.right.count[after] > 0)
                AddTarget(units[right][i % setup.right.count[after]], units[left][i % units[left].Count], shotsCount[right]);
        }
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);

        foreach (var unitView in units[left].Concat(units[right]))
            Object.Destroy(unitView.gameObject);
    }
}