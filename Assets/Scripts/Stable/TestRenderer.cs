using UnityEngine;

public class TestRenderer : MonoBehaviour {
	public Mesh mesh;
	public Material material;
	public void Update() {
		material.SetMatrix("_Transform", transform.localToWorldMatrix);
		Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
	}
}