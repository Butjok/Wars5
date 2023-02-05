using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

public static class PathFinding {

    public struct Node {
        public TileType tileType;
        public int g; // shortest path cost
        public int h; // heuristic for A* = minimal distance to any of the target positions
        public Vector2Int? cameFrom; // where did we come from to this position?
        public int F => g + h; // total heuristic cost used in A*
    }

    public const int infinity = 9999; // concerned of a potential integer overflow if use int.MaxValue
    public static readonly IReadOnlyList<Vector2Int> offsets = new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    private static readonly Dictionary<Vector2Int, Node> moveNodes = new();
    private static readonly SimplePriorityQueue<Vector2Int> priorityQueue = new();
    private static readonly HashSet<Vector2Int> destinations = new();
    private static readonly Dictionary<Vector2Int, Node> pathNodes = new();
    private static readonly HashSet<Vector2Int> pathClosed = new();
    private static readonly List<Vector2Int> movePath = new();
    private static readonly List<Vector2Int> restPath = new();

    public static List<Vector2Int> Path => movePath.ToList();
    public static List<Vector2Int> RestPath => restPath.ToList();

    [Command] public static Vector2Int goal = new(-5, -17);
    [Command] public static bool TrySetGoal() {
        if (!Mouse.TryGetPosition(out Vector2Int position))
            return false;
        goal = position;
        return true;
    }
    [Command]
    public static void FindMove(bool onlyValidStayPositions = true) {

        var main = Object.FindObjectOfType<Main>();
        Assert.IsTrue(main);

        if (!Mouse.TryGetPosition(out Vector2Int position) || !main.TryGetUnit(position, out var unit))
            return;

        main.StopAllCoroutines();
        if (TryFindMove(unit, goal, onlyValidStayPositions))
            main.StartCoroutine(DrawDebug());
    }
    
    /// <summary>
    /// traverses the map with 2 consecutive pathfinding algos:
    /// <list type="number">
    /// <item>traverses from the unit position using Dijkstra - traverses only positions which are reachable in one move: it remembers these destination positions</item>
    /// <item>traverses from the goal with A*, finding for the first shortest path up to any of the destination positions.
    ///    uses a heuristic = minimum distance to any of the destination points</item>
    /// </list>
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="goal"></param>
    /// <param name="onlyValidStayPositions"></param>
    /// <returns></returns>
    public static bool TryFindMove(Unit unit, Vector2Int goal, bool onlyValidStayPositions = true) {

        moveNodes.Clear();
        priorityQueue.Clear();

        var main = unit.Player.main;
        foreach (var (position, tileType) in main.tiles) {
            var node = new Node {
                tileType = tileType,
                g = position == unit.NonNullPosition ? 0 : infinity
            };
            moveNodes.Add(position, node);
            priorityQueue.Enqueue(position, node.F);
        }

        var maxCost = Rules.MoveDistance(unit);

        while (priorityQueue.TryDequeue(out var position)) {
            var current = moveNodes[position];
            if (current.g > maxCost)
                break;

            foreach (var offset in offsets) {
                var neighborPosition = position + offset;
                if (moveNodes.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, neighbor.tileType, out var cost) &&
                    Rules.CanPass(unit,neighborPosition) &&
                    priorityQueue.Contains(neighborPosition)) {

                    var alternativeG = current.g + cost;
                    if (alternativeG < neighbor.g) {
                        neighbor.g = alternativeG;
                        neighbor.cameFrom = position;
                    }

                    moveNodes[neighborPosition] = neighbor;
                    priorityQueue.UpdatePriority(neighborPosition, neighbor.F);
                }
            }
        }

        destinations.Clear();
        foreach (var (position, node) in moveNodes)
            if (node.g <= maxCost && (!onlyValidStayPositions || Rules.CanStay(unit, position)))
                destinations.Add(position);
        Assert.IsTrue(destinations.Count > 0);

        pathNodes.Clear();
        priorityQueue.Clear();

        foreach (var (position, tileType) in main.tiles) {
            var node = new Node {
                tileType = tileType,
                g = position == goal ? 0 : infinity,
                h = infinity
            };
            foreach (var stayPosition in destinations)
                node.h = Mathf.Min(node.h, (stayPosition - position).ManhattanLength());
            pathNodes.Add(position, node);
            priorityQueue.Enqueue(position, node.F);
        }

        Vector2Int? destinationOption = null;

        pathClosed.Clear();
        while (priorityQueue.TryDequeue(out var position)) {
            var current = pathNodes[position];

            if (current.g >= infinity)
                break;

            pathClosed.Add(position);

            if (destinations.Contains(position)) {
                destinationOption = position;
                break;
            }

            foreach (var offset in offsets) {
                var neighborPosition = position + offset;
                if (pathNodes.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, neighbor.tileType, out var cost) &&
                    Rules.CanPass(unit,neighborPosition) &&
                    priorityQueue.Contains(neighborPosition)) {

                    var alternativeG = current.g + cost;
                    if (alternativeG < neighbor.g) {
                        neighbor.g = alternativeG;
                        neighbor.cameFrom = position;
                    }

                    pathNodes[neighborPosition] = neighbor;
                    priorityQueue.UpdatePriority(neighborPosition, neighbor.F);
                }
            }
        }

        if (destinationOption is not { } destination)
            return false;

        //
        // reconstruct move up to stay position
        //

        movePath.Clear();
        for (var position = destination; moveNodes.TryGetValue(position, out var node);) {
            movePath.Add(position);
            if (node.cameFrom is { } previousPosition)
                position = previousPosition;
            else
                break;
        }
        movePath.Reverse();

        //
        // reconstruct rest path
        // 

        restPath.Clear();
        for (var position = destination; pathNodes.TryGetValue(position, out var node);) {
            restPath.Add(position);
            if (node.cameFrom is { } previousPosition)
                position = previousPosition;
            else
                break;
        }

        return true;
    }

    public static IEnumerator DrawDebug() {

        while (!Input.GetKeyDown(KeyCode.Alpha0)) {
            yield return null;

            foreach (var position in destinations) {
                Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, Color.yellow);
                Draw.ingame.Label2D((Vector3)position.ToVector3Int(), moveNodes[position].g.ToString(), 14, LabelAlignment.Center, Color.black);
            }
        }
        yield return null;

        while (!Input.GetKeyDown(KeyCode.Alpha0)) {
            yield return null;

            foreach (var position in pathClosed) {
                Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, Color.yellow);
                Draw.ingame.Label2D((Vector3)position.ToVector3Int(), pathNodes[position].g.ToString(), 14, LabelAlignment.Center, Color.black);
            }
        }
        yield return null;

        while (!Input.GetKeyDown(KeyCode.Alpha0)) {
            yield return null;

            using (Draw.ingame.WithLineWidth(2)) {
                for (var i = 1; i < movePath.Count; i++) {
                    Vector3 from = movePath[i - 1].ToVector3Int();
                    Vector3 to = movePath[i].ToVector3Int();
                    Draw.ingame.Arrow(from, to, Color.blue);
                }
                for (var i = 1; i < restPath.Count; i++) {
                    Vector3 from = restPath[i - 1].ToVector3Int();
                    Vector3 to = restPath[i].ToVector3Int();
                    Draw.ingame.Arrow(from, to, Color.yellow);
                }
            }
        }
        yield return null;
    }
}