using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class InstancedMeshRendererLod : MonoBehaviour {

    public Mesh[] lods = { };
    public Material material;
    public List<Vector2> points = new();
    public float radius = .1f;
    public Vector2 cellSize = new(1, 1);
    public int lodsCount = 3;
    public Vector2Int indexRange = new Vector2Int();
    public Transform target;
    public float distanceThreshold = 1;

    public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;

    public int[] triangleCount = { 800, 200, 100 };

    [Command]
    public void PopulatePoints(Vector2 min, Vector2 max, int count) {
        points.Clear();
        for (var i = 0; i < count; i++)
            points.Add(new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y)));
    }

    public class Cell {
        public int startPointIndex, pointsCount;
        public Cell parent;
        public Cell[] children;
        public Vector2 min, max;
        public Vector2 Size => max - min;
        public Vector2 Center => min + Size / 2;
        public List<Vector2> points;
        public bool Contains(Vector2 point) {
            return point.x >= min.x && point.x <= max.x &&
                   point.y >= min.y && point.y <= max.y;
        }
        public void Subdivide(int count) {
            if (count <= 0)
                return;
            children = new Cell[4];
            for (var y = 0; y < 2; y++)
            for (var x = 0; x < 2; x++) {
                var child = children[y * 2 + x] = new Cell {
                    parent = this,
                    min = new Vector2(min.x + x * Size.x / 2, min.y + y * Size.y / 2),
                };
                child.max = child.min + Size / 2;
                child.Subdivide(count - 1);
            }
        }
        public bool TryPlace(Vector2 point) {
            if (!Contains(point))
                return false;
            if (children == null) {
                points ??= new List<Vector2>();
                points.Add(point);
                return true;
            }
            foreach (var child in children)
                if (child.TryPlace(point))
                    break;
            return true;
        }
    }

    public void OnGUI() {
        if (points.Count == 0)
            return;

        var minX = points.Min(p => p.x);
        var minY = points.Min(p => p.y);
        var maxX = points.Max(p => p.x);
        var maxY = points.Max(p => p.y);
        var size = Mathf.Max(maxX - minX, maxY - minY);
        if (size == 0)
            return;

        var cell = new Cell {
            min = new Vector2(minX, minY)
        };
        cell.max = cell.min + new Vector2(size, size);
        //var cellSubdivisionsCount = Mathf.RoundToInt(Mathf.Log(size, 2));
        cell.Subdivide(lodsCount - 1);

        foreach (var point in points)
            cell.TryPlace(point);

        var sortedPoints = new List<Vector2>();

        void AddToSorted(Cell c) {
            if (c.children == null) {
                c.startPointIndex = sortedPoints.Count;
                if (c.points != null) {
                    sortedPoints.AddRange(c.points);
                    c.pointsCount = c.points.Count;
                }
                else
                    c.pointsCount = 0;
            }
            else {
                foreach (var child in c.children)
                    AddToSorted(child);
                c.startPointIndex = c.children[0].startPointIndex;
                c.pointsCount = c.children.Sum(child => child.pointsCount);
            }
        }

        AddToSorted(cell);

        var tris = 0;

        void DrawCell(Cell cell, float subdivideThreshold) {
            var bounds = new Bounds(cell.Center, cell.Size);
            var distance = Mathf.Sqrt(bounds.SqrDistance(target.position.ToVector2()));

            if (cell.children == null || distance > subdivideThreshold) {
                var lod = 0;
                for (var th = distanceThreshold; lod < lodsCount - 1; th /= 2, lod++) {
                    if (distance > th)
                        break;
                }
                lod = lodsCount - 1 - lod;

                var color = Color.HSVToRGB((cell.min.x * .2f + cell.min.y * .1f) % 1, 1, 1);
                color = new Color(1, 1, 1);
                color /= lod + 1;
                color.a = 1;
                var center = cell.min + cell.Size / 2;
                Draw.ingame.SolidBox(center.ToVector3(), cell.Size.ToVector3(), color * new Color(1, 1, 1, .1f));
                Draw.ingame.WireBox(center.ToVector3(), cell.Size.ToVector3(), color);

                for (var i = cell.startPointIndex; i < cell.startPointIndex + cell.pointsCount; i++)
                    Draw.ingame.SolidBox(sortedPoints[i].ToVector3(), Vector3.one * .02f, color);

                tris += triangleCount[lod] * cell.pointsCount;
            }
            else if (cell.children != null)
                foreach (var child in cell.children)
                    DrawCell(child, subdivideThreshold / 2);
        }

        if (target)
            DrawCell(cell, distanceThreshold);

        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Tris: {tris}");
    }

    public static void DrawMeshViaIndirect(Mesh mesh, Material material, Matrix4x4 transform) {
        var argsBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(new[] { (int)mesh.GetIndexCount(0), 1, (int)mesh.GetIndexStart(0), (int)mesh.GetBaseVertex(0), 0 });
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer, 0, null, ShadowCastingMode.On, true);
    }
}