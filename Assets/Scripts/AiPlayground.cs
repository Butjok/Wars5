using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using  UnityEditor;
using Object = UnityEngine.Object;

[RequireComponent(typeof(DebugTerrainMeshGenerator))]
public class AiPlayground : MonoBehaviour {

    public Game game;
    public DebugTerrainMeshGenerator terrainMeshGenerator;
    public UnitBrain unitBrain;

    public void Start() {
        terrainMeshGenerator = GetComponent<DebugTerrainMeshGenerator>();
        game = Testing.CreateGame(new Testing.Options {
            min = new Vector2Int(-5, -5),
            max = new Vector2Int(5, 5)
        });
        terrainMeshGenerator.game = game;
        terrainMeshGenerator.Generate();

        game.levelLogic = new LevelLogic();
        // game.StartCoroutine(SelectionState.New(game, true));
    }

    private void Update() {
        
        if (Input.GetKeyDown(KeyCode.KeypadEnter)) {

            if (!Input.GetKey(KeyCode.LeftShift) && ! Input.GetKey(KeyCode.RightShift) &&
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

    [Command]
    public void AddBuilding(Vector2Int position, TileType type, int playerIndex = -1) {
        Assert.IsTrue(TileType.PlayerOwned.HasFlag(type), type.ToString());
        new Building(game, position, type, playerIndex == -1 ? null : game.players[playerIndex]);
        terrainMeshGenerator.Generate();
    }

    [Command]
    public void AddTile(Vector2Int position, TileType type) {
        Assert.IsFalse(TileType.PlayerOwned.HasFlag(type), type.ToString());
        game.tiles[position] = type;
        terrainMeshGenerator.Generate();
    }
}