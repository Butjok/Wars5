using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TilemapMesh : MonoBehaviour {

    public MeshFilter filter;
    public MeshRenderer renderer;

    public void Reset() {
        filter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
    }

    public static Vector2Int bottomLeft = new(0, 0);
    public static Vector2Int bottomRight = new(1, 0);
    public static Vector2Int topLeft = new(0, 1);
    public static Vector2Int topRight = new(1, 1);
    public static Vector2Int[] quad = { bottomLeft, bottomRight, topLeft, topRight };
    public static Vector2Int[][] quadTriangles = {
        new[] { bottomLeft, topLeft, bottomRight },
        new[] { bottomRight, topLeft, topRight }
    };

    public class Vertex {
        public Vector3 position;
        public Color color;
        public int flatIndex;
    }
    public class Triangle {
        public Vertex[] vertices = new Vertex[3];
    }
    public class Quad {
        public Triangle[] triangles = { new(), new() };
    }

    private void OnEnable() {
        RebuildAndShow(new[] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(3, 3),
        });
    }

    public HashSet<Vector2Int> positions = new();
    public void Update() {
        var left = Input.GetMouseButton(Mouse.left);
        var right = Input.GetMouseButton(Mouse.right);
        if ((left || right) && Camera.main.TryRaycastPlane(out var hit)) {
            var position = hit.ToVector2Int();
            if (left)
                positions.Add(position);
            else if (right)
                positions.Remove(position);
            RebuildAndShow(positions);
        }
    }

    public void RebuildAndShow(IReadOnlyCollection<Vector2Int> positions) {

        var subdivisions = new Vector2Int(8, 8);
        var vertices = new List<Vector2>();
        var triangles = new List<int>();

        void AddQuad(Vector2Int position, Vector2Int subdivisions) {
            var stepX = 1f / subdivisions.x;
            var stepY = 1f / subdivisions.y;
            for (var y = 0; y < subdivisions.y; y++)
            for (var x = 0; x < subdivisions.x; x++) {
                var a = position - Vector2.one / 2 + new Vector2(x * stepX, y * stepY);
                var b = a + new Vector2(0, stepY);
                var c = a + new Vector2(stepX, stepY);
                var d = a + new Vector2(stepX, 0);
                var first = vertices.Count;
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(c);
                vertices.Add(d);
                triangles.Add(first + 0);
                triangles.Add(first + 1);
                triangles.Add(first + 2);
                triangles.Add(first + 0);
                triangles.Add(first + 2);
                triangles.Add(first + 3);
            }
        }

        var border = new HashSet<Vector2Int>();
        foreach (var position in positions)
            for (var y = -1; y <= 1; y++)
            for (var x = -1; x <= 1; x++)
                border.Add(position + new Vector2Int(x, y));
        border.ExceptWith(positions);

        foreach (var position in positions)
            AddQuad(position, Vector2Int.one);

        foreach (var position in border)
            AddQuad(position, Vector2Int.one * subdivisions);

        Vector2 Abs(Vector2 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y));
        float SDFBox(Vector2 p, Vector2 size) {
            Vector2 d = Abs(p) - size;
            float result = Vector2.Max(d, Vector2.zero).magnitude;
            result += Mathf.Min(Mathf.Max(d.x, d.y), 0.0f);
            return result;
        }

        var colors = vertices.Select(_ => Color.white).ToArray();
        for (var i = 0; i < vertices.Count; i++) {
            var p = vertices[i];
            var minD = float.MaxValue;
            var pos = p.RoundToInt();
            for (var y = -1; y <= 1; y++)
            for (var x = -1; x <= 1; x++) {
                var position = pos + new Vector2Int(x, y);
                if (positions.Contains(position)) {
                    var d = SDFBox(p - position, Vector2.one / 2);
                    minD = Mathf.Min(minD, d);
                }
            }
            colors[i] = Color.white * minD;
        }

        var mesh = new Mesh {
            vertices = vertices.Select(v => v.ToVector3()).ToArray(),
            colors = colors,
            triangles = triangles.ToArray(),
        };
        mesh.RecalculateBounds();

        filter.sharedMesh = mesh;
        renderer.enabled = true;
    }

    public void Hide() {
        filter.sharedMesh = null;
        renderer.enabled = false;
    }
}