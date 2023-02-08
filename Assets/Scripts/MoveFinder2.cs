using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

#pragma warning disable 8509

/*
 * good article to keep in mind: http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
 * it is about:
 * - types of heuristics
 * - precomputing the heuristics
 * - breaking ties
 */

public static class MoveFinder2 {

    /*[Command]
    public static bool TrySetGoal(Vector2Int range) {
        if (!Mouse.TryGetPosition(out Vector2Int mousePosition))
            return false;
        Goals = Object.FindObjectOfType<Main>().PositionsInRange(mousePosition, range);
        return true;
    }*/

    public struct Node {
        public int g, h;
        public Vector2Int? shortCameFrom, longCameFrom;
    }

    public static Unit unit;
    public static readonly Dictionary<Vector2Int, Node> nodes = new();
    public static readonly SimplePriorityQueue<Vector2Int> priorityQueue = new();
    public static readonly HashSet<Vector2Int> destinations = new();
    private static readonly HashSet<Vector2Int> goals = new();

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
            if (!Rules.TryGetMoveCost(unit, position, out _) || !Rules.CanPass(unit, position))
                continue;
            var node = new Node { g = position == startPosition ? 0 : infinity };
            nodes.Add(position, node);
            if ((position - startPosition).ManhattanLength() <= moveCapacity)
                priorityQueue.Enqueue(position, node.g);
        }

        while (priorityQueue.TryDequeue(out var position) && nodes.TryGetValue(position, out var current) && current.g <= moveCapacity) {

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
                    Rules.TryGetMoveCost(unit, tiles[neighborPosition], out var cost)) {

                    var alternativeG = current.g + cost;
                    if (alternativeG < neighbor.g) {

                        neighbor.g = alternativeG;
                        neighbor.shortCameFrom = position;
                        nodes[neighborPosition] = neighbor;

                        priorityQueue.UpdatePriority(neighborPosition, neighbor.g);
                    }
                }
            }
        }

        Assert.AreNotEqual(0, destinations.Count);

        priorityQueue.Clear();
        foreach (var position in tiles.Keys) {
            if (!nodes.TryGetValue(position, out var node))
                continue;
            if (!destinations.Contains(position)) {
                node.g = infinity;
                nodes[position] = node;
            }
            priorityQueue.Enqueue(position, node.g);
        }

        closed.Clear();
    }

    public static readonly HashSet<Vector2Int> closed = new();

    public static bool TryFindPath(out List<Vector2Int> shortPath, out List<Vector2Int> restPath,
        Vector2Int? target = null, IEnumerable<Vector2Int> targets = null) {

        shortPath = restPath = null;
        Vector2Int goal = default;

        goals.Clear();
        if (target is { } value)
            goals.Add(value);
        if (targets != null)
            foreach (var position in targets)
                goals.Add(position);

        goals.RemoveWhere(position => !Rules.TryGetMoveCost(unit, position, out _) || !Rules.CanPass(unit, position));
        if (goals.Count == 0)
            return false;

        var minG = infinity;
        var allGoalsAreClosed = true;
        foreach (var position in goals) {
            var g = nodes[position].g;
            if (g < minG) {
                minG = g;
                goal = position;
            }
            if (allGoalsAreClosed && priorityQueue.Contains(position))
                allGoalsAreClosed = false;
        }

        if (!allGoalsAreClosed) {

            foreach (var position in priorityQueue) {
                var node = nodes[position];
                node.h = infinity;
                foreach (var g in goals)
                    node.h = Mathf.Min(node.h, (g - position).ManhattanLength());
                nodes[position] = node;
                priorityQueue.UpdatePriority(position, node.g + node.h);
            }

            while (priorityQueue.TryDequeue(out var position) && nodes.TryGetValue(position, out var current) && current.g < infinity) {

                closed.Add(position);

                for (var i = 0; i < 4; i++) {

                    var neighborPosition = position + i switch {
                        0 => Vector2Int.right,
                        1 => Vector2Int.left,
                        2 => Vector2Int.up,
                        3 => Vector2Int.down
                    };

                    if (nodes.TryGetValue(neighborPosition, out var neighbor) &&
                        Rules.TryGetMoveCost(unit, unit.Player.main.tiles[neighborPosition], out var cost)) {

                        var alternativeG = current.g + cost;
                        if (alternativeG < neighbor.g) {

                            neighbor.g = alternativeG;
                            neighbor.longCameFrom = position;
                            nodes[neighborPosition] = neighbor;

                            priorityQueue.UpdatePriority(neighborPosition, neighbor.g + neighbor.h);
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
        }

        if (drawClosed)
            using (Draw.ingame.WithDuration(5))
                foreach (var position in closed) {
                    Draw.ingame.SolidCircleXZ((Vector3)position.ToVector3Int(), .5f, Color.yellow);
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

    [Command] public static bool drawClosed;
}