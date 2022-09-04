using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MovePathMeshBuilder {

	public static readonly Vector3[] quad = new Vector2[] {

		new(+.5f, +.5f),
		new(-.5f, -.5f),
		new(-.5f, +.5f),

		new(-.5f, -.5f),
		new(+.5f, +.5f),
		new(+.5f, -.5f),

	}.Select(vector2 => vector2.ToVector3()).ToArray();

	public static readonly List<Vector3> vertices = new();
	public static readonly List<Vector2> uvs = new();
	public static readonly List<int> triangles = new();
	
	public static Mesh Build(Mesh mesh, MovePath path, MoveTypeAtlas atlas) {

		if (!mesh)
			mesh = new Mesh();
		else
			mesh.Clear();

		vertices.Clear();
		uvs.Clear();

		foreach (var move in path.moves) {

			if (move.type is MovePath.MoveType.RotateLeft or MovePath.MoveType.RotateRight or MovePath.MoveType.RotateBack)
				continue;

			var position = move.type == MovePath.MoveType.Start
				? move.midpoint.RoundToInt()
				: (move.midpoint + (Vector2)move.forward / 2).RoundToInt();

			var translate = Matrix4x4.Translate(position.ToVector3Int());
			var rotate = Matrix4x4.Rotate(Quaternion.LookRotation(move.forward.ToVector3Int(), Vector3.up));
			var transform = translate * rotate;
			
			foreach (var vertex in quad)
				vertices.Add(transform * vertex.ToVector4());

			var rect = atlas[move.type];
			var pp = new Vector2(rect.xMax, rect.yMax);
			var pm = new Vector2(rect.xMax, rect.yMin);
			var mp = new Vector2(rect.xMin, rect.yMax);
			var mm = new Vector2(rect.xMin, rect.yMin);
			
			uvs.Add(pp);
			uvs.Add(mm);
			uvs.Add(mp);
			
			uvs.Add(mm);
			uvs.Add(pp);
			uvs.Add(pm);
		}

		triangles.Clear();
		for (var i = 0; i < vertices.Count; i++)
			triangles.Add(i);

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		return mesh;
	}
}