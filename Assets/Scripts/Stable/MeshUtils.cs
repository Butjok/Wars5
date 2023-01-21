using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
}