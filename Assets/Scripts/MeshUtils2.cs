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

    private static readonly List<Quad> quads2 = new();
    private static readonly Dictionary<Vertex, int> indices = new();
    private static readonly List<int> triangles = new();
    private static readonly List<Vector3> vertices = new();
    private static readonly HashSet<Vertex> uniqueVertices = new();
    private static readonly List<Vector2> uv0s = new();

    public static Mesh Construct(IEnumerable<Quad> quads, Mesh mesh, float thresholdAngle = 30) {

        quads2.Clear();
        quads2.AddRange(quads);
        
        uniqueVertices.Clear();
        foreach (var quad in quads2) {
            uniqueVertices.Add(quad.a);
            uniqueVertices.Add(quad.b);
            uniqueVertices.Add(quad.c);
            uniqueVertices.Add(quad.d);
        }
        
        vertices.Clear();
        indices.Clear();
        uv0s.Clear();
        foreach (var vertex in uniqueVertices) {
            vertices.Add(vertex.position);
            indices[vertex] = indices.Count;
            uv0s.Add(vertex.uv0 ?? default);
        }

        triangles.Clear();
        foreach (var quad in quads2) {
            triangles.Add(indices[quad.a]);
            triangles.Add(indices[quad.b]);
            triangles.Add(indices[quad.c]);
            triangles.Add(indices[quad.a]);
            triangles.Add(indices[quad.c]);
            triangles.Add(indices[quad.d]);
        }

        if (mesh)
            mesh.Clear();
        else
            mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uv0s);
        
        //if (vertices.Any(vertex => vertex.uv0 != null))
        //  mesh.uv = vertices.Select(vertex => vertex.uv0 ?? default).ToArray();
        /*if (vertices.Any(vertex => vertex.uv1 != null))
            mesh.uv2 = vertices.Select(vertex => vertex.uv1 ?? default).ToArray();
        if (vertices.Any(vertex => vertex.uv2 != null))
            mesh.uv3 = vertices.Select(vertex => vertex.uv2 ?? default).ToArray();
        if (vertices.Any(vertex => vertex.color != null))
            mesh.colors = vertices.Select(vertex => vertex.color ?? default).ToArray();*/
        //mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}