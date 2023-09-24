using System.Collections.Generic;
using UnityEngine;

public class MeshProjector : MonoBehaviour {

    public Mesh sourceMesh;
    public Mesh mesh;
    public MeshFilter meshFilter;
    public float offset = 0;

    public void Awake() {
        mesh = Instantiate(sourceMesh);
    }

    public void Update() {
        if (TryProjectDown(mesh, sourceMesh, transform.position, transform.rotation.eulerAngles.y, 1 << LayerMask.NameToLayer("Terrain"), offset))
            meshFilter.sharedMesh = mesh;
    }
    public static bool TryProjectDown(Mesh destinationMesh, Mesh sourceMesh, Vector3 position, float yaw, LayerMask layerMask,
        float offset = 0) {

        var result = true;
        var outputVertices = new List<Vector3>();

        var transform = Matrix4x4.TRS(position, Quaternion.Euler(0, yaw, 0), Vector3.one);
        foreach (var vertex in sourceMesh.vertices) {
            var worldPosition = transform.MultiplyPoint(vertex);
            var ray = new Ray(worldPosition + Vector3.up*1000, Vector3.down);
            var wasHit = Physics.Raycast(ray, out var hit, float.PositiveInfinity, layerMask);
            if (!wasHit)
                result = false;
            var point = wasHit ? hit.point : ray.origin;
            var localPosition = transform.inverse.MultiplyPoint(point + (vertex.y + offset) * Vector3.up);
            outputVertices.Add(localPosition);
        }

        destinationMesh.Clear();

        destinationMesh.vertices = outputVertices.ToArray();
        destinationMesh.triangles = sourceMesh.triangles;
        destinationMesh.uv = sourceMesh.uv;
        destinationMesh.uv2 = sourceMesh.uv2;
        destinationMesh.uv3 = sourceMesh.uv3;
        destinationMesh.uv4 = sourceMesh.uv4;
        destinationMesh.colors = sourceMesh.colors;

        destinationMesh.RecalculateBounds();
        destinationMesh.RecalculateNormals();
        destinationMesh.RecalculateTangents();

        return result;
    }
}