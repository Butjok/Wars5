using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static BattleConstants;

public class BattleViews : MonoBehaviour {

    private static BattleViews instance;

    public static BattleViews Instance {
        get {
            if (instance)
                return instance;
            var objects = FindObjectsOfType<BattleViews>();
            Assert.IsTrue(objects.Length <= 1);
            return instance = objects.Length == 1 ? objects[0] : nameof(BattleViews).LoadAs<BattleViews>();
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

public class BattleViewSide : IDisposable {

    public BattleViews.View view;

    public BattleViewSide(Side side, BattleView battleView, TileType tileType = TileType.Plain) {
        var battleViews = BattleViews.Instance;
        if (!battleViews.TryGet(side, tileType, out view) && !battleViews.TryGet(side, TileType.Plain, out view))
            throw new AssertionException($"{side} {tileType}", null);
        view.transform.gameObject.SetActive(true);

        PlaceUnits(battleView.unitViews[side]);
    }

    public void PlaceUnits(IEnumerable<UnitView> units) {
        var queue = new Queue<Transform>(view.spawnPoints);
        var index = 0;
        foreach (var unit in units) {
            var valid = queue.TryDequeue(out var spawnPoint);
            Assert.IsTrue(valid);
            unit.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            unit.PlaceOnTerrain(true);
            unit.spawnPointIndex = index;
            unit.Dancing = false;
            var bipedalWalker = unit.GetComponent<BipedalWalker>();
            if (bipedalWalker) {
                unit.transform.localScale = Vector3.one * .66f;
                bipedalWalker.legLength *= .66f;
                bipedalWalker.height *= .66f;
                bipedalWalker.stepLength *= .66f;
            }
            index++;
        }
    }

    public void Dispose() {
        view.transform.gameObject.SetActive(false);
    }
}

public static class BattleConstants {
    public const int before = 0, after = 1;
}

public enum Side {
    Left = 0,
    Right = 1
}

public class BattleView : IDisposable {

    public class Setup {
        public class SideSettings {
            public Side side;
            public UnitView unitViewPrefab;
            public Vector2Int count;
            public Transform parent;
            public Color color;
            public WeaponName? weaponName;
            public static int Count(UnitType unitType, int hp) {
                if (unitType == UnitType.Apc)
                    return hp > 0 ? 1 : 0;
                return (hp + 1) / 2;
            }
            public static Vector2Int CountBeforeAndAfter(UnitType unitType, int hpBefore, int hpAfter) {
                return new Vector2Int(Count(unitType, hpBefore), Count(unitType, hpAfter));
            }
        }

        public SideSettings left, right;
        public SideSettings attacker, target;

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

    public readonly Dictionary<Side, List<UnitView>> unitViews = new() {
        [Side.Left] = new List<UnitView>(),
        [Side.Right] = new List<UnitView>()
    };
    public readonly Dictionary<UnitView, List<UnitView>> targetsMap = new();

    public void AddTarget(UnitView attacker, UnitView target, WeaponName weaponName) {
        if (!targetsMap.TryGetValue(attacker, out var list)) {
            list = new List<UnitView>();
            targetsMap.Add(attacker, list);
        }

        list.Add(target);
        if (!attacker.targets.Contains(target)) {
            attacker.targets.Add(target);
            target.incomingProjectilesLeft += attacker.GetShotsCount(weaponName);
            target.totalIncomingProjectiles = target.incomingProjectilesLeft;
        }
    }
    public IEnumerable<UnitView> GetTargets(UnitView attacker) {
        return targetsMap.TryGetValue(attacker, out var list) ? list : Enumerable.Empty<UnitView>();
    }
    public bool TryRemoveTarget(UnitView attacker, UnitView target) {
        return targetsMap.TryGetValue(attacker, out var list) && list.Remove(target);
    }

    public BattleView(Setup setup) {
        Assert.IsTrue(setup.left.unitViewPrefab);
        Assert.IsTrue(setup.left.count[before] >= 0);
        Assert.IsTrue(setup.left.count[after] <= setup.left.count[before]);

        Assert.IsTrue(setup.right.unitViewPrefab);
        Assert.IsTrue(setup.right.count[before] >= 0);
        Assert.IsTrue(setup.right.count[after] <= setup.right.count[before]);

        undisposed.Add(this);

        for (var side = Side.Left; side <= Side.Right; side++)
        for (var i = 0; i < setup[side].count[before]; i++) {
            var view = Object.Instantiate(setup[side].unitViewPrefab, setup[side].parent);
            view.ResetWeapons();
            //Assert.IsTrue(view);
            view.PlayerColor = setup[side].color;
            unitViews[side].Add(view);
            view.survives = i < setup[side].count[after];
            view.prefab = setup[side].unitViewPrefab;
        }

        var attackers = unitViews[setup.attacker.side];
        var targets = unitViews[setup.target.side];
        for (var i = 0; i < Mathf.Max(attackers.Count, targets.Count); i++) {
            AddTarget(attackers[i % attackers.Count], targets[i % targets.Count], (WeaponName)setup.attacker.weaponName);
            if (setup.target.count[after] > 0 && setup.target.weaponName is { } actualTargetWeaponName)
                AddTarget(targets[i % setup.target.count[after]], attackers[i % attackers.Count], actualTargetWeaponName);
        }
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);

        foreach (var unitView in unitViews[Side.Left].Concat(unitViews[Side.Right]))
            if (unitView)
                Object.Destroy(unitView.gameObject);
    }
}