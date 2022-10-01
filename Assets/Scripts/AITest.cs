using System;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour {

    public Game2 game;
    public Player red, blue;

    public void Awake() {

        game = gameObject.AddComponent<Game2>();

        red = new Player(game, Color.red, Team.Alpha);
        blue = new Player(game, Color.blue, Team.Alpha, type: PlayerType.Ai);
        game.players = new List<Player> { red, blue };
        game.realPlayer = red;

        game.turn = 0;

        var min = new Vector2Int(0, 0);
        var max = new Vector2Int(10, 10);

        game.tiles = new Map2D<TileType>(min, max);
        foreach (var position in game.tiles.Keys)
            game.tiles[position] = TileType.Plain;

        game.units = new Map2D<Unit>(min, max);
        game.buildings = new Map2D<Building>(min, max);

        game.levelLogic = new DefaultLevelLogic();
    }

    public void OnDrawGizmos() {
        
    }
}