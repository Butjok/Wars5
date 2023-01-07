using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

	public bool active = true;

	public MovePathBuilder pathBuilder = new(Vector2Int.zero);
	public bool followMousePath = true;

	public Main main;
	public Traverser traverser = new Traverser();

	public void Update() {

		// if (Input.GetKeyDown(KeyCode.ScrollLock))
		// 	active = !active;
		//
		// if (!active)
		// 	return;
		//
		// if (!mesh)
		// 	mesh = new Mesh();
		//
		// pathBuilder.Clear();
		// if (Mouse.TryGetPosition(out Vector2Int mousePosition) && level.TryGetTile(mousePosition, out _)) {
		// 	traverser.Traverse(level.tiles.Keys, pathBuilder.startPosition, (position, distance) => 1);
		// 	if (traverser.IsReachable(mousePosition)) {
		//
		// 		foreach (var position in traverser.ReconstructPath(mousePosition).Skip(1))
		// 			pathBuilder.Add(position);
		//
		// 				//path = MovePath.Moves(pathBuilder.Positions, Vector2Int.down);
		// 		mesh = MovePathMeshBuilder.Build(mesh, path, atlas);
		// 		meshFilter.sharedMesh = mesh;
		// 	}
		// 	else {
		// 		pathBuilder.Clear();
		// 		mesh.Clear();
		// 	}
		// }
		// else
		// 	mesh.Clear();
		//
		// meshFilter.sharedMesh = mesh;


/*		var offset = Vector2Int.zero;
		if (Input.GetKeyDown(KeyCode.LeftArrow))
			offset.x -= 1;
		if (Input.GetKeyDown(KeyCode.RightArrow))
			offset.x += 1;
		if (Input.GetKeyDown(KeyCode.UpArrow))
			offset.y += 1;
		if (Input.GetKeyDown(KeyCode.DownArrow))
			offset.y -= 1;

		if (offset.x != 0 && offset.y != 0)
			offset.y = 0;*/

		/*if (offset != Vector2Int.zero) {
			position += offset;
			pathBuilder.Add(position);

			path = pathBuilder.GetMovePath(Vector2Int.down);
			walker.moves = path.moves;
			walker.enabled = true;

			mesh = MovePathMeshBuilder.Build(mesh, path, atlas);
			meshFilter.sharedMesh = mesh;
		}
		if (Input.GetKeyDown(KeyCode.Escape)) {
			pathBuilder.Clear();
			position = Vector2Int.zero;

			mesh.Clear();
			meshFilter.sharedMesh = mesh;
		}*/
	}
	public void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		foreach (var position in positions)
			Gizmos.DrawWireSphere(position.ToVector3Int(), .1f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(this.position.ToVector3Int(), .2f);
	}
}