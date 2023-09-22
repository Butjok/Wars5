using System.Collections.Generic;
using UnityEngine;

public static class MeshProjector {

    public static bool TryProjectDown(this Mesh destinationMesh, Mesh sourceMesh, Vector3 position, float yaw, LayerMask layerMask,
        float offset = 0) {

        var outputVertices = new List<Vector3>();

        var transform = Matrix4x4.TRS(position, Quaternion.Euler(0, yaw, 0), Vector3.one);
        foreach (var vertex in sourceMesh.vertices) {
            var worldPosition = transform.MultiplyPoint(vertex);
            var ray = new Ray(worldPosition, Vector3.down);
            if (Physics.Raycast(ray, out var hit, float.PositiveInfinity, layerMask)) {
                var localPosition = transform.inverse.MultiplyPoint(hit.point + (vertex.y + offset) * Vector3.up);
                outputVertices.Add(localPosition);
            }
            else
                return false;
        }

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

        return true;
    }
}