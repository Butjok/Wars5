using System.Collections.Generic;
using UnityEngine;

// See "A Fast Voxel Traversal Algorithm for Ray Tracing" by John Amanatides & Andrew Woo

public static class Woo {
	public static IEnumerable<Vector2Int> Traverse2D(Vector2 from, Vector2 to) {
		var x = Mathf.RoundToInt(from.x);
		var y = Mathf.RoundToInt(from.y);

		var offset = to - from;

		var target = new Vector2(
			offset.x > 0 ? x + .5f : x - .5f,
			offset.y > 0 ? y + .5f : y - .5f);

		var tMaxX = offset.x == 0 ? float.MaxValue : (target.x - from.x) / offset.x;
		var tMaxY = offset.y == 0 ? float.MaxValue : (target.y - from.y) / offset.y;

		var tDelta = new Vector2(1f / Mathf.Abs(offset.x), 1f / Mathf.Abs(offset.y));
		var step = new Vector2Int(offset.x > 0 ? 1 : -1, offset.y > 0 ? 1 : -1);

		while (true) {
			if (tMaxX >= 1 && tMaxY >= 1) break;

			if (tMaxX < tMaxY) {
				tMaxX += tDelta.x;
				x += step.x;
			}
			else {
				tMaxY += tDelta.y;
				y += step.y;
			}

			yield return new Vector2Int(x, y);
		}
	}
}