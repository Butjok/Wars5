using System.Collections.Generic;
using System.Linq;
using FullscreenEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public  class Ai :MonoBehaviour{

    public static UnitAction FindBestAction(Level level) {

        var player = level.CurrentPlayer;
        var players = level.players;
        var tiles = level.tiles;
        var units = level.units.Values.ToList();
        var enemies = level.players.Where(p => Rules.AreEnemies(player, p)).ToList();
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

    public Level level;
    public int playerIndex;
    
    private void OnDrawGizmos() {

        void DrawWireTile(Vector2Int position, Color color) {
            Gizmos.color = color;
            Gizmos.DrawWireCube(position.ToVector3Int(), Vector2Int.one.ToVector3Int());
        }
        void DrawSolidTile(Vector2Int position, Color color) {
            Gizmos.color = color;
            Gizmos.DrawCube(position.ToVector3Int(), Vector2Int.one.ToVector3Int());
        }
        
        if (!level) {
            level = FindObjectOfType<Level>();
            if (!level)
                return;
        }
        
        foreach (var (position, tileType) in level.tiles) 
            DrawWireTile(position, Color.white);
        
        if (level.players.Count == 0)
            return;
        
        var player = level.players[playerIndex % level.players.Count];
        var enemies = level.players.Where(p => Rules.AreEnemies(player, p)).ToList();
        var units = level.units.Values.Where(u => u.player == player && u.position.v!=null).ToList();
        var enemyUnits = level.units.Values.Where(u => enemies.Contains(u.player) && u.position.v!=null).ToList();
        
        foreach (var unit in units)
            DrawSolidTile((Vector2Int)unit.position.v, Color.green);
        foreach (var unit in enemyUnits)
            DrawSolidTile((Vector2Int)unit.position.v, Color.red);
    }
}