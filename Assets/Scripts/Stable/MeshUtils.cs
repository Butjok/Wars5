using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public static class MeshUtils {

    public static readonly Vector3[] quad = new Vector2[] {
        new(+.5f, +.5f),
        new(-.5f, -.5f),
        new(-.5f, +.5f),

        new(-.5f, -.5f),
        new(+.5f, +.5f),
        new(+.5f, -.5f),
    }.Select(vector2 => MathUtils.ToVector3((Vector2)vector2)).ToArray();

    public static IEnumerable<Vector3> QuadAt(Vector3 position) {
        return quad.Select(vertex => position + vertex);
    }

    /// <summary>
    /// the colors count must match the vertices count
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="source"></param>
    /// <param name="transform"></param>
    public static void AppendMesh(
        (List<Vector3>vertices, List<int>triangles, List<Color> colors) destination,
        (IEnumerable<Vector3> vertices, IEnumerable<int> triangles, IEnumerable<Color> colors) source,
        Matrix4x4 transform) {
        var triangleStart = destination.vertices.Count;

        foreach (var vertex in source.vertices) {
            var transformed = transform.MultiplyPoint(vertex);
            destination.vertices.Add(transformed);
        }

        foreach (var triangle in source.triangles)
            destination.triangles.Add(triangle + triangleStart);

        foreach (var color in source.colors)
            destination.colors.Add(color);
    }

    public static void StartProceduralMesh(out List<Vector3> vertices, out List<int> triangles) {
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    public static void StartProceduralMesh(out List<Vector3> vertices, out List<int> triangles, out List<Vector2> uvs) {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
    }

    public static void StartProceduralMesh(out List<Vector3> vertices, out List<int> triangles, out List<Color> colors) {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
    }

    public static void StartProceduralMesh(out List<Vector3> vertices, out List<int> triangles, out List<Vector2> uvs, out List<Color> colors) {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        colors = new List<Color>();
    }

    
}