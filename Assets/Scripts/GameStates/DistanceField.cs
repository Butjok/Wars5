using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

public static class DistanceField {

    public const int infinity = 999999;
    public static readonly List<Vector2Int> squareOffsets = new();

    static DistanceField() {
        for (var xOffset = -1; xOffset <= 1; xOffset++)
        for (var yOffset = -1; yOffset <= 1; yOffset++) {
            var offset = new Vector2Int(xOffset, yOffset);
            if (offset != Vector2Int.zero)
                squareOffsets.Add(offset);
        }
    }

    public static HashSet<Vector2Int> FindObstacles(Level level, TileType isObstacle) {
        var result = new HashSet<Vector2Int>();
        foreach (var (position, tileType) in level.tiles)
            if (isObstacle.HasFlag(tileType))
                result.Add(position);
        return result;
    }

    /*
     * there are lots room for future performance improvement
     * if this is going to be used only to find out choke points
     * then we can keep the obstacle positions for each movement type
     * and add enemy unit positions to them without using FindObstacles()
     *
     * another improvement is to cap the dequeued node's distance quickly
     * if it gets larger than a desired choke length -> we can end the search early
     */

    public static Dictionary<Vector2Int, float> Calculate(HashSet<Vector2Int> seed, Level level) {
        var minDistance = new Dictionary<Vector2Int, float>();
        var priorityQueue = new SimplePriorityQueue<Vector2Int, float>();
        foreach (var position in level.tiles.Keys) {
            var distance = seed.Contains(position) ? 0 : infinity;
            minDistance.Add(position, distance);
            priorityQueue.Enqueue(position, distance);
        }

        while (priorityQueue.TryDequeue(out var position) && minDistance[position] < infinity)
            foreach (var offset in Level.offsets) {
                var neighborPosition = position + offset;
                if (!priorityQueue.Contains(neighborPosition))
                    continue;

                var relaxedDistance = Mathf.Min(minDistance[neighborPosition], minDistance[position] + 1);
                if (relaxedDistance < minDistance[neighborPosition]) {
                    minDistance[neighborPosition] = relaxedDistance;
                    priorityQueue.UpdatePriority(neighborPosition, relaxedDistance);
                }
            }

        foreach (var position in minDistance.Keys.ToList())
            if (!level.tiles.ContainsKey(position))
                minDistance.Remove(position);

        return minDistance;
    }

    public static Dictionary<Vector2Int, float> Map(this Dictionary<Vector2Int, float> a, Func<float, float> mapping) {
        var c = new Dictionary<Vector2Int, float>();
        foreach (var (position, aa) in a)
            c.Add(position, mapping(aa));
        return c;
    }
    public static Dictionary<Vector2Int, float> Map(this Dictionary<Vector2Int, float> a, Dictionary<Vector2Int, float> b, Func<float, float, float> mapping) {
        var c = new Dictionary<Vector2Int, float>();
        foreach (var (position, aa) in a) {
            Assert.IsTrue(b.TryGetValue(position, out var bb));
            c.Add(position, mapping(aa, bb));
        }
        return c;
    }

    public static float BaseUnitInfluence(UnitType unitType) {
        return unitType switch {
            UnitType.Infantry => .1f,
            UnitType.AntiTank => .15f,
            UnitType.Artillery => .5f,
            UnitType.Apc => .1f,
            UnitType.Recon => .25f,
            UnitType.LightTank => .5f,
            UnitType.Rockets => 1,
            UnitType.MediumTank => 1,
            UnitType.TransportHelicopter => .1f,
            UnitType.AttackHelicopter => .75f,
            UnitType.FighterJet => 1,
            UnitType.Bomber => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }

    private static readonly PathFinder pathFinder = new();
    public static Dictionary<Vector2Int, float> CalculateUnitInfluence(Unit unit) {
        var result = new Dictionary<Vector2Int, float>();
        if (!Rules.TryGetAttackRange(unit, out var attackRange))
            return result;
        pathFinder.FindShortPaths(unit, PathFinder.ShortPathDestinationsAreValidTo.Stay, PathFinder.RestPathMovesThrough.FriendlyUnitsOnly);
        if (Rules.IsIndirect(unit))
            foreach (var position in unit.Player.level.PositionsInRange(unit.NonNullPosition, attackRange))
                result.Add(position, BaseUnitInfluence(unit));
        else {
            foreach (var position in pathFinder.shortPathDestinations.GrownBy(1))
                result.Add(position, BaseUnitInfluence(unit));
        }
        return result;
    }
}