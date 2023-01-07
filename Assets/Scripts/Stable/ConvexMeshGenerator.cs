using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class ConvexMeshGenerator : MonoBehaviour {
	public MeshCollider collider;
	public List<Vector2> input = new();
	public IList<Vector2> output;
	public void Awake() {
		collider = GetComponent<MeshCollider>();
	}
	[ContextMenu(nameof(Calculate))]
	public void Calculate() {
		input.Clear();
		for (var i = 0; i < 100; i++)
			input.Add(Random.insideUnitCircle * Random.value);
		output = ConvexHull.Compute(input);
		var vertices=output.Select(v => v.ToVector3()).ToList();
		vertices.AddRange(vertices.Select(v=>v+Vector3.up));
		if (output.Count > 2) {
			var mesh = new Mesh();
			mesh.vertices = output.Select(v=>(Vector3)v).ToArray();
			var triangles = new List<int>();
			for (var i = 2; i < output.Count; i++) {
				triangles.Add(0);
				triangles.Add(i-1);
				triangles.Add(i);
				triangles.Add(0);
				triangles.Add(i);
				triangles.Add(i-1);
			}
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateNormals();
			collider.sharedMesh = mesh;
		}
	}
	public void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		foreach (var position in input)
			Gizmos.DrawWireSphere(position, .01f);
		/*if (output != null && output.Count > 1) {
			Gizmos.color = Color.white;
			for (var i = 0; i < output.Count; i++)
				Gizmos.DrawLine(output[i], output[(i + 1) % output.Count]);
		}*/
		if (probe) {
			
		}
	}
	public Transform probe;
}