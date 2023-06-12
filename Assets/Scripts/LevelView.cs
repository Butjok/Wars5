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
    public static LevelView TryFind() {
        var instances = FindObjectsOfType<LevelView>(true).Where(i => !i.prefab).ToList();
        Assert.IsTrue(instances.Count is 0 or 1);
        return instances.Count == 0 ? null : instances[0];
    }
    public static LevelView TryInstantiate() {
        var prefab = TryFind();
        if (!prefab)
            return null;
        prefab.gameObject.SetActive(false);
        var instance = Instantiate(prefab);
        instance.prefab = prefab;
        instance.gameObject.SetActive(true);
        return instance;
    }

    public LevelView prefab;
    public Camera[] battleCameras = { null, null };
    public CameraRig cameraRig;
    public Material terrainMaterial;
    public CursorView cursorView;
    public Canvas canvas;
    public DialogueUi3 dialogueUi;
    public MinimapUi minimap;
}