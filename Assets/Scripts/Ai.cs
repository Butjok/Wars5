using System.Collections.Generic;
using System.Linq;
using FullscreenEditor;
using UnityEngine;

public static class Ai {

    public static UnitAction FindBestAction(Game game) {

        var player = game.CurrentPlayer;
        var players = game.players;
        var tiles = game.tiles;
        var units = game.units.Values.ToList();
        var enemies = game.players.Where(p => Rules.AreEnemies(player, p)).ToList();
        var playerUnits = units.Where(u => u.player == player).ToList();
        var unmovedPlayerUnits = playerUnits.Where(u => !u.moved.v).ToList();
        var unmovedPlayerArtilleryUnits = unmovedPlayerUnits.Where(u => Rules.IsArtillery(u)).ToList();


        return null;
    }

    public static IEnumerable<Vector2Int> RangePositions(Map2D<TileType> tiles, Vector2Int position, Vector2Int range) {
        return range.Offsets()
            .Select(offset => position + offset)
            .Where(position => tiles.TryGetValue(position, out var tileType) && tileType != 0);
    }
    public static IEnumerable<Unit> RangeTargets(Map2D<TileType> tiles, Map2D<Unit> units, Vector2Int position, Vector2Int range) {
        foreach (var p in RangePositions(tiles, position, range))
            if (units.TryGetValue(p, out var unit) && unit != null)
                yield return unit;
    }
    
}