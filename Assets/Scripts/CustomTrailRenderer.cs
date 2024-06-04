using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class CustomTrailRenderer : MonoBehaviour {

    public List<Vector3> points = new();
    public List<float> lengths = new();
    public List<float> times = new();
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public float halfWidth = .1f;

    private Mesh mesh;
    private readonly List<Vector3> rightVectors = new();
    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();
    private readonly List<Color> colors = new();

    public void Reset() {
        meshRenderer = GetComponent<MeshRenderer>();
        Assert.IsTrue(meshRenderer);
        meshFilter = GetComponent<MeshFilter>();
        Assert.IsTrue(meshFilter);
    }

    public void Rebuild() {
        rightVectors.Clear();
        for (var i = 0; i < points.Count; i++) {
            var point = points[i];
            Vector3? outgoing = i + 1 < points.Count ? points[i + 1] - point : null;
            Vector3? ingoing = i - 1 >= 0 ? point - points[i - 1] : null;
            var actualOutgoing = outgoing ?? ingoing ?? Vector3.zero;
            var actualIngoing = ingoing ?? outgoing ?? Vector3.zero;
            var direction = Vector3.Lerp(actualOutgoing, actualIngoing, .5f);
            var right = Vector3.Cross(direction, Vector3.up);
            if (right != Vector3.zero)
                right = right.normalized;
            rightVectors.Add(right);
        }

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();

        for (var i = 0; i < points.Count - 1; i++) {
            var start = points[i];
            var startRight = rightVectors[i];
            var end = points[i + 1];
            var endRight = rightVectors[i + 1];

            var vertexStartIndex = vertices.Count;

            vertices.Add(start + startRight * halfWidth);
            vertices.Add(start - startRight * halfWidth);
            vertices.Add(end + endRight * halfWidth);
            vertices.Add(end - endRight * halfWidth);

            uvs.Add(new Vector2(0, lengths[i]));
            uvs.Add(new Vector2(1, lengths[i]));
            uvs.Add(new Vector2(0, lengths[i + 1]));
            uvs.Add(new Vector2(1, lengths[i + 1]));

            for (var j = 0; j < 4; j++)
                colors.Add(new Color(times[i], 0, 0, 0));

            triangles.Add(vertexStartIndex + 0);
            triangles.Add(vertexStartIndex + 2);
            triangles.Add(vertexStartIndex + 1);
            triangles.Add(vertexStartIndex + 2);
            triangles.Add(vertexStartIndex + 3);
            triangles.Add(vertexStartIndex + 1);
        }

        if (vertices.Count > 0) {
            if (!mesh) {
                mesh = new Mesh();
                mesh.MarkDynamic();
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);

            var bounds = new Bounds(transform.InverseTransformPoint(vertices[0]), Vector3.zero);
            foreach (var vertex in vertices)
                bounds.Encapsulate(transform.InverseTransformPoint(vertex));
            mesh.bounds = bounds;

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshFilter.sharedMesh = mesh;
        }
        else
            meshFilter.sharedMesh = null;
    }
}