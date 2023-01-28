using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

public class Traverser {

    public delegate bool TryGetCostDelegate(Vector2Int position, int distance, out int cost);
    
    private Dictionary<Vector2Int, (int distance, Vector2Int? cameFrom)> infos = new();
    private SimplePriorityQueue<Vector2Int> queue = new();

    public void Traverse(IEnumerable<Vector2Int> positions, Vector2Int start, TryGetCostDelegate tryGetCost, int maxDistance) {

        infos.Clear();
        queue.Clear();

        foreach (var position in positions) {
            var distance = position == start ? 0 : int.MaxValue;
            infos[position] = (distance, null);
            queue.Enqueue(position, distance);
        }

        while (queue.Count > 0) {

            var position = queue.Dequeue();
            var distance = infos[position].distance;
            if (distance > maxDistance)
                break;

            foreach (var offset in Rules.offsets) {

                var neighbor = position + offset;
                if (!infos.TryGetValue(neighbor, out var neighborInfo) ||
                    !queue.Contains(neighbor) ||
                    !tryGetCost(neighbor, distance, out var cost))
                    continue;

                var alternativeDistance = distance + cost;
                if (alternativeDistance < neighborInfo.distance) {
                    infos[neighbor] = (alternativeDistance, position);
                    queue.UpdatePriority(neighbor, alternativeDistance);
                }
            }
        }
    }

    public List<Vector2Int> ReconstructPath(Vector2Int target) {

        if (!infos.TryGetValue(target, out var info) || info.distance == int.MaxValue)
            return null;

        var result = new List<Vector2Int>();
        for (Vector2Int? position = target; position != null; position = infos[(Vector2Int)position].cameFrom)
            result.Add((Vector2Int)position);
        result.Reverse();
        return result;
    }

    public bool TryGetDistance(Vector2Int position, out int distance) {
        distance = int.MaxValue;
        if (!infos.TryGetValue(position, out var info) || info.distance == int.MaxValue)
            return false;
        distance = info.distance;
        return true;
    }
    public bool IsReachable(Vector2Int position, int maxDistance) {
        return TryGetDistance(position, out var distance) && distance <= maxDistance;
    }
}