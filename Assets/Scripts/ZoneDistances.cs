using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UnityEngine;
using static PathFinder;

public static class ZoneDistances {

    public static Dictionary<Vector2Int, int> Calculate(Dictionary<Vector2Int,TileType> tiles, Zone zone,MoveType moveType) {
        var result = new Dictionary<Vector2Int, int>();
        var queue = new SimplePriorityQueue<Vector2Int, int>();
        foreach (var position in tiles.Keys)
            queue.Enqueue(position, zone.tiles.Contains(position) ? 0 : infinity);
        while (queue.TryFirst(out var position)) {
            var distance = queue.GetPriority(position);
            queue.Dequeue();
            result.Add(position, distance);
            foreach (var neighbor in position.GrownBy(1).Intersect(tiles.Keys).Where(queue.Contains)) {
                if (Rules.TryGetMoveCost(moveType, tiles[neighbor], out var cost)) {
                    var neighborDistance = queue.GetPriority(neighbor);
                    if (distance + cost < neighborDistance)
                        queue.UpdatePriority(neighbor, distance + cost);
                }
            }
        }
        return result;
    }
}