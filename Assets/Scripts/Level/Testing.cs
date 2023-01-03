using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class Testing {

    public class Options {
        public Vector2Int min = new Vector2Int(-10, -10);
        public Vector2Int max = new Vector2Int(10, 10);
        public IEnumerable<Color> colors = new[] { Color.red, Color.blue };
        public IEnumerable<(Color color, UnitType type, Vector2Int position)> units = new[] {
            (Color.red, UnitType.Infantry, Vector2Int.zero),
            (Color.blue, UnitType.Infantry, Vector2Int.one),
        };
        public IEnumerable<(Color color, TileType type, Vector2Int position)> buildings = new[] {
            (Color.red, TileType.Hq, Vector2Int.zero),
            (Color.blue, TileType.Hq, Vector2Int.one),
        };
    }
    public static Team[] teamLoop = { Team.Alpha, Team.Bravo, Team.Charlie, Team.Delta };

    public static Level CreateGame(Options options = null) {

        options ??= new Options();

        var go = new GameObject(nameof(Testing));
        Object.DontDestroyOnLoad(go);
        var game = go.AddComponent<Level>();
        
        var index = 0;
        foreach (var color in options.colors)
            new Player(game, color, teamLoop[index++ % teamLoop.Length]);

        game.tiles = new Map2D<TileType>(options.min, options.max);
        game.units = new Map2D<Unit>(options.min, options.max);
        game.buildings = new Map2D<Building>(options.min, options.max);

        for (var x = options.min.x; x <= options.max.x; x++)
        for (var y = options.min.y; y <= options.max.y; y++)
            game.tiles[new Vector2Int(x, y)] = TileType.Plain;

        foreach (var unit in options.units) {
            Assert.IsTrue(game.players.TryGet(unit.color, out var player));
            new Unit(player, type: unit.type, position: unit.position);
        }

        foreach (var building in options.buildings) {
            game.players.TryGet(building.color, out var player);
            new Building(game, building.position, building.type, player);
        }

        game.localPlayer = game.players[0];

        return game;
    }
}