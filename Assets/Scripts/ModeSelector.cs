using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class ModeSelector : MonoBehaviour {

    public enum Mode { None, Terrain, Roads, Props }

    public TerrainCreator terrainCreator;
    public RoadCreator roadCreator;
    public PropPlacement propPlacement;

    public Mode startMode = Mode.None;

    private void Awake() {

        Assert.IsTrue(terrainCreator);
        Assert.IsTrue(roadCreator);
        Assert.IsTrue(propPlacement);

        DisableAll();
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

    public void DisableAll() {
        terrainCreator.enabled = false;
        roadCreator.enabled = false;
        propPlacement.enabled = false;
    }

    [Command]
    public void SwitchToTerrainMode() {
        DisableAll();
        terrainCreator.enabled = true;
    }

    [Command]
    public void SwitchToRoadMode() {
        DisableAll();
        roadCreator.enabled = true;
    }

    [Command]
    public void SwitchToPropMode() {
        DisableAll();
        propPlacement.enabled = true;
    }
}