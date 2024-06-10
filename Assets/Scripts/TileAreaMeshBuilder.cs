using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TileAreaMeshBuilder {

    public static readonly Vector2Int[] offsets = {

        new(-1, -1),
        new(0, -1),
        new(1, -1),

        new(-1, 0),
        new(0,0),
        new(1, 0),

        new(-1, 1),
        new(0, 1),
        new(1, 1),
    };

    public static readonly HashSet<Vector2Int> positions = new();
    public static readonly HashSet<Vector2Int> border = new();

    public static Vector3[] subdividedQuadVertices;
    public static Vector2[] subdividedQuadUvs;
    public static int[] subdividedQuadTriangles;

    public static Vector3[] quadVertices ;
    public static Vector2[] quadUvs;
    public static int[] quadTriangles;
    
    public static List<Vector3> vertices = new();
    public static List<Vector2> uvs = new();
    public static List<int> triangles = new();
    public static List<Color> colors = new();

    public static Mesh Build(IEnumerable<Vector2Int> _positions) {

        positions.Clear();
        positions.UnionWith(_positions);

        border.Clear();
        foreach (var position in positions)
        foreach (var offset in offsets)
            border.Add(position + offset);
        border.ExceptWith(positions);

        if (subdividedQuadVertices == null) {
            var subdividedQuad = "QuadSubdivided".LoadAs<Mesh>();
             subdividedQuadVertices = subdividedQuad.vertices;
             subdividedQuadUvs = subdividedQuad.uv;
             subdividedQuadTriangles = subdividedQuad.triangles;    
        }
        if (quadVertices == null) {
            var quad = "Quad".LoadAs<Mesh>();
             quadVertices = quad.vertices;
             quadUvs = quad.uv;
             quadTriangles = quad.triangles;
        }
        
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        colors.Clear();

        void Add(Vector2Int position, Vector3[] pieceVertices, Vector2[] pieceUvs, int[] pieceTriangles) {

            var translate = Matrix4x4.Translate(position.ToVector3Int());
            var vertexStartIndex = vertices.Count;

            foreach (var vertex in pieceVertices) {
                Vector3 worldPosition = translate * vertex.ToVector4();
                vertices.Add(worldPosition);

                var minDistance = float.MaxValue;
                foreach (var p in offsets.Select(offset => position + offset).Where(p => positions.Contains(p)))
                    minDistance = Mathf.Min(minDistance, (worldPosition.ToVector2() - p).SignedDistanceBox(Vector2.one / 2));

                colors.Add(new Color(minDistance, 0, 0));
            }
            foreach (var uv in pieceUvs)
                uvs.Add(uv);

            foreach (var triangle in pieceTriangles)
                triangles.Add(vertexStartIndex + triangle);
        }

        foreach (var position in positions)
            Add(position, quadVertices, quadUvs, quadTriangles);

        foreach (var position in border)
            Add(position, subdividedQuadVertices, subdividedQuadUvs, subdividedQuadTriangles);

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles,0);
        mesh.SetUVs(0,uvs);
        mesh.SetColors(colors);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}