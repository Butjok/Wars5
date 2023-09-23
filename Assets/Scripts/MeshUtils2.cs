using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

public static class MeshUtils2 {

    public class Vertex {
        public Vector3 position;
        public Vector2? uv0, uv1, uv2;
        public Color? color;
    }

    public class Quad {
        public Vertex a, b, c, d;
        public Vector3 Normal => Vector3.Cross(b.position - a.position, c.position - a.position).normalized;
        public void Flip() {
            (a, c) = (c, a);
        }
        public IEnumerable<Vertex> Vertices {
            get {
                yield return a;
                yield return b;
                yield return c;
                yield return d;
            }
        }
    }

    public static Mesh Construct(IEnumerable<Quad> quads, Mesh mesh, float thresholdAngle = 30) {

        var quads2 = quads.ToList();
        var vertices = quads2.SelectMany(quad => new[] { quad.a, quad.b, quad.c, quad.d }).Distinct().ToList();
        int Index(Vertex vertex) {
            var index = vertices.IndexOf(vertex);
            Assert.IsTrue(index != -1);
            return index;
        }

        var triangles = new List<int>();
        foreach (var quad in quads2) {
            triangles.Add(Index(quad.a));
            triangles.Add(Index(quad.b));
            triangles.Add(Index(quad.c));
            triangles.Add(Index(quad.a));
            triangles.Add(Index(quad.c));
            triangles.Add(Index(quad.d));
        }

        if (mesh)
            mesh.Clear();
        else
            mesh = new Mesh();

        mesh.vertices = vertices.Select(vertex => vertex.position).ToArray();
        if (vertices.All(vertex => vertex.uv0 != null))
            mesh.uv = vertices.Select(vertex => vertex.uv0.Value).ToArray();
        if (vertices.All(vertex => vertex.uv1 != null))
            mesh.uv2 = vertices.Select(vertex => vertex.uv1.Value).ToArray();
        if (vertices.All(vertex => vertex.uv2 != null))
            mesh.uv3 = vertices.Select(vertex => vertex.uv2.Value).ToArray();
        if (vertices.All(vertex => vertex.color != null))
            mesh.colors = vertices.Select(vertex => vertex.color.Value).ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}