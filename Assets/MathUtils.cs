using System;
using UnityEngine;

public static class MathUtils {

	public static Vector2Int[] offsets = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
	
	public static int CrossProduct(this Vector2Int a, Vector2Int b) {
		return a.x * b.y - a.y * b.x;
	}

	public static Vector2Int RoundToInt(this Vector2 a) {
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
		return from == -to ? 2 : from.CrossProduct(to);
	}

	public static Vector3 ToVector3(this Vector2 v) {
		return new Vector3(v.x, 0, v.y);
	}

	public static Vector2 ToVector2(this Vector3 v) {
		return new Vector2(v.x, v.z);
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

	public static T Random<T>(this T[]array) {
		return array[UnityEngine.Random.Range(0,array.Length)];
	}
}