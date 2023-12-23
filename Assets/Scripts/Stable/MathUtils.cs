using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Mathf;
using Random = System.Random;

public static class MathUtils {

    public static int Cross(this Vector2Int a, Vector2Int b) {
        return a.x * b.y - a.y * b.x;
    }

    public static float Cross(this Vector2 a, Vector2 b) {
        return a.x * b.y - a.y * b.x;
    }

    [Pure] public static Vector2Int RoundToInt(this Vector2 a) {
        return new Vector2Int(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y));
    }

    public static Vector2 Rotate(this Vector2 v, int rotation) {
        rotation = (rotation % 4 + 16) % 4;
        return rotation switch {
            0 => v,
            1 => new Vector2(-v.y, v.x),
            2 => -v,
            3 => new Vector2(v.y, -v.x),
            _ => throw new Exception()
        };
    }

    public static Vector2Int Rotate(this Vector2Int v, int rotation) {
        rotation = (rotation % 4 + 16) % 4;
        return rotation switch {
            0 => v,
            1 => new Vector2Int(-v.y, v.x),
            2 => -v,
            3 => new Vector2Int(v.y, -v.x),
            _ => throw new Exception()
        };
    }

    public static int Rotation(this Vector2Int from, Vector2Int to) {
        return from == -to ? 2 : from.Cross(to);
    }

    public static Vector3 ToVector3(this Vector2 v) {
        return new Vector3(v.x, 0, v.y);
    }

    public static Vector2 ToVector2(this Vector3 v) {
        return new Vector2(v.x, v.z);
    }

    public static Vector4 ToVector4(this Vector3 v) {
        return new Vector4(v.x, v.y, v.z, 1);
    }

    public static Vector3Int ToVector3Int(this Vector2Int v) {
        return new Vector3Int(v.x, 0, v.y);
    }

    public static Vector3Int RoundToInt(this Vector3 v) {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    public static Vector3 ToVector3(this Vector3Int v) {
        return new Vector3(v.x, v.y, v.z);
    }

    public static Quaternion ToQuaternion(this int rotation) {
        return Quaternion.Euler(0, 90 * rotation, 0);
    }

    public static T Random<T>(this T[] array) {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    public static T Random<T>(this IEnumerable<T> sequence) {
        var i = 0;
        T result = default;
        var count = 0;
        foreach (var item in sequence) {
            count++;
            if (UnityEngine.Random.Range(0, ++i) == 0)
                result = item;
        }

        Assert.AreNotEqual(0, count, "empty sequence for Random()");
        return result;
    }

    public static IEnumerable<T> Randomize<T>(this IEnumerable<T> sequence) {
        return sequence.OrderBy(_ => UnityEngine.Random.value);
    }

    public static IEnumerable<Vector2Int> Offsets(this Vector2Int range) {
        for (var radius = range[0]; radius <= range[1]; radius++)
        for (var x = radius; x >= -radius; x--) {
            var y = radius - Mathf.Abs(x);
            yield return new Vector2Int(x, y);
            if (y != 0)
                yield return new Vector2Int(x, -y);
        }
    }

    public static int ManhattanLength(this Vector2Int vector) {
        return Mathf.Abs(vector.x) + Mathf.Abs(vector.y);
    }

    public static int ManhattanDistance(Vector2Int a, Vector2Int b) {
        return (a - b).ManhattanLength();
    }

    public static bool IsInRange(this int value, Vector2Int range) {
        return range[0] <= value && value <= range[1];
    }

    public static int PositiveModulo(this int a, int b) {
        Assert.IsTrue(b > 0);
        var result = (a % b + b) % b;
        Assert.IsTrue(result >= 0);
        return result;
    }

    public static float PositiveModulo(this float a, float b) {
        Assert.IsTrue(b > 0);
        var result = (a % b + b) % b;
        Assert.IsTrue(result >= 0);
        return result;
    }

    public static float Wrap360(this float value) {
        return (value % 360 + 360) % 360;
    }

    public static int Sign(this float value) {
        return value > 0 ? 1 : -1;
    }

    public static int ZeroSign(this float value) {
        return Mathf.Approximately(value, 0) ? 0 : Sign(value);
    }

    public static Vector2 Abs(this Vector2 v) {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    public static float SignedDistanceBox(this Vector2 samplePosition, Vector2 halfSize) {
        var componentWiseEdgeDistance = Abs(samplePosition) - halfSize;
        var outsideDistance = Vector2.Max(componentWiseEdgeDistance, Vector2.zero).magnitude;
        var insideDistance = Mathf.Min(Mathf.Max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
        return outsideDistance + insideDistance;
    }

    public static (Vector2Int min, Vector2Int max) GetMinMax(this IEnumerable<Vector2Int> positions) {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MaxValue;

        foreach (var position in positions) {
            minX = Mathf.Min(minX, position.x);
            minY = Mathf.Min(minY, position.y);
            maxX = Mathf.Min(maxX, position.x);
            maxY = Mathf.Min(maxY, position.y);
        }

        Assert.AreNotEqual(minX, int.MaxValue);
        return (new Vector2Int(minX, minY), new Vector2Int(maxX, maxY));
    }

    public static T Cycle<T>(this T value, IEnumerable<T> _values, int offset = 1) {
        var values = _values.ToArray();
        var index = Array.IndexOf(values, value);
        var nextIndex = index == -1 ? 0 : (index + offset).PositiveModulo(values.Length);
        return values[nextIndex];
    }

    public static T GetWrapped<T>(this IReadOnlyList<T> list, int index) {
        Assert.IsTrue(list.Count > 0);
        return list[index.PositiveModulo(list.Count)];
    }

    public static Quaternion SlerpWithMaxSpeed(this Quaternion rotation, Quaternion targetRotation, float maxSpeed = 9999) {
        var angle = Quaternion.Angle(rotation, targetRotation);
        if (Mathf.Approximately(0, angle))
            return rotation;
        var maxSpeedThisFrame = Time.deltaTime * maxSpeed;
        var angleToRotate = Mathf.Min(maxSpeedThisFrame, angle);
        return Quaternion.Slerp(rotation, targetRotation, angleToRotate / angle);
    }

    public static Vector2 Average(this IEnumerable<Vector2Int> positions) {
        var accumulator = Vector2.zero;
        var count = 0;
        foreach (var position in positions) {
            accumulator += position;
            count++;
        }

        Assert.AreNotEqual(0, count);
        return accumulator / count;
    }

    public static Vector2 Center(this IEnumerable<Vector2Int> positions) {
        var min = new Vector2Int(int.MaxValue, int.MaxValue);
        var max = new Vector2Int(int.MinValue, int.MinValue);

        foreach (var position in positions) {
            min.x = Min(min.x, position.x);
            min.y = Min(min.y, position.y);
            max.x = Max(min.x, position.x);
            max.y = Max(min.y, position.y);
        }

        Assert.AreNotEqual(min, new Vector2Int(int.MaxValue, int.MaxValue));
        Assert.AreNotEqual(max, new Vector2Int(int.MinValue, int.MinValue));

        return Vector2.Lerp(min, max, .5f);
    }

    public static Vector2Int Apply(this Vector2Int v, Func<int, int> function) {
        return new Vector2Int(function(v[0]), function(v[1]));
    }

    public static string ToStringWithThousandsSeparator(this int value, string thousandsSeparator = " ") {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = thousandsSeparator;
        return value.ToString("#,0", nfi);
    }

    public static Color32 YiqContrastColor(this Color32 color) {
        var yiq = (color.r * 299 + color.r * 587 + color.r * 114) / 1000;
        return yiq >= 128 ? Color.black : Color.white;
    }

    public static Color YiqContrastColor(this Color color) {
        return YiqContrastColor((Color32)color);
    }

    public static float SmoothStep(float edge0, float edge1, float x) {
        if (x < edge0)
            return 0;

        if (x >= edge1)
            return 1;

        // Scale/bias into [0..1] range
        x = (x - edge0) / (edge1 - edge0);

        return x * x * (3 - 2 * x);
    }

    public static Vector3 InverseTransformPointWithoutScale(this Transform transform, Vector3 point) {
        return Quaternion.Inverse(transform.rotation) * (point - transform.position);
    }

    public static (int low, int high) FitSegment(int count, int low, int high) {
        if (low < 0 && high >= count) {
            low = 0;
            high = count - 1;
        }
        else if (low < 0) {
            high = Mathf.Min(count - 1, high - low);
            low = 0;
        }
        else if (high >= count) {
            low = Mathf.Max(0, low - high + count - 1);
            high = count - 1;
        }

        return (low, high);
    }

    public static Vector2Int[] offsets = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

    public static IEnumerable<(Vector2Int position, TileType tileType)> Neighbors(this IReadOnlyDictionary<Vector2Int, TileType> tiles, Vector2Int position) {
        foreach (var offset in offsets)
            if (tiles.TryGetValue(position + offset, out var neighbor))
                yield return (position + offset, neighbor);
    }

    public static Vector2Int ToVector2Int(this Vector3 vector3) {
        return vector3.ToVector2().RoundToInt();
    }

    public static Vector3 ToVector3(this Vector2Int vector2Int) {
        return vector2Int.ToVector3Int().ToVector3();
    }

    public static int GetOrientation(Vector2 a, Vector2 b, Vector2 c) {
        var ab = b - a;
        var bc = c - b;
        return ab.Cross(bc) switch {
            0 => 0,
            > 0 => 1,
            < 0 => -1,
            float.NaN => throw new Exception("NaN"),
        };
    }

    public static Rect ToPreciseBounds(this RectInt bounds) {
        return new Rect { min = bounds.min - Vector2.one / 2, max = bounds.max + Vector2.one / 2 };
    }

    public static Rect GetScreenSpaceAABB(this Camera camera, IEnumerable<Vector3> points) {
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (var point in points) {
            Vector2 screenSpacePoint = camera.WorldToScreenPoint(point);
            min = Vector2.Min(min, screenSpacePoint);
            max = Vector2.Max(max, screenSpacePoint);
        }

        return new Rect(min, max - min);
    }

    public static bool TryGetShortestLine(Rect a, Rect b, out Vector2 aa, out Vector2 bb) {
        const int min = 0, max = 1;

        bool TryIntersectRanges(float aMin, float aMax, float bMin, float bMax, out Vector2 ab) {
            var a = new Vector2(aMin, aMax);
            var b = new Vector2(bMin, bMax);
            if (b[max] < a[min]) {
                ab = new Vector2(a[min], b[max]);
                return false;
            }

            if (b[min] > a[max]) {
                ab = new Vector2(a[max], b[min]);
                return false;
            }

            if (b[min] <= a[min] && b[max] >= a[max]) {
                var center = (a[min] + a[max]) / 2;
                ab = new Vector2(center, center);
                return true;
            }

            if (b[min] >= a[min] && b[max] <= a[max]) {
                var center = (b[min] + b[max]) / 2;
                ab = new Vector2(center, center);
                return true;
            }

            if (b[max] >= a[min] && b[max] <= a[max]) {
                var center = (a[min] + b[max]) / 2;
                ab = new Vector2(center, center);
                return true;
            }

            if (b[min] >= a[min] && b[min] <= a[max]) {
                var center = (b[min] + a[max]) / 2;
                ab = new Vector2(center, center);
                return true;
            }

            throw new Exception();
        }

        var xTest = TryIntersectRanges(a.xMin, a.xMax, b.xMin, b.xMax, out var abx);
        var yTest = TryIntersectRanges(a.yMin, a.yMax, b.yMin, b.yMax, out var aby);
        if (xTest && yTest) {
            aa = bb = default;
            return false;
        }

        aa = new Vector2(abx[0], aby[0]);
        bb = new Vector2(abx[1], aby[1]);
        return true;
    }

    public static Quaternion ToRotation(this Vector3 normal, float yaw = 0) {
        var right = Vector3.Cross(Vector3.forward, normal);
        var forward = Vector3.Cross(normal, right);
        return Quaternion.LookRotation(forward, normal) * Quaternion.Euler(0, yaw, 0);
    }

    public static float Square(this float value) {
        return value * value;
    }

    public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, float totalWeight, Func<T, float> weightSelector) {
        // The weight we are after...
        var itemWeightIndex = (float)new Random().NextDouble() * totalWeight;
        float currentWeightIndex = 0;

        foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) }) {
            currentWeightIndex += item.Weight;

            // If we've hit or passed the weight we are after for this item then it's the one we want....
            if (currentWeightIndex >= itemWeightIndex)
                return item.Value;
        }

        return default(T);
    }
}