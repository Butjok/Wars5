using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class ScalarFieldUtilities {

    public const int maxRadius = 10;
    public static readonly List<List<Vector2Int>> areas = new();
    public const float infinity = 999999;

    static ScalarFieldUtilities() {
        for (var radius = 0; radius <= maxRadius; radius++) {
            var offsets = new List<Vector2Int>();
            areas.Add(offsets);
            for (var x = -radius; x <= radius; x++)
            for (var y = -radius; y <= radius; y++) {
                var offset = new Vector2Int(x, y);
                if (offset.ManhattanLength() <= radius)
                    offsets.Add(offset);
            }
        }
    }

    public static HashSet<Vector2Int> GrownBy(this IEnumerable<Vector2Int> positions, IReadOnlyCollection<Vector2Int> area) {
        var result = new HashSet<Vector2Int>();
        foreach (var position in positions)
        foreach (var offset in area)
            result.Add(position + offset);
        return result;
    }
    public static HashSet<Vector2Int> GrownBy(this IEnumerable<Vector2Int> positions, int radius) {
        Assert.IsTrue(radius is >= 0 and <= maxRadius);
        return positions.GrownBy(areas[radius]);
    }
    public static HashSet<Vector2Int> GrownBy(this Vector2Int position, IReadOnlyCollection<Vector2Int> area) {
        return new[] { position }.GrownBy(area);
    }
    public static HashSet<Vector2Int> GrownBy(this Vector2Int position, int radius) {
        return GrownBy(position, areas[radius]);
    }

    public static Func<T, bool> Negated<T>(this Func<T, bool> predicate) {
        return t => !predicate(t);
    }

    public static bool HaveSameDomain(ScalarField2 a, ScalarField2 b) {
        return !a.Domain.Except(b.Domain).Any() && !b.Domain.Except(a.Domain).Any();
    }
}