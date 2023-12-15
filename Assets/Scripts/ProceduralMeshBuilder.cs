using System.Collections.Generic;
using UnityEngine;

public class ProceduralMeshBuilder {

    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector2>[] uv = new List<Vector2>[8];
    public List<Color> colors;

    private static readonly List<Vector3> vector3Buffer = new();
    private static readonly List<Vector2> vector2Buffer = new();
    private static readonly List<Color> colorBuffer = new();

    public Mesh PopulateMesh(ref Mesh target) {

        if (!target)
            target = new Mesh();
        else
            target.Clear();

        target.SetVertices(vertices);
        target.SetTriangles(triangles, 0);
        for (var i = 0; i < uv.Length; i++)
            if (uv[i] != null)
                target.SetUVs(i, uv[i]);
        if (colors != null)
            target.SetColors(colors);

        target.RecalculateBounds();
        target.RecalculateNormals();
        target.RecalculateTangents();

        return target;
    }

    public ProceduralMeshBuilder AppendVertices(Mesh mesh, Matrix4x4 transform) {
        vector3Buffer.Clear();
        mesh.GetVertices(vector3Buffer);
        var triangleOffset = vertices.Count;
        foreach (var vertex in vector3Buffer)
            vertices.Add(transform.MultiplyPoint(vertex));
        foreach (var triangle in mesh.triangles)
            triangles.Add(triangle + triangleOffset);
        return this;
    }
    public ProceduralMeshBuilder AppendAttributes<T>(ref List<T> target, IReadOnlyList<T> source) {
        target ??= new List<T>();
        target.AddRange(source);
        return this;
    }
    public ProceduralMeshBuilder AppendUv(Mesh mesh, int channel) {
        vector2Buffer.Clear();
        mesh.GetUVs(channel, vector2Buffer);
        return AppendAttributes(ref uv[channel], vector2Buffer);
    }
    public ProceduralMeshBuilder AppendColors(Mesh mesh) {
        colorBuffer.Clear();
        mesh.GetColors(colorBuffer);
        return AppendAttributes(ref colors, mesh.colors);
    }
}