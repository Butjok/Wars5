using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using static UnityEngine.Vector2;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TilemapAreaMesh : MonoBehaviour {

    public static readonly Vector2Int[] cornerOffsets = { new(-1, -1), new(1, -1), new(1, 1), new(-1, 1) };

    public MeshFilter filter;
    public MeshRenderer renderer;

    public HashSet<Vector2Int> set = new();
    public HashSet<Vector2Int> positions = new();
    public HashSet<Vector2Int> corners = new();

    public List<Vector2> vertices = new();
    public List<int> triangles = new();
    public List<Color> colors = new();
    public Mesh mesh;

    [Range(0.5f, 1)] public float t = .5f;

    public void AddTriangle(Vector2 a, Vector2 b, Vector2 c, Color colorA, Color colorB, Color colorC) {
        if ((b - a).Cross(c - b) > 0) {
            (b, c) = (c, b);
            (colorB, colorC) = (colorC, colorB);
        }

        var index = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        colors.Add(colorA);
        colors.Add(colorB);
        colors.Add(colorC);
    }
    public void AddQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color colorA, Color colorB, Color colorC, Color colorD) {
        AddTriangle(a, b, c, colorA, colorB, colorC);
        AddTriangle(a, c, d, colorA, colorC, colorD);
    }
    public void ClearMeshData() {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
    }
    public Mesh CreateMesh() {
        return new Mesh {
            vertices = vertices.Select(v => new Vector3(v.x, 0, v.y)).ToArray(),
            triangles = triangles.ToArray(),
            colors = colors.ToArray()
        };
    }

    public void Reset() {
        filter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
    }

    public void Rebuild(IEnumerable<Vector2Int> _positions) {
        set.Clear();
        set.UnionWith(_positions);

        corners.Clear();
        foreach (var position in set)
        foreach (var offset in cornerOffsets)
            corners.Add(position + offset);

        ClearMeshData();

        foreach (var position in set) {
            Vector2 ToWorld(Vector2 localPoint) {
                return (position + localPoint) / 2;
            }

            AddQuad(
                ToWorld(cornerOffsets[0]),
                ToWorld(cornerOffsets[1]),
                ToWorld(cornerOffsets[2]),
                ToWorld(cornerOffsets[3]),
                Color.white, Color.white, Color.white, Color.white);
        }

        foreach (var corner in corners) {
            Vector2 ToWorld(Vector2 localPoint) {
                return (corner + localPoint) / 2;
            }

            void AddCorner(Vector2Int pos, float t) {
                var posOpposite = -pos;
                var posOppositeVertical = new Vector2Int(pos.x, posOpposite.y);
                var posOppositeHorizontal = new Vector2Int(posOpposite.x, pos.y);

                AddQuad(
                    ToWorld(Lerp(pos, posOppositeHorizontal, .5f)),
                    ToWorld(Lerp(pos, posOppositeHorizontal, t)),
                    ToWorld(Lerp(pos, posOpposite, t)),
                    ToWorld(Lerp(pos, posOpposite, .5f)),
                    Color.white,
                    Color.clear,
                    Color.clear,
                    Color.white);

                AddQuad(
                    ToWorld(Lerp(pos, posOppositeVertical, .5f)),
                    ToWorld(Lerp(pos, posOppositeVertical, t)),
                    ToWorld(Lerp(pos, posOpposite, t)),
                    ToWorld(Lerp(pos, posOpposite, .5f)),
                    Color.white,
                    Color.clear,
                    Color.clear,
                    Color.white);
            }

            var count = cornerOffsets.Count(p => set.Contains(corner + p));
            switch (count) {
                case 1: {
                    var pos = cornerOffsets.First(p => set.Contains(corner + p));
                    AddCorner(pos, t);
                    break;
                }
                case 2: {
                    var pos = cornerOffsets.Where(p => set.Contains(corner + p)).ToArray();
                    var a = pos[0];
                    var b = pos[1];
                    // on one side
                    if ((a - b).ManhattanLength() == 2) {
                        var aOpposite = -b;
                        var bOpposite = -a;
                        var aStart = Lerp(a, aOpposite, .5f);
                        var bStart = Lerp(b, bOpposite, .5f);
                        var aEnd = Lerp(a, aOpposite, t);
                        var bEnd = Lerp(b, bOpposite, t);
                        AddQuad(
                            ToWorld(aStart), ToWorld(aEnd), ToWorld(bEnd), ToWorld(bStart),
                            Color.white, Color.clear, Color.clear, Color.white);
                    }
                    // opposite sides
                    else {
                        AddCorner(a, t);
                        AddCorner(b, t);
                    }

                    break;
                }
                case 3: {
                    var localEmptyCorner = cornerOffsets.First(p => !set.Contains(corner + p));
                    AddCorner(localEmptyCorner, 1 - t);
                    break;
                }
            }
        }

        mesh = CreateMesh();
    }

    public void Update() {
        var left = Input.GetMouseButton(Mouse.left);
        var right = Input.GetMouseButton(Mouse.right);
        if ((left || right) && Camera.main.TryRaycastPlane(out var hit)) {
            var position = hit.ToVector2Int() * 2;
            var wadModified = left ? positions.Add(position) : positions.Remove(position);
            if (wadModified)
                Rebuild(positions);
        }

        if (mesh && filter)
            filter.sharedMesh = mesh;
    }

    public void Start() {
        Rebuild(new Vector2Int[] { new(0, 0), new(2, 0), new(2, 2) });
    }

    public bool Visible {
        set => renderer.enabled = value;
    }
}