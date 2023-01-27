using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

public static class MathUtils {

	public static int Cross(this Vector2Int a, Vector2Int b) {
		return a.x * b.y - a.y * b.x;
	}
	public static float Cross(this Vector2 a, Vector2 b) {
		return a.x * b.y - a.y * b.x;
	}

	[Pure]public static Vector2Int RoundToInt(this Vector2 a) {
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
		foreach (var item in sequence)
			if (UnityEngine.Random.Range(0, ++i) == 0)
				result = item;
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
	public static bool IsIn(this int value, Vector2Int range) {
		return range[0] <= value && value <= range[1];
	}

	public static int PositiveModulo(this int a, int b) {
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
		return  new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
	}
	public static float SignedDistanceBox(this Vector2 samplePosition, Vector2 halfSize) {

		var componentWiseEdgeDistance = Abs(samplePosition) - halfSize;
		var outsideDistance = Vector2.Max(componentWiseEdgeDistance, Vector2.zero).magnitude;
		var insideDistance = Mathf.Min(Mathf.Max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
		return outsideDistance + insideDistance;
	}

	public static (Vector2Int min, Vector2Int max) GetMinMax(this IEnumerable<Vector2Int>positions) {
		
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
		return (new Vector2Int(minX, minY), new Vector2Int(maxX,maxY));
	}

	public static T Cycle<T>(this T value, IEnumerable<T> _values, int offset = 1) {
		var values = _values.ToArray();
		var index = Array.IndexOf(values, value);
		var nextIndex = index == -1 ? 0 : (index + offset).PositiveModulo(values.Length);
		return values[nextIndex];
	}

	public static Quaternion SlerpWithMaxSpeed(this Quaternion rotation, Quaternion targetRotation, float maxSpeed = 9999) {
		var angle = Quaternion.Angle(rotation, targetRotation);
		if (Mathf.Approximately(0, angle))
			return rotation;
		var maxSpeedThisFrame = Time.deltaTime * maxSpeed;
		var angleToRotate = Mathf.Min(maxSpeedThisFrame, angle);
		return Quaternion.Slerp(rotation, targetRotation, angleToRotate / angle);
	}
}