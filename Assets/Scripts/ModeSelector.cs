using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class ModeSelector : MonoBehaviour {

    public enum Mode { None, Terrain, Roads, Props }

    public TerrainCreator terrainCreator;
    public RoadCreator roadCreator;
    public PropPlacement propPlacement;

    public Mode startMode = Mode.None;

    [Command]
    public void TryLoad(string name) {
        if (!terrainCreator.TryLoad(name))
            terrainCreator.Clear();
        if (!roadCreator.TryLoad(name))
            roadCreator.Clear();
        if (!propPlacement.TryLoad(name))
            propPlacement.Clear();
    }
    [Command]
    public void Save(string name) {
        terrainCreator.Save(name);
        roadCreator.Save(name);
        propPlacement.Save(name);
    }

    
    private void OnApplicationQuit() {
        Save("Autosave");
    }

    private void Awake() {

        Assert.IsTrue(terrainCreator);
        Assert.IsTrue(roadCreator);
        Assert.IsTrue(propPlacement);

        DisableAllModes();
        switch (startMode) {
            case Mode.Terrain:
                SwitchToTerrainMode();
                break;
            case Mode.Roads:
                SwitchToRoadMode();
                break;
            case Mode.Props:
                SwitchToPropMode();
                break;
        }
    }

    [Command]
    public void DisableAllModes() {
        terrainCreator.enabled = false;
        roadCreator.enabled = false;
        propPlacement.enabled = false;
    }

    [Command]
    public void SwitchToTerrainMode() {
        DisableAllModes();
        terrainCreator.enabled = true;
    }

    [Command]
    public void SwitchToRoadMode() {
        DisableAllModes();
        roadCreator.enabled = true;
    }

    [Command]
    public void SwitchToPropMode() {
        DisableAllModes();
        propPlacement.enabled = true;
    }
}