using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using Object = UnityEngine.Object;

[RequireComponent(typeof(DebugTerrainMeshGenerator))]
public class AiPlayground : MonoBehaviour {

    public Level level;
    public DebugTerrainMeshGenerator terrainMeshGenerator;
    public UnitBrain unitBrain;

    private void Update() {

        if (Input.GetKeyDown(KeyCode.KeypadEnter)) {

            if (!level)
                level = FindObjectOfType<Level>();
            if (!level)
                return;

            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) &&
                Mouse.TryGetPosition(out Vector2Int mousePosition) &&
                level.TryGetUnit(mousePosition, out var selectedUnit) &&
                selectedUnit.view) {

                if (unitBrain)
                    unitBrain.debugDraw = false;
                
                unitBrain = selectedUnit.view.GetComponent<UnitBrain>();
                if (unitBrain) {
                    unitBrain.debugDraw = true;
                    Selection.objects = new Object[] { unitBrain };
                }
                else
                    Selection.objects = null;
            }
            else {
                Selection.objects = null;
                if (unitBrain) {
                    unitBrain.debugDraw = false;
                    unitBrain = null;
                }
            }
        }
    }
}