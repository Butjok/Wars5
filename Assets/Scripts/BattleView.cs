using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BattleView : MonoBehaviour {

    public List<UnitView> unitViews = new();
    public Dictionary<UnitView, List<ImpactPoint>> impactPoints = new();
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
    }

    public void Setup(UnitView unitViewPrefab, int count) {

        EnsureInitialized();

        Assert.IsTrue(count <= spawnPoints.Length);
        Assert.IsTrue(unitViewPrefab);

        Cleanup();

        for (var i = 0; i < count; i++) {
            var spawnPoint = spawnPoints[i];
            var unitView = Instantiate(unitViewPrefab, spawnPoint.position, spawnPoint.rotation, transform);
            //unitView.transform.localScale = spawnPoint.localScale;
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
        }

        if (unitViews.Count > 0) {
            var manualControl = unitViews[0].GetComponent<ManualControl>();
            if (manualControl)
                manualControl.enabled = true;
        }
    }

    public void Cleanup() {
        foreach (var unitView in unitViews)
            Destroy(unitView.gameObject);
        unitViews.Clear();
    }

    public static Dictionary<UnitView, List<ImpactPoint>> AssignTargets(IReadOnlyList<UnitView> attackers, IReadOnlyList<UnitView> targets) {

        Assert.AreNotEqual(0, attackers.Count);
        Assert.AreNotEqual(0, targets.Count);

        var impactPoints = new Dictionary<UnitView, List<ImpactPoint>>();
        foreach (var unitView in attackers)
            impactPoints.Add(unitView, new List<ImpactPoint>());

        for (var i = 0; i < Mathf.Max(attackers.Count, targets.Count); i++) {

            var attacker = attackers[i % attackers.Count];
            var target = targets[i % targets.Count];

            Assert.AreNotEqual(0, target.impactPoints.Length);
            var impactPoint = target.impactPoints.Random();
            impactPoints[attacker].Add(impactPoint);
        }

        return impactPoints;
    }
}

[Serializable]
public class TileTypeGameObjectDictionary : SerializableDictionary<TileType, GameObject> { }