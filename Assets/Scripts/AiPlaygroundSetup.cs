using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(DebugTerrainMeshGenerator))]
public class AiPlaygroundSetup : MonoBehaviour {

    public Game game;
    public DebugTerrainMeshGenerator terrainMeshGenerator;

    public void Awake() {
        terrainMeshGenerator = GetComponent<DebugTerrainMeshGenerator>();
        game = Testing.CreateGame(new Testing.Options {
            min = new Vector2Int(-5,-5),
            max = new Vector2Int(5,5)
        });
        terrainMeshGenerator.game = game;
        terrainMeshGenerator.Generate();

        game.levelLogic = new LevelLogic();
        game.StartCoroutine(SelectionState.New(game, true));
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