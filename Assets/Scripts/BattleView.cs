using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class BattleView : MonoBehaviour {

    public List<UnitView> unitViews = new();
    public Dictionary<UnitView, List<ImpactPoint>> impactPoints = new();

    public Transform[] spawnPoints = Array.Empty<Transform>();
    public bool shuffleUnitViews = true;

    private void Awake() {
        EnsureInitialized();
    }

    [ContextMenu(nameof(Initialize))]
    private void Initialize() {
        spawnPoints = GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("SpawnPoint")).ToArray();
        Assert.AreNotEqual(0,spawnPoints.Length);
    }

    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;
        Initialize();
    }

    public void Setup(UnitView unitViewPrefab, int count, TileType tileType = TileType.Plain) {

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
        }

        if (shuffleUnitViews)
            unitViews = unitViews.OrderBy(_ => Random.value).ToList();

        if (unitViews.Count > 0) {
            var manualControl = unitViews[0].GetComponent<ManualControl>();
            if (manualControl)
                manualControl.enabled = true;
        }
    }

    public void Cleanup() {
        
        EnsureInitialized();
        
        foreach (var unitView in unitViews)
            Destroy(unitView.gameObject);
        unitViews.Clear();
    }

    public void AssignTargets(IList<UnitView> targets) {

        EnsureInitialized();
        
        Assert.AreNotEqual(0, unitViews.Count);
        Assert.AreNotEqual(0, targets.Count);

        impactPoints.Clear();
        foreach (var unitView in unitViews)
            impactPoints.Add(unitView, new List<ImpactPoint>());

        for (var i = 0; i < Mathf.Max(unitViews.Count, targets.Count); i++) {

            var attacker = unitViews[i % unitViews.Count];
            var target = targets[i % targets.Count];

            Assert.AreNotEqual(0, target.impactPoints.Length);
            var impactPoint = target.impactPoints.Random();
            impactPoints[attacker].Add(impactPoint);
        }

        foreach (var attacker in impactPoints.Keys)
            if (attacker.turret && attacker.turret.computer)
                attacker.turret.computer.Target = impactPoints[attacker].Random().transform;
    }

    public int shooterIndex = -1;

    public bool Shoot() {
        if (unitViews.Count == 0)
            return false;
        shooterIndex = (shooterIndex + 1) % unitViews.Count;
        var shooter = unitViews[shooterIndex];
        if (impactPoints.TryGetValue(shooter, out var list) && list.Count > 0) {
            shooter.turret.Fire(list);
            return true;
        }
        return false;
    }

    public void MoveAndShoot() {
        foreach (var unitView in unitViews)
            unitView.moveAndShoot.Play(impactPoints[unitView]);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            Shoot();
    }
}

[Serializable]
public class TileTypeGameObjectDictionary : SerializableDictionary<TileType, GameObject> { }