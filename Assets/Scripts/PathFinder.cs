using System;
using System.Collections;
using System.Collections.Generic;
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

public class PathFinder {

    public struct Tile {
        public int restPathCost, fullCost, h;
        public Vector2Int? shortPathBacktracePosition, restPathBacktracePosition;
    }

    public enum RestPathMovesThrough {
        FriendlyUnitsOnly,
        AllUnits
    }

    public const int infinity = 99999;

    public Unit unit;
    public readonly Dictionary<Vector2Int, Tile> tiles = new();
    public readonly SimplePriorityQueue<Vector2Int> queue = new();
    public readonly HashSet<Vector2Int> validShortPathDestinations = new();
    public readonly HashSet<Vector2Int> goals = new();
    public List<Vector2Int> dequeued = new();
    public RestPathMovesThrough restPathMovesThrough = RestPathMovesThrough.AllUnits;

    public PathFinder() { }
    public PathFinder(
        Unit unit,
        bool allowStayOnFriendlyUnits = false, HashSet<Vector2Int> allowedStayPositions = null,
        RestPathMovesThrough restPathMovesThrough = RestPathMovesThrough.AllUnits) {
        FindShortPaths(unit, allowStayOnFriendlyUnits, allowedStayPositions, restPathMovesThrough);
    }

    public void FindShortPaths(Unit unit, bool allowStayOnFriendlyUnits = false, HashSet<Vector2Int> allowedStayPositions = null, RestPathMovesThrough restPathMovesThrough = RestPathMovesThrough.AllUnits) {
        this.unit = unit;
        this.restPathMovesThrough = restPathMovesThrough;

        var startPosition = unit.NonNullPosition;
        var moveCapacity = Rules.MoveCapacity(unit);

        tiles.Clear();
        queue.Clear();
        validShortPathDestinations.Clear();
        dequeued.Clear();

        var level = unit.Player.level;
        foreach (var position in level.tiles.Keys)
            if (Rules.TryGetMoveCost(unit, position, out _)) {
                var tile = new Tile { fullCost = position == startPosition ? 0 : infinity };
                tiles.Add(position, tile);
                queue.Enqueue(position, tile.fullCost);
            }

        while (TryDequeue(out var position) && tiles.TryGetValue(position, out var current) && current.fullCost <= moveCapacity) {
            if (Rules.CanStay(unit, position) ||
                allowStayOnFriendlyUnits && Rules.CanMoveThrough(unit, position) ||
                allowedStayPositions != null && allowedStayPositions.Contains(position))
                validShortPathDestinations.Add(position);

            foreach (var offset in Rules.gridOffsets) {
                var neighborPosition = position + offset;
                if (tiles.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, level.tiles[neighborPosition], out var cost) &&
                    Rules.CanMoveThrough(unit, neighborPosition)) {
                    var relaxedFullCost = current.fullCost + cost;
                    if (relaxedFullCost < neighbor.fullCost && relaxedFullCost <= moveCapacity) {
                        neighbor.fullCost = relaxedFullCost;
                        neighbor.shortPathBacktracePosition = position;
                        tiles[neighborPosition] = neighbor;
                        queue.UpdatePriority(neighborPosition, neighbor.fullCost);
                    }
                }
            }
        }

        Assert.AreNotEqual(0, validShortPathDestinations.Count);

        foreach (var position in dequeued)
            queue.Enqueue(position, 0);

