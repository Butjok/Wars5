using System.Collections.Generic;
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
        public int g, h;
        public Vector2Int? shortCameFrom, longCameFrom;
    }
    
    public enum ShortPathDestinationsAreValidTo {
        Stay,
        MoveThrough
    }
    public enum RestPathMovesThrough {
        FriendlyUnitsOnly,
        AllUnits
    }

    public const int infinity = 99999;
    
    public Unit unit;
    public readonly Dictionary<Vector2Int, Tile> tiles = new();
    public readonly SimplePriorityQueue<Vector2Int> queue = new();
    public readonly HashSet<Vector2Int> shortPathDestinations = new();
    public readonly HashSet<Vector2Int> goals = new();
    public RestPathMovesThrough restPathMovesThrough;
    public List<Vector2Int> dequeued = new();

    public PathFinder() { }
    public PathFinder(Unit unit, ShortPathDestinationsAreValidTo shortPathDestinationsAreValidTo = ShortPathDestinationsAreValidTo.Stay, RestPathMovesThrough restPathMovesThrough = RestPathMovesThrough.AllUnits) {
        FindShortPaths(unit, shortPathDestinationsAreValidTo, restPathMovesThrough);
    }

    public void FindShortPaths(Unit unit, ShortPathDestinationsAreValidTo shortPathDestinationsAreValidTo = ShortPathDestinationsAreValidTo.Stay, RestPathMovesThrough restPathMovesThrough = RestPathMovesThrough.AllUnits) {
        this.unit = unit;
        this.restPathMovesThrough = restPathMovesThrough;

        var startPosition = unit.NonNullPosition;
        var moveCapacity = Rules.MoveCapacity(unit);

        tiles.Clear();
        queue.Clear();
        shortPathDestinations.Clear();
        dequeued.Clear();

        var level = unit.Player.level;
        foreach (var position in level.tiles.Keys)
            if (Rules.TryGetMoveCost(unit, position, out _)) {
                var tile = new Tile { g = position == startPosition ? 0 : infinity };
                tiles.Add(position, tile);
                queue.Enqueue(position, tile.g);
            }

        while (TryDequeue(out var position) && tiles.TryGetValue(position, out var current) && current.g <= moveCapacity) {
            if (shortPathDestinationsAreValidTo == ShortPathDestinationsAreValidTo.Stay && Rules.CanStay(unit, position) ||
                shortPathDestinationsAreValidTo == ShortPathDestinationsAreValidTo.MoveThrough && Rules.CanMoveThrough(unit, position))
                shortPathDestinations.Add(position);

            foreach (var offset in Rules.gridOffsets) {
                var neighborPosition = position + offset;
                if (tiles.TryGetValue(neighborPosition, out var neighbor) &&
                    Rules.TryGetMoveCost(unit, level.tiles[neighborPosition], out var cost) &&
                    Rules.CanMoveThrough(unit, position)) {
                    var relaxedG = current.g + cost;
                    if (relaxedG < neighbor.g && relaxedG <= moveCapacity) {
                        neighbor.g = relaxedG;
                        neighbor.shortCameFrom = position;
                        tiles[neighborPosition] = neighbor;
                        queue.UpdatePriority(neighborPosition, neighbor.g);
                    }
                }
            }
        }

        Assert.AreNotEqual(0, shortPathDestinations.Count);

        foreach (var position in dequeued)
            queue.Enqueue(position, 0);

        foreach (var position in queue) {
            var tile = tiles[position];
            if (!shortPathDestinations.Contains(position)) {
                tile.g = infinity;
                tiles[position] = tile;
            }
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

        goals.IntersectWith(tiles.Keys);
        if (goals.Count == 0)
            return false;

        var minG = infinity;
        var allGoalsAreClosed = true;
        foreach (var position in goals) {
            var g = tiles[position].g;
            if (g < minG) {
                minG = g;
                goal = position;
            }
            if (allGoalsAreClosed && queue.Contains(position))
                allGoalsAreClosed = false;
        }

        if (!allGoalsAreClosed) {
            foreach (var position in queue) {
                var node = tiles[position];
                node.h = infinity;
                foreach (var g in goals)
                    node.h = Mathf.Min(node.h, (g - position).ManhattanLength());
                tiles[position] = node;
                queue.UpdatePriority(position, node.g + node.h);
            }

            while (queue.TryDequeue(out var position) && tiles.TryGetValue(position, out var current) && current.g < infinity) {
                foreach (var offset in Rules.gridOffsets) {
                    var neighborPosition = position + offset;
                    if (tiles.TryGetValue(neighborPosition, out var neighbor) &&
                        Rules.TryGetMoveCost(unit, unit.Player.level.tiles[neighborPosition], out var cost) &&
                        (restPathMovesThrough == RestPathMovesThrough.AllUnits || Rules.CanMoveThrough(unit, position))) {
                        var relaxedG = current.g + cost;
                        if (relaxedG < neighbor.g) {
                            neighbor.g = relaxedG;
                            neighbor.longCameFrom = position;
                            tiles[neighborPosition] = neighbor;
                            queue.UpdatePriority(neighborPosition, neighbor.g + neighbor.h);
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

        if (minG >= infinity)
            return false;

        restPath = new List<Vector2Int>();
        for (Vector2Int? item = goal; item is { } position; item = tiles[position].longCameFrom)
            restPath.Add(position);
        restPath.Reverse();

        shortPath = new List<Vector2Int>();
        for (Vector2Int? item = restPath[0]; item is { } position; item = tiles[position].shortCameFrom)
            shortPath.Add(position);
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
}