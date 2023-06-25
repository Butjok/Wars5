using System.Collections.Generic;
using UnityEngine;

/*
 * TODO: This is actually not a correct implementation of vision. I actually should use something like Bresenham's line algorithm.
 */

public static class Vision {

    public const int infinity = 999;
    public const int mountainCapacityBonus = 2;
    public const int airbornePerTileCostBonus = 1;

    public static IEnumerable<Vector2Int> Calculate(Player player) {
        var visible = new HashSet<Vector2Int>();
        foreach (var unit in player.level.FindUnitsOf(player))
            visible.UnionWith(Calculate(unit));
        return visible;
    }

    public static IEnumerable<Vector2Int> Calculate(Unit unit) {
        var visionCapacity = Rules.VisionCapacity(unit);
        var isAirborne = Rules.IsAirborne(unit);
        if (!isAirborne && unit.Player.level.tiles[unit.NonNullPosition] == TileType.Mountain)
            visionCapacity += mountainCapacityBonus;
        return Calculate(unit.Player.level.tiles, unit.NonNullPosition, visionCapacity, isAirborne);
    }

    public static IEnumerable<Vector2Int> Calculate(Dictionary<Vector2Int, TileType> tiles, Vector2Int startPosition, int visionCapacity, bool isAirborne = false) {

        var visible = new HashSet<Vector2Int> { startPosition };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(startPosition);
        var explored = new HashSet<Vector2Int> { startPosition };
        var minimalTotalCost = new Dictionary<Vector2Int, int> { [startPosition] = 0 };
        int GetTotalCost(Vector2Int position) => minimalTotalCost.TryGetValue(position, out var c) ? c : infinity;

        while (queue.TryDequeue(out var position)) {

            if ((startPosition - position).ManhattanLength() > visionCapacity)
                break;

            var totalCost = GetTotalCost(position);
            if (totalCost > visionCapacity)
                continue;
            
            visible.Add(position);

            foreach (var neighbor in tiles.Neighbors(position)) {

                if (!explored.Contains(neighbor.position)) {
                    explored.Add(neighbor.position);
                    queue.Enqueue(neighbor.position);
                }

                var visionCost = Rules.VisionCost(neighbor.tileType);
                if (isAirborne)
                    visionCost = Mathf.Max(1, visionCost - airbornePerTileCostBonus);
                
                var alternativeNeighborTotalCost = totalCost + visionCost;
                
                // mountains should block the vision when viewed not from another mountain for non airborne units
                if (!isAirborne && tiles[position] != TileType.Mountain && neighbor.tileType == TileType.Mountain)
                    alternativeNeighborTotalCost = Mathf.Max(visionCapacity, alternativeNeighborTotalCost);
                
                minimalTotalCost[neighbor.position] = Mathf.Min(GetTotalCost(neighbor.position), alternativeNeighborTotalCost);   
            }
        }

        return visible;
    }
}