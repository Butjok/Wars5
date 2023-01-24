using System.Collections.Generic;
using System.Linq;
using FullscreenEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class Ai : MonoBehaviour {

    public const string a = "hello,l world!!!asdasdssdfsasdasdsdfasdasd";
    public int x = 3128;

    public static UnitAction FindBestAction(Main main) {

        var player = main.CurrentPlayer;
        var players = main.players;
        var tiles = main.tiles;
        var units = main.units.Values.ToList();
        var enemies = main.players.Where(p => Rules.AreEnemies(player, p)).ToList();
        var playerUnits = units.Where(u => u.Player == player).ToList();
        var unmovedPlayerUnits = playerUnits.Where(u => !u.Moved).ToList();
        var unmovedPlayerArtilleryUnits = unmovedPlayerUnits.Where(u => Rules.IsArtillery(u)).ToList();


        return null;
    }

    /*public static IEnumerable<Vector2Int> RangePositions(Map2D<TileType> tiles, Vector2Int position, Vector2Int range) {
        return range.Offsets()
            .Select(offset => position + offset)
            .Where(position => tiles.TryGetValue(position, out var tileType) && tileType != 0);
    }
    public static IEnumerable<Unit> RangeTargets(Map2D<TileType> tiles, Map2D<Unit> units, Vector2Int position, Vector2Int range) {
        foreach (var p in RangePositions(tiles, position, range))
            if (units.TryGetValue(p, out var unit) && unit != null)
                yield return unit;
    }*/

    public Main main;
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

        if (!main) {
            main = FindObjectOfType<Main>();
            if (!main)
                return;
        }

        foreach (var (position, tileType) in main.tiles)
            DrawWireTile(position, Color.white);

        if (main.players.Count == 0)
            return;

        var player = main.players[playerIndex % main.players.Count];
        var enemies = main.players.Where(p => Rules.AreEnemies(player, p)).ToList();
        var units = main.units.Values.Where(u => u.Player == player && u.Position != null).ToList();
        var enemyUnits = main.units.Values.Where(u => enemies.Contains(u.Player) && u.Position != null).ToList();

        foreach (var unit in units)
            DrawSolidTile((Vector2Int)unit.Position, Color.green);
        foreach (var unit in enemyUnits)
            DrawSolidTile((Vector2Int)unit.Position, Color.red);
    }
}