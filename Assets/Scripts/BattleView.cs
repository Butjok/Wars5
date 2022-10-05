using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CameraRectDriver))]
public class BattleView : MonoBehaviour {

    public List<UnitView> unitViews = new();
    public Dictionary<UnitView, List<ImpactPoint>> impactPoints = new();

    public Transform[] spawnPoints = Array.Empty<Transform>();

    public TileTypeGameObjectDictionary backgrounds = new();
    public GameObject background;
    public CameraRectDriver cameraRectDriver;
    public bool shuffleUnitViews = true;

    [ContextMenu(nameof(FindSpawnPoints))]
    public void FindSpawnPoints() {
        spawnPoints = GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("SpawnPoint")).ToArray();
    }

    public void Awake() {
        cameraRectDriver = GetComponent<CameraRectDriver>();
        Assert.IsTrue(cameraRectDriver);
    }

    public void Setup(UnitView unitViewPrefab, int count, TileType tileType = TileType.Plain) {

        Assert.IsTrue(count <= spawnPoints.Length);
        Assert.IsTrue(unitViewPrefab);
        
        Cleanup();
        
        if (backgrounds.TryGetValue(tileType, out  background) && background)
            background.SetActive(true);

        for (var i = 0; i < count; i++) {
            var spawnPoint = spawnPoints[i];
            var unitView = Instantiate(unitViewPrefab, spawnPoint.position, spawnPoint.rotation, transform);
            unitView.gameObject.SetLayerRecursively(gameObject.layer);
            unitView.PlaceOnTerrain();
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
        
        if (background)
            background.SetActive(false);
        
        foreach (var unitView in unitViews)
            Destroy(unitView.gameObject);
        unitViews.Clear();
    }

    public void AssignTargets(IList<UnitView> targets) {

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

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            Shoot();
    }
}

[Serializable]
public class TileTypeGameObjectDictionary : SerializableDictionary<TileType, GameObject> { }