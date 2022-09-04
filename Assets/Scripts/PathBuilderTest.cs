using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PathBuilderTest : MonoBehaviour {

	public List<Vector2Int> positions = new() { Vector2Int.zero };
	public HashSet<Vector2Int> set = new() { Vector2Int.zero };
	public Vector2Int position;
	public MovePathWalker walker;
	public MovePath path;
	public MeshFilter meshFilter;
	public Mesh mesh;
	public Mesh quad;

	public MoveTypeAtlas atlas;
	
	public void Update() {

		var offset = Vector2Int.zero;
		if (Input.GetKeyDown(KeyCode.LeftArrow))
			offset.x -= 1;
		if (Input.GetKeyDown(KeyCode.RightArrow))
			offset.x += 1;
		if (Input.GetKeyDown(KeyCode.UpArrow))
			offset.y += 1;
		if (Input.GetKeyDown(KeyCode.DownArrow))
			offset.y -= 1;

		if (offset != Vector2Int.zero) {
			position += offset;
			if (!set.Contains(position)) {
				positions.Add(position);
				set.Add(position);
			}
			else
				for (var i = positions.Count - 1; i >= 0; i--) {
					if (positions[i] == position)
						break;
					set.Remove(positions[i]);
					positions.RemoveAt(i);
				}

			path = new MovePath(positions, Vector2Int.down);
			walker.moves = path.moves;
			walker.enabled = true;

			mesh = MovePathMeshBuilder.Build(mesh, path, atlas);
			meshFilter.sharedMesh = mesh;
		}
		if (Input.GetKeyDown(KeyCode.Escape)) {
			positions.Clear();
			set.Clear();
			position = Vector2Int.zero;
			positions.Add(position);
			set.Add(position);
			
			mesh.Clear();
			meshFilter.sharedMesh = mesh;
		}
	}
	public void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		foreach (var position in positions)
			Gizmos.DrawWireSphere(position.ToVector3Int(), .1f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(this.position.ToVector3Int(), .2f);
	}
}