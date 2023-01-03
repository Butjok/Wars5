using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class InteropTest : MonoBehaviour {

    public Player red, blue;

    public void Awake() {

        var game = GetComponent<Level>();
        Assert.IsTrue(game);

        red = new Player(game, Color.red, Team.Alpha);
        blue = new Player(game, Color.blue, Team.Alpha, type: PlayerType.Ai);
        game.localPlayer = red;

        game.turn = 0;

        var min = new Vector2Int(0, 0);
        var max = new Vector2Int(9, 9);

        game.tiles = new Map2D<TileType>(min, max);
        foreach (var position in game.tiles.Keys)
            game.tiles[position] = TileType.Plain;

        game.units = new Map2D<Unit>(min, max);
        game.buildings = new Map2D<Building>(min, max);

        game.levelLogic = new LevelLogic();

        new Unit(red, position: new Vector2Int(0, 0));
        new Unit(blue, position: new Vector2Int(1, 1));

        new Building(game, new Vector2Int(0, 0), TileType.Factory);
    }
}