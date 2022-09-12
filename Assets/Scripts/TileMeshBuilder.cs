using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TileMeshBuilder {

	public static readonly List<Vector3> vertices = new();
	public static readonly List<Vector2> uvs = new();
	public static readonly List<int> triangles = new();

	public static Mesh Build(Mesh mesh, Game2 level, Traverser traverser) {

		if (!mesh)
			mesh = new Mesh();
		else
			mesh.Clear();

		vertices.Clear();
		uvs.Clear();
		triangles.Clear();

		foreach (var position in level.tiles.Keys.Where(traverser.IsReachable)) {

			var translate = Matrix4x4.Translate(position.ToVector3Int());

			foreach (var vertex in MeshUtils.quad)
				vertices.Add(translate * vertex.ToVector4());

			var rect = new Rect(0, 0, 1, 1);
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