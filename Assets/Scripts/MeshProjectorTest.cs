using UnityEngine;

public class MeshProjectorTest : MonoBehaviour {

    public Mesh sourceMesh;
    public Mesh mesh;
    public MeshFilter meshFilter;
    public float offset = 0;

    public void Awake() {
        mesh = Instantiate(sourceMesh);
    }

    public void Update() {
        if (mesh.TryProjectDown(sourceMesh, transform.position, transform.rotation.eulerAngles.y, 1 << LayerMask.NameToLayer("Terrain"), offset))
            meshFilter.sharedMesh = mesh;
    }
}