using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using Object = UnityEngine.Object;

[RequireComponent(typeof(DebugTerrainMeshGenerator))]
public class AiPlayground : MonoBehaviour {

    public Game game;
    public DebugTerrainMeshGenerator terrainMeshGenerator;
    public UnitBrain unitBrain;

    private void Update() {

        if (Input.GetKeyDown(KeyCode.KeypadEnter)) {

            if (!game)
                game = FindObjectOfType<Game>();
            if (!game)
                return;

            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) &&
                Mouse.TryGetPosition(out Vector2Int mousePosition) &&
                game.TryGetUnit(mousePosition, out var selectedUnit) &&
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