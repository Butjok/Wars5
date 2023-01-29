using System.Collections.Generic;
using Butjok.CommandLine;
using Drawing;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

public class Traverser {

    public const int infinity = 999;

    [Command]
    public static bool debugShowVisited = false;
    [Command]
    public static float debugDuration = 5;
    [Command]
    public static Color debugColor = Color.yellow;
    [Command]
    public static float debugThickness = 2;

    public delegate bool TryGetCostDelegate(Vector2Int position, int distance, out int cost);

    private Dictionary<Vector2Int, (int distance, Vector2Int? cameFrom)> infos = new();
    private SimplePriorityQueue<Vector2Int> queue = new();
    private List<Vector2Int> reachable = new();
    public IReadOnlyList<Vector2Int> Reachable => reachable;

    public void Traverse(IEnumerable<Vector2Int> positions, Vector2Int start, TryGetCostDelegate tryGetCost, Vector2Int? goal = null) {

        int DistanceToGoal(Vector2Int position) {
            return goal is { } actualValue ? (actualValue - position).ManhattanLength() : 0;
        }

        infos.Clear();
        queue.Clear();
        reachable.Clear();

        foreach (var position in positions) {
            var distance = (position == start ? 0 : infinity);
            infos[position] = (distance, null);
            queue.Enqueue(position, distance + DistanceToGoal(position));
        }

        while (queue.Count > 0) {

            var position = queue.Dequeue();
            var distance = infos[position].distance;

            if (distance >= infinity)
                break;

            reachable.Add(position);

            if (debugShowVisited)
                using (Draw.ingame.WithDuration(debugDuration))
                using (Draw.ingame.WithLineWidth(debugThickness))
                using (Draw.ingame.WithColor(debugColor)) {
                    Draw.ingame.CircleXZ((Vector3)position.ToVector3Int(), .4f);
                    Draw.ingame.Label2D((Vector3)position.ToVector3Int(), distance.ToString());
                }

            if (goal is { } actualValue && actualValue == position)
                break;

            foreach (var offset in Rules.offsets) {

                var neighborPosition = position + offset;
                if (!infos.TryGetValue(neighborPosition, out var neighborInfo) ||
                    !queue.Contains(neighborPosition) ||
                    !tryGetCost(neighborPosition, distance, out var cost))
                    continue;

                var alternativeDistance = distance + cost;
                if (alternativeDistance < neighborInfo.distance) {
                    infos[neighborPosition] = (alternativeDistance, position);
                    queue.UpdatePriority(neighborPosition, alternativeDistance + DistanceToGoal(neighborPosition));
                }
            }
        }
    }

    public bool TryReconstructPath(Vector2Int target, List<Vector2Int> path) {

        if (!infos.TryGetValue(target, out var info) || info.distance >= infinity)
            return false;

        path.Clear();
        for (Vector2Int? position = target; position != null; position = infos[(Vector2Int)position].cameFrom)
            path.Add((Vector2Int)position);
        path.Reverse();
        return true;
    }

    public bool TryFindPath(Unit unit, Vector2Int goal, List<Vector2Int>path) {
        if (unit.Position is not { } position)
            throw new AssertionException("unit.Position == null", null);
        Traverse(unit.Player.main.tiles.Keys, position, Rules.GetMoveCostFunction(unit, false), goal);
        return TryReconstructPath(goal, path);
    }

    public bool TryGetDistance(Vector2Int position, out int distance) {
        distance = infinity;
        if (!infos.TryGetValue(position, out var info) || info.distance >= infinity)
            return false;
        distance = info.distance;
        return true;
    }
    public bool IsReachable(Vector2Int position, int maxDistance) {
        return TryGetDistance(position, out var distance) && distance <= maxDistance;
    }
}