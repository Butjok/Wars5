using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

#pragma warning disable 8509

public static class MoveFinder2 {

    private static readonly HashSet<Vector2Int> goals = new();
    public static IEnumerable<Vector2Int> Goals {
        set {
            goals.Clear();
            foreach (var position in value)
                goals.Add(position);
        }
    }
    public static Vector2Int Goal {
        set {
            goals.Clear();
            goals.Add(value);
        }
    }

    [Command]
    public static bool TrySetGoal() {
        if (!Mouse.TryGetPosition(out Vector2Int mousePosition))
            return false;
        Goal = mousePosition;
        return true;
    }

    public struct Node {
        public int shortG, longG;
        public Vector2Int? shortCameFrom, longCameFrom;
        public int heuristic;
    }

    public static Unit unit;
    public static readonly Dictionary<Vector2Int, Node> nodes = new();
    public static readonly SimplePriorityQueue<Vector2Int> priorityQueue = new();
    public static readonly HashSet<Vector2Int> destinations = new();

    public const int infinity = 99999;

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

        var tiles = unit.Player.main.tiles;
        foreach (var position in tiles.Keys) {
            var node = new Node {
                shortG = position == startPosition ? 0 : infinity,
                longG = infinity
            };
            nodes.Add(position, node);
            if ((position - startPosition).ManhattanLength() <= moveCapacity)
                priorityQueue.Enqueue(position, node.shortG);
        }

        while (priorityQueue.TryDequeue(out var position) && nodes.TryGetValue(position, out var current) && current.shortG <= moveCapacity) {

            if (!onlyStayPositions || Rules.CanStay(unit, position))
                destinations.Add(position);

            for (var i = 0; i < 4; i++) {

                var neighborPosition = position + i switch {
                    0 => Vector2Int.right,
                    1 => Vector2Int.left,
                    2 => Vector2Int.up,
                    3 => Vector2Int.down
                };

                if ((neighborPosition - startPosition).ManhattanLength() <= moveCapacity &&
                    nodes.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, tiles[neighborPosition], out var cost) &&
                    Rules.CanPass(unit, neighborPosition)) {

                    var alternativeG = current.shortG + cost;
                    if (alternativeG < neighbor.shortG) {

                        neighbor.shortG = alternativeG;
                        neighbor.shortCameFrom = position;
                        nodes[neighborPosition] = neighbor;

                        priorityQueue.UpdatePriority(neighborPosition, neighbor.shortG);
                    }
                }
            }
        }

        Assert.AreNotEqual(0, destinations.Count);

        priorityQueue.Clear();
        foreach (var position in tiles.Keys) {
            var node = nodes[position];
            if (destinations.Contains(position)) {
                node.longG = 0;
                nodes[position] = node;
            }
            priorityQueue.Enqueue(position, node.longG);
        }
    }


    public static bool TryFindPath(out List<Vector2Int> shortPath, out List<Vector2Int> restPath) {

        shortPath = restPath = null;
        Vector2Int goal = default;

        if (goals.Count == 0)
            return false;

        var minG = infinity;
        var allGoalsAreClosed = true;
        foreach (var position in goals) {
            var longG = nodes[position].longG;
            if (longG < minG) {
                minG = longG;
                goal = position;
            }
            if (allGoalsAreClosed && priorityQueue.Contains(position))
                allGoalsAreClosed = false;
        }
        if (allGoalsAreClosed && minG >= infinity)
            return false;

        foreach (var position in priorityQueue) {
            var node = nodes[position];
            node.heuristic = infinity;
            foreach (var g in goals)
                node.heuristic = Mathf.Min(node.heuristic, (g - position).ManhattanLength());
            nodes[position] = node;
            priorityQueue.UpdatePriority(position, nodes[position].longG + nodes[position].heuristic);
        }

        while (priorityQueue.TryDequeue(out var position) && nodes.TryGetValue(position, out var current) && current.longG < infinity) {

            for (var i = 0; i < 4; i++) {

                var neighborPosition = position + i switch {
                    0 => Vector2Int.right,
                    1 => Vector2Int.left,
                    2 => Vector2Int.up,
                    3 => Vector2Int.down
                };

                // TODO: possible optimization here: remove unpassable tiles beforehand
                if (nodes.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, unit.Player.main.tiles[neighborPosition], out var cost) &&
                    Rules.CanPass(unit, neighborPosition)) {

                    var alternativeG = current.longG + cost;
                    if (alternativeG < neighbor.longG) {

                        neighbor.longG = alternativeG;
                        neighbor.longCameFrom = position;
                        nodes[neighborPosition] = neighbor;

                        priorityQueue.UpdatePriority(neighborPosition, neighbor.longG + neighbor.heuristic);
                    }
                }
            }

            if (goals.Contains(position)) {
                if (current.longG < minG) {
                    minG = current.longG;
                    goal = position;
                }
                break;
            }
        }

        if (minG >= infinity)
            return false;

        restPath = new List<Vector2Int>();
        for (Vector2Int? item = goal; item is { } position; item = nodes[position].longCameFrom)
            restPath.Add(position);
        restPath.Reverse();

        shortPath = new List<Vector2Int>();
        for (Vector2Int? item = restPath[0]; item is { } position; item = nodes[position].shortCameFrom)
            shortPath.Add(position);
        shortPath.Reverse();

        return true;
    }
}