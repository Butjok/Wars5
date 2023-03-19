using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BattleView : MonoBehaviour {

    public struct TargetingSetup {
        public Dictionary<UnitViewOld, List<UnitViewOld>> targets;
        public Dictionary<UnitViewOld, int> remainingAttackersCount;
    }

    public List<UnitViewOld> unitViews = new();
    public Transform target;

    public Transform[] spawnPoints = Array.Empty<Transform>();

    private void Awake() {
        EnsureInitialized();
    }

    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;
        spawnPoints = GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("SpawnPoint")).ToArray();
        if (FindObjectOfType<Level>())
            foreach (var spawnPoint in spawnPoints)
                spawnPoint.gameObject.SetActive(false);
    }

    public void Setup(UnitViewOld unitViewPrefab, int count) {

        EnsureInitialized();

        Assert.IsTrue(count <= spawnPoints.Length);
        Assert.IsTrue(unitViewPrefab);

        Cleanup();

        /*for (var i = 0; i < count; i++) {
            var spawnPoint = spawnPoints[i];
            var unitView = Instantiate(unitViewPrefab, spawnPoint.position, spawnPoint.rotation);
            //unitView.transform.localScale = spawnPoint.localScale;
            DontDestroyOnLoad(unitView.gameObject);
            unitView.gameObject.SetLayerRecursively(gameObject.layer);
            unitView.PlaceOnTerrain(true);
            unitViews.Add(unitView);
            unitView.turret.computer.Target = target;
        }

        foreach (var unitView in unitViews) {
            if (unitView.moveAndAttack)
                unitView.moveAndAttack.siblings = unitViews.Select(item => item.moveAndAttack).ToArray();
            if (unitView.attack)
                unitView.attack.siblings = unitViews.Select(item => item.attack).ToArray();
            if (unitView.respond)
                unitView.respond.siblings = unitViews.Select(item => item.respond).ToArray();
        }*/

        if (unitViews.Count > 0) {
            var manualControl = unitViews[0].GetComponent<ManualControl>();
            if (manualControl)
                manualControl.enabled = true;
        }
    }

    public void Cleanup() {
        foreach (var unitView in unitViews)
            if (unitView)
                Destroy(unitView.gameObject);
        unitViews.Clear();
    }

    public static TargetingSetup AssignTargets(IReadOnlyList<UnitViewOld> attackers, IReadOnlyList<UnitViewOld> targets, IReadOnlyCollection<UnitViewOld> survivors) {

        Assert.AreNotEqual(0, attackers.Count);
        Assert.AreNotEqual(0, targets.Count);

        var setup = new TargetingSetup {
            targets = new Dictionary<UnitViewOld, List<UnitViewOld>>(),
            remainingAttackersCount = new Dictionary<UnitViewOld, int>()
        };

        foreach (var attacker in attackers)
            setup.targets.Add(attacker, new List<UnitViewOld>());
        foreach (var target in targets)
            setup.remainingAttackersCount.Add(target, survivors.Contains(target) ? 999 : 0);

        for (var i = 0; i < Mathf.Max(attackers.Count, targets.Count); i++) {

            var attacker = attackers[i % attackers.Count];
            var target = targets[i % targets.Count];

            setup.targets[attacker].Add(target);
            setup.remainingAttackersCount[target]++;
        }

        return setup;
    }
}

[Serializable]
public class TileTypeGameObjectDictionary : SerializableDictionary<TileType, GameObject> { }