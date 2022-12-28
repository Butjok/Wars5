using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshUtils {
	
	public static readonly Vector3[] quad = new Vector2[] {
   
		new(+.5f, +.5f),
		new(-.5f, -.5f),
		new(-.5f, +.5f),
   
		new(-.5f, -.5f),
		new(+.5f, +.5f),
		new(+.5f, -.5f),
   
	}.Select(vector2 => MathUtils.ToVector3((Vector2)vector2)).ToArray();

	public static IEnumerable<Vector3> QuadAt(Vector3 position) {
		return quad.Select(vertex => position + vertex);
	}
}