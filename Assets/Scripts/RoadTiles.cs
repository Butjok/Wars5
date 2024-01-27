using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RoadTiles {
    
    public enum Type { I, L, T, X, Isolated, Cap }
    
    public static readonly List<Vector2Int> neighbors = new();
    public static readonly Vector2Int[] offsets = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    
    public static (Type type, Vector2Int forward) DetermineTile(Vector2Int position, ICollection<Vector2Int> positions) {
        
        neighbors.Clear();
        neighbors.AddRange(positions.Where(p => (position - p).ManhattanLength() == 1).ToList());
        
        switch (neighbors.Count) {
            case 0:
                return (Type.Isolated, Vector2Int.up);
            case 1:
                return (Type.Cap, (position - neighbors[0]));
            case 4:
                return (Type.X, Vector2Int.up);
            case 3: {
                var missingPosition = offsets.Select(offset => offset + position).Except(neighbors).Single();
                return (Type.T, (position - missingPosition));
            }
            case 2: {
                var offset = neighbors[0] - position;
                // I
                if (neighbors.Contains(position - offset))
                    return (Type.I, offset);
                // L
                var up = ((neighbors[0] - position).Rotate(3) == (neighbors[1] - position) ? neighbors[0] : neighbors[1]) - position;
                return (Type.L, up);
            }
            default:
                throw new Exception("should be unreachable");
        }
    }
}