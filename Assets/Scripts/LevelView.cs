using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class LevelView : MonoBehaviour {

    public static string TryGetSceneName(MissionName missionName) {
        return missionName switch {
            _ => null
        };
    }
    public static bool TryLoadScene(MissionName missionName) {
        var requiredSceneName = TryGetSceneName(missionName);
        if (requiredSceneName == null || SceneManager.GetActiveScene().name == requiredSceneName)
            return false;
        SceneManager.LoadScene(requiredSceneName);
        return true;
    }
    public static bool TryUnloadScene(MissionName missionName) {
        var requiredSceneName = TryGetSceneName(missionName);
        if (requiredSceneName == null || SceneManager.GetActiveScene().name != requiredSceneName)
            return false;
        SceneManager.UnloadSceneAsync(requiredSceneName);
        return true;
    }
    public static bool TryFindPrefab(out LevelView prefab) {
        var instances = FindObjectsOfType<LevelView>(true).Where(i => !i.prefab).ToList();
        Assert.IsTrue(instances.Count is 0 or 1);
        prefab = instances.Count == 0 ? null : instances[0];
        return prefab;
    }
    public static bool TryInstantiatePrefab(out LevelView instance) {
        if (!TryFindPrefab(out var prefab)) {
            instance = null;
            return false;
        }
        prefab.gameObject.SetActive(false);
        instance = Instantiate(prefab);
        instance.prefab = prefab;
        instance.gameObject.SetActive(true);
        return true;
    }

    public LevelView prefab;
    public Camera[] battleCameras = { null, null };
    public CameraRig cameraRig;
    public Material terrainMaterial;
    public Canvas canvas;
    public DialogueUi3 dialogueUi;
    public MinimapUi minimap;
    public TilemapCursor tilemapCursor;
    public Sun sun;
    public TurnButton turnButton;
    public UnitBuildMenu2 unitBuildMenu;
}