        foreach (var position in queue) {
            var tile = tiles[position];
            if (validShortPathDestinations.Contains(position)) {
                //tile.fullCost = tile.fullCost;
                tile.restPathCost = 0;
            }
            else {
                tile.fullCost = infinity;
                tile.restPathCost = infinity;
            }
            tiles[position] = tile;
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

        if (goals.Count == 0)
            return false;

        var minCost = infinity;
        var allGoalsAreClosed = true;
        foreach (var position in goals) {
            var tile = tiles[position];
            var g = tile.fullCost;
            if (g < minCost) {
                minCost = g;
                goal = position;
            }
            if (allGoalsAreClosed && queue.Contains(position))
                allGoalsAreClosed = false;
        }

        if (!allGoalsAreClosed) {
            // update heuristic values
            foreach (var position in queue) {
                var node = tiles[position];
                node.h = infinity;
                foreach (var g in goals)
                    node.h = Mathf.Min(node.h, (g - position).ManhattanLength());
                tiles[position] = node;
                queue.UpdatePriority(position, node.restPathCost + node.h);
            }

            while (queue.TryDequeue(out var position) && tiles.TryGetValue(position, out var current) && current.restPathCost < infinity) {
                // we can relax neighbor tiles only from:
                // 1) the destinations of the short path (it is like making a new move from those positions)
                // 2) from the positions of rest path
                // basically this disallows to relax tiles occupied by friendly units from tiles in a short path destinations

                if (current.shortPathBacktracePosition == null || validShortPathDestinations.Contains(position))
                    foreach (var offset in Rules.gridOffsets) {
                        var neighborPosition = position + offset;
                        if (tiles.TryGetValue(neighborPosition, out var neighbor) &&
                            Rules.TryGetMoveCost(unit, unit.Player.level.tiles[neighborPosition], out var cost) &&
                            (restPathMovesThrough == RestPathMovesThrough.AllUnits || Rules.CanMoveThrough(unit, neighborPosition))) {
                            var relaxedRestPathCost = current.restPathCost + cost;
                            if (relaxedRestPathCost < neighbor.restPathCost) {
                                neighbor.restPathCost = relaxedRestPathCost;
                                neighbor.fullCost = current.fullCost + cost;
                                neighbor.restPathBacktracePosition = position;
                                tiles[neighborPosition] = neighbor;
                                queue.UpdatePriority(neighborPosition, neighbor.restPathCost + neighbor.h);
                            }
                        }
                    }

                // we have already discovered a goal node before with a smaller distance
                // there is no point of discovering any other nodes since all of them will 
                // have greater distance
                if (current.fullCost >= minCost)
                    break;

                // if we have stumbled upon a goal node
                // the first one is guaranteed to be the closest of the remaining goals
                // just compare it with the already closed goals
                if (goals.Contains(position)) {
                    if (current.fullCost < minCost) {
                        minCost = current.fullCost;
                        goal = position;
                    }
                    break;
                }
            }
        }

        if (minCost >= infinity)
            return false;

        restPath = new List<Vector2Int>();
        for (Vector2Int? position = goal; position is { } actualPosition; position = tiles[actualPosition].restPathBacktracePosition)
            restPath.Add(actualPosition);
        restPath.Reverse();

        shortPath = new List<Vector2Int>();
        for (Vector2Int? position = restPath[0]; position is { } actualPosition; position = tiles[actualPosition].shortPathBacktracePosition)
            shortPath.Add(actualPosition);
        shortPath.Reverse();

        return true;
    }

    public bool TryDequeue(out Vector2Int position) {
        position = default;
        if (queue.Count == 0)
            return false;
        position = queue.Dequeue();
        dequeued.Add(position);
        return true;
    }
    public int FullCost(Vector2Int position) {
        return tiles[position].fullCost;
    }

    public static void Filter<T>(HashSet<T> hashSet, Func<T, bool> predicate) {
        hashSet.RemoveWhere(item => !predicate(item));
    }
}

public static class PathFinderExtensions {
    public static IEnumerator previousDrawingCoroutine;
    public static void DrawNodes(this PathFinder pathFinder, float duration = 10) {
        if (previousDrawingCoroutine != null) {
            Game.Instance.StopCoroutine(previousDrawingCoroutine);
            previousDrawingCoroutine = null;
        }
        previousDrawingCoroutine = DrawingCoroutine(pathFinder, duration);
        Game.Instance.StartCoroutine(previousDrawingCoroutine);
    }
    public static IEnumerator DrawingCoroutine(this PathFinder pathFinder, float duration) {
        for (var timeLeft = duration; timeLeft > 0; timeLeft -= Time.deltaTime) {
            using (Draw.ingame.WithLineWidth(1.5f))
                foreach (var position in pathFinder.tiles.Keys) {
                    Color color = Color.clear;
                    if (pathFinder.tiles[position].shortPathBacktracePosition != null)
                        color = Color.green;
                    else if (pathFinder.tiles[position].restPathBacktracePosition != null)
                        color = Color.yellow;
                    var position3d = position.Raycasted();
                    Draw.ingame.Label3D(position3d, Quaternion.Euler(90, 0, 0), $"{pathFinder.tiles[position].fullCost}", .2f, color);
                    var cameFrom = pathFinder.tiles[position].shortPathBacktracePosition ?? pathFinder.tiles[position].restPathBacktracePosition;
                    if (cameFrom is { } actualCameFrom)
                        Draw.ingame.Line(position3d, actualCameFrom.Raycasted(), color);
                }
            yield return null;
        }
    }
    public static void DrawPath(this List<Vector2Int> path, float duration = 5) {
        using (Draw.ingame.WithDuration(duration))
        using (Draw.ingame.WithLineWidth(1.5f))
            for (var i = 0; i < path.Count - 1; i++)
                Draw.ingame.Line(path[i].Raycasted(), path[i + 1].Raycasted(), Color.red);
    }
}