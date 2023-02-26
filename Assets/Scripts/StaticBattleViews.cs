using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
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

    public void Arrange(IEnumerable<UnitView> unitViews) {
        var queue = new Queue<Transform>(view.spawnPoints);
        foreach (var unitView in unitViews) {
            var valid = queue.TryDequeue(out var spawnPoint);
            Assert.IsTrue(valid);
            unitView.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
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
            public UnitView unitViewPrefab;
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

    public readonly List<UnitView>[] units = { new(), new() };
    public readonly Dictionary<UnitView, List<UnitView>> targets = new();
    public readonly Dictionary<UnitView, int> incomingRoundsLeft = new();
    public readonly  HashSet<UnitView> survivors = new ();

    public void AddTarget(UnitView attacker, UnitView target, int shotsCount) {
        if (!targets.TryGetValue(attacker, out var list)) {
            list = new List<UnitView>();
            targets.Add(attacker, list);
        }
        incomingRoundsLeft[target] = (incomingRoundsLeft.TryGetValue(target, out var count) ? count : 0) + shotsCount;
        list.Add(target);
    }
    public IEnumerable<UnitView> GetTargets(UnitView attacker) {
        return targets.TryGetValue(attacker, out var list) ? list : Enumerable.Empty<UnitView>();
    }
    public bool TryRemoveTarget(UnitView attacker, UnitView target) {
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
            var unitView = Object.Instantiate(setup[side].unitViewPrefab, setup[side].parent);
            unitView.PlayerColor = setup[side].color;
            units[side].Add(unitView);
            if (i < setup[side].count[after])
                survivors.Add(unitView);
        }

        var shotsCount = new Vector2Int(
            setup.left.unitViewPrefab.battleAnimationPlayer.ShotsCount,
            setup.right.unitViewPrefab.battleAnimationPlayer.ShotsCount);

        for (var i = 0; i < Mathf.Max(units[left].Count, units[right].Count); i++) {
            AddTarget(units[left][i % units[left].Count], units[right][i % units[right].Count], shotsCount[left]);
            AddTarget(units[right][i % units[right].Count], units[left][i % units[left].Count], shotsCount[right]);
        }
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);

        foreach (var unitView in units[left].Concat(units[right]))
            Object.Destroy(unitView.gameObject);
    }
}