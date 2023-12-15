using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MarchingSquares {

    public enum SquareType { Empty, Full, StraightLine, ConvexCorner, ConcaveCorner }

    [Serializable]
    public class SquareSet {
        public Mesh full, straightLine, convexCorner, concaveCorner;
        public int fullRotationOffset, straightLineRotationOffset, convexCornerRotationOffset, concaveCornerRotationOffset;
        public Mesh this[SquareType type] => type switch {
            SquareType.Empty => null,
            SquareType.Full => full,
            SquareType.StraightLine => straightLine,
            SquareType.ConvexCorner => convexCorner,
            SquareType.ConcaveCorner => concaveCorner
        };
        public int GetRotationOffset(SquareType type) => type switch {
            SquareType.Empty => 0,
            SquareType.Full => fullRotationOffset,
            SquareType.StraightLine => straightLineRotationOffset,
            SquareType.ConvexCorner => convexCornerRotationOffset,
            SquareType.ConcaveCorner => concaveCornerRotationOffset
        };
    }

    public struct Square {
        public Vector2 position;
        public SquareType type;
        public int rotation;
        public int? rotation2;
    }

    public static IEnumerable<Square> EnumerateSquares(Dictionary<Vector2Int, int> corners, float threshold) {

        if (corners.Count == 0)
            yield break;

        var cornersRange = new RectInt {
            xMin = corners.Keys.Min(x => x.x),
            xMax = corners.Keys.Max(x => x.x),
            yMin = corners.Keys.Min(x => x.y),
            yMax = corners.Keys.Max(x => x.y)
        };

        for (var squareMinY = cornersRange.yMin - 1; squareMinY <= cornersRange.yMax; squareMinY++)
        for (var squareMinX = cornersRange.xMin - 1; squareMinX <= cornersRange.xMax; squareMinX++) {

            var bottomLeft = new Vector2Int(squareMinX, squareMinY);
            var center = bottomLeft + Vector2.one / 2;
            int HeightAt(Vector2Int corner) {
                return corners.TryGetValue(bottomLeft + corner, out var height) ? height : 0;
            }
            int CornerBit(Vector2Int corner) {
                return HeightAt(corner) > threshold ? 1 : 0;
            }
            var caseMask = (CornerBit(Vector2Int.zero) << 0) +
                           (CornerBit(Vector2Int.right) << 1) +
                           (CornerBit(Vector2Int.one) << 2) +
                           (CornerBit(Vector2Int.up) << 3);

            if (caseMask != 0) {

                SquareType type;
                int rotation;
                int? rotation2 = null;

                // full square
                if (caseMask == 15) {
                    type = SquareType.Full;
                    rotation = 0;
                }
                // horizontal or vertical isoline
                else if (caseMask % 3 == 0) {
                    type = SquareType.StraightLine;
                    rotation = caseMask switch { 3 => 0, 6 => -1, 9 => -3, 12 => -2 };
                }
                // only one corner is lifted
                else if (caseMask is (1 or 2 or 4 or 8)) {
                    type = SquareType.ConvexCorner;
                    rotation = caseMask switch { 1 => 0, 2 => -1, 4 => -2, 8 => -3 };
                }
                // 
                else if (caseMask is 5 or 10) {
                    type = SquareType.ConvexCorner;
                    rotation = caseMask switch { 5 => 0, 10 => -1 };
                    rotation2 = rotation + 2;
                }
                else {
                    type = SquareType.ConcaveCorner;
                    rotation = caseMask switch { 7 => 0, 11 => 1, 13 => 2, 14 => 3 };
                }

                yield return new Square { position = center, type = type, rotation = rotation, rotation2 = rotation2 };
            }
        }
    }
}