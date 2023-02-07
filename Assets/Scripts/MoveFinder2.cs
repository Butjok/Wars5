using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

public static class MoveFinder2 {

    private static readonly HashSet<Vector2Int> goals = new();
    public static IEnumerable<Vector2Int> Goals {
        set {
            goals.Clear();
            foreach (var position in value)
                goals.Add(position);
            h.Clear();
        }
    }
    public static Vector2Int Goal {
        set {
            goals.Clear();
            goals.Add(value);
            h.Clear();
        }
    }
    [Command]
    public static bool TrySetGoal() {
        if (!Mouse.TryGetPosition(out Vector2Int mousePosition))
            return false;
        Goal = mousePosition;
        return true;
    }

    public static readonly Dictionary<Vector2Int, int> h = new();
    public static int H(Vector2Int position) {
        if (!h.TryGetValue(position, out var value)) {
            Assert.AreNotEqual(0, goals.Count);
            value = goals.Min(goal => (goal - position).ManhattanLength());
            h.Add(position, value);
        }
        return value;
    }

    public struct Node {
        public TileType tileType;
        public int g;
        public Vector2Int? cameFrom;
    }

    public static Unit unit;
    public static readonly Dictionary<Vector2Int, Node> nodes = new();
    public static readonly SimplePriorityQueue<Vector2Int> priorityQueue = new();
    public static readonly HashSet<Vector2Int> destinations = new();
    public static readonly HashSet<Vector2Int> closed = new();

    public const int infinity = 9999;
    public static readonly IReadOnlyList<Vector2Int> offsets = new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    [Command]
    public static void FindDestinations() {
        if (!Main.TryFind(out var main) || !Mouse.TryGetPosition(out Vector2Int mousePosition) || !main.TryGetUnit(mousePosition, out var unit))
            return;
        FindDestinations(unit);

        using (Draw.ingame.WithDuration(5))
            foreach (var position in destinations)
                Draw.ingame.SolidCircleXZ((Vector3)position.ToVector3Int(), .5f, Color.cyan);
    }

    public static void FindDestinations(Unit unit, bool onlyStayPositions = true) {

        MoveFinder2.unit = unit;
        var startPosition = unit.NonNullPosition;
        var moveCapacity = Rules.MoveCapacity(unit);

        nodes.Clear();
        priorityQueue.Clear();
        destinations.Clear();
        closed.Clear();

        foreach (var (position, tileType) in unit.Player.main.tiles) {
            var node = new Node {
                tileType = tileType,
                g = position == startPosition ? 0 : infinity
            };
            nodes.Add(position, node);
            priorityQueue.Enqueue(position, node.g);
        }

        while (priorityQueue.TryDequeue(out var position) && nodes.TryGetValue(position, out var current) && current.g < infinity) {

            closed.Add(position);

            if (!onlyStayPositions || Rules.CanStay(unit, position))
                destinations.Add(position);

            foreach (var offset in offsets) {
                var neighborPosition = position + offset;

                if (nodes.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, neighbor.tileType, out var cost) &&
                    Rules.CanPass(unit, neighborPosition) &&
                    priorityQueue.Contains(neighborPosition)) {

                    var alternativeG = current.g + cost;
                    if (alternativeG < neighbor.g) {

                        neighbor.g = alternativeG;
                        neighbor.cameFrom = position;
                        nodes[neighborPosition] = neighbor;

                        priorityQueue.UpdatePriority(neighborPosition, neighbor.g);
                    }
                }
            }

            // stop Dijkstra after relaxing neighbour tiles
            if (current.g > moveCapacity)
                break;
        }

        Assert.AreNotEqual(0, destinations.Count);
    }

    [Command]
    public static void TryFindPath() {
        TryFindPath(out _, out _);
        using (Draw.ingame.WithDuration(5))
            foreach (var position in closed)
                Draw.ingame.SolidCircleXZ((Vector3)position.ToVector3Int(), .5f, Color.yellow);
    }

    public static bool TryFindPath(out Vector2Int goal, out Vector2Int destination) {

        goal = destination = default;

        if (goals.Count == 0)
            return false;

        var minG = infinity;
        var allGoalsAreClosed = true;
        foreach (var position in goals) {
            if (nodes[position].g < minG) {
                minG = nodes[position].g;
                goal = position;
            }
            if (allGoalsAreClosed && priorityQueue.Contains(position))
                allGoalsAreClosed = false;
        }
        if (allGoalsAreClosed && minG >= infinity)
            return false;

        foreach (var position in priorityQueue)
            priorityQueue.UpdatePriority(position, nodes[position].g + H(position));

        while (priorityQueue.TryDequeue(out var position) && nodes.TryGetValue(position, out var current) && current.g < infinity) {

            closed.Add(position);

            foreach (var offset in offsets) {
                var neighborPosition = position + offset;

                if (nodes.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, neighbor.tileType, out var cost) &&
                    Rules.CanPass(unit, neighborPosition) &&
                    priorityQueue.Contains(neighborPosition)) {

                    var alternativeG = current.g + cost;
                    if (alternativeG < neighbor.g) {

                        neighbor.g = alternativeG;
                        neighbor.cameFrom = position;
                        nodes[neighborPosition] = neighbor;

                        priorityQueue.UpdatePriority(neighborPosition, neighbor.g + H(neighborPosition));
                    }
                }
            }

            if (goals.Contains(position)) {
                if (current.g < minG) {
                    minG = current.g;
                    goal = position;
                }
                break;
            }
        }

        if (minG >= infinity)
            return false;

        for (Vector2Int? i = goal; i is { } value; i = nodes[value].cameFrom)
            if (destinations.Contains(value)) {
                destination = value;
                return true;
            }
        throw new AssertionException("no valid destination was found", null);
    }

    public static List<Vector2Int> ReconstructPath(Vector2Int position) {
        var path = new List<Vector2Int>();
        for (Vector2Int? i = position; i is { } value; i = nodes[value].cameFrom)
            path.Add(value);
        path.Reverse();
        return path;
    }
}