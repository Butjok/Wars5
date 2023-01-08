using System.Collections.Generic;
using UnityEngine;

public static class MoveSequenceMeshBuilder {
	
	public static readonly List<Vector3> vertices = new();
	public static readonly List<Vector2> uvs = new();
	public static readonly List<int> triangles = new();
	
	public static Mesh Build(Mesh mesh, MoveSequence moveSequence, MoveSequenceAtlas atlas) {

		if (!mesh)
			mesh = new Mesh();
		else
			mesh.Clear();

		vertices.Clear();
		uvs.Clear();
		triangles.Clear();

		var uvOffset = Vector2.zero;
		
		foreach (var move in moveSequence.segments) {

			if (move.type is MoveSequence.Segment.Type.RotateLeft or MoveSequence.Segment.Type.RotateRight or MoveSequence.Segment.Type.Rotate180)
				continue;

			var position = move.type == MoveSequence.Segment.Type.StartMoving
				? move.startPoint.RoundToInt()
				: (move.startPoint + (Vector2)move.startDirection / 2).RoundToInt();

			var translate = Matrix4x4.Translate(position.ToVector3Int());
			var rotate = Matrix4x4.Rotate(Quaternion.LookRotation(move.startDirection.ToVector3Int(), Vector3.up));
			var transform = translate * rotate;
			
			foreach (var vertex in MeshUtils.quad)
				vertices.Add(transform * vertex.ToVector4());

			var rect = atlas.uv[move.type];
			var pp = new Vector2(rect.xMax, rect.yMax);
			var pm = new Vector2(rect.xMax, rect.yMin);
			var mp = new Vector2(rect.xMin, rect.yMax);
			var mm = new Vector2(rect.xMin, rect.yMin);
			
			uvs.Add(pp + uvOffset);
			uvs.Add(mm + uvOffset);
			uvs.Add(mp + uvOffset);
			
			uvs.Add(mm + uvOffset);
			uvs.Add(pp + uvOffset);
			uvs.Add(pm + uvOffset);

			uvOffset += new Vector2(0, 1);
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