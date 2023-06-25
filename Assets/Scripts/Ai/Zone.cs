using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Zone {

    public string name;
    public HashSet<Vector2Int> tiles = new();
    public int Area => tiles.Count;
    public HashSet<Zone> neighbors = new();
    public Dictionary<(MoveType moveType, Vector2Int position), int> distances=new();

    public static HashSet<Zone> GetConnected(Zone seed) {
        Assert.IsNotNull(seed);
        var closed = new HashSet<Zone> { seed };
        void AddNeighbors(Zone zone) {
            foreach (var neighbor in zone.neighbors.Where(neighbor => !closed.Contains(neighbor))) {
                closed.Add(neighbor);
                AddNeighbors(neighbor);
            }
        }
        AddNeighbors(seed);
        return closed;
    }
    public override string ToString() {
        return $"{name} ({Area})";
    }
}

public static class ZoneExtensions {
    public static Vector3 GetCenter(this Zone zone) {
        return (zone.tiles.Aggregate(Vector2.zero, (sum, position) => sum + position) / zone.tiles.Count).ToVector3();
    }
}