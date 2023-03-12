using System;
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

public class MoveFinder2 {

    public struct Node {
        public int g, h;
        public Vector2Int? shortCameFrom, longCameFrom;
    }

    public Unit unit;
    public readonly Dictionary<Vector2Int, Node> nodes = new();
    public readonly SimplePriorityQueue<Vector2Int> priorityQueue = new();
    public readonly HashSet<Vector2Int> movePositions = new();
    private readonly HashSet<Vector2Int> goals = new();
    public readonly HashSet<Vector2Int> closed = new();

    public const int infinity = 99999;

    public void FindStayMoves(Unit unit) {
        FindMoves(unit, position => Rules.CanStay(unit, position));
    }
    public void FindMoves(Unit unit, Predicate<Vector2Int> filter = null) {

        this.unit = unit;
        var startPosition = unit.NonNullPosition;
        var moveCapacity = Rules.MoveCapacity(unit);

        nodes.Clear();
        priorityQueue.Clear();
        movePositions.Clear();
        closed.Clear();

        var tiles = unit.Player.level.tiles;
        foreach (var position in tiles.Keys) {
            if (!Rules.TryGetMoveCost(unit, position, out _) || !Rules.CanPass(unit, position))
                continue;
            var node = new Node { g = position == startPosition ? 0 : infinity };
            nodes.Add(position, node);
            if ((position - startPosition).ManhattanLength() <= moveCapacity)
                priorityQueue.Enqueue(position, node.g);
        }

        while (priorityQueue.TryDequeue(out var position) && nodes.TryGetValue(position, out var current) && current.g <= moveCapacity) {

            closed.Add(position);

            if (filter == null || filter(position))
                movePositions.Add(position);

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

        Assert.AreNotEqual(0, movePositions.Count);

        priorityQueue.Clear();
        foreach (var position in tiles.Keys) {
            if (!nodes.TryGetValue(position, out var node))
                continue;
            if (!movePositions.Contains(position)) {
                node.g = infinity;
                nodes[position] = node;
            }
            priorityQueue.Enqueue(position, node.g);
        }
    }

    public bool TryFindPath(out List<Vector2Int> shortPath, out List<Vector2Int> restPath,
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
                        Rules.TryGetMoveCost(unit, unit.Player.level.tiles[neighborPosition], out var cost)) {

                        var alternativeG = current.g + cost;
                        if (alternativeG < neighbor.g) {

                            neighbor.g = alternativeG;
                            neighbor.longCameFrom = position;
                            nodes[neighborPosition] = neighbor;

                            priorityQueue.UpdatePriority(neighborPosition, neighbor.g + neighbor.h);
                        }
                    }
                }

                // we have already discovered a goal node before with a smaller distance
                // there is no point of discovering any other nodes since all of them will 
                // have greater distance
                if (current.g >= minG)
                    break;

                // if we have stumbled upon a goal node
                // the first one is guaranteed to be the closest of the remaining goals
                // just compare it with the already closed goals
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