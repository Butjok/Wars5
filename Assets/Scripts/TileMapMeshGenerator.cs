using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter))]
public class TileMapMeshGenerator : MonoBehaviour {

    public MeshFilter meshFilter;

    public void Reset() {
        meshFilter = GetComponent<MeshFilter>();
        Assert.IsTrue(meshFilter);
    }

    public List<Vector2Int> points = new();
    [Command]
    public void Clear() {
        points.Clear();
        meshFilter.sharedMesh = null;
    }
    [Command]
    public void AddPoint(Vector2Int point) {
        points.Add(point);
    }

    public Material material;
    public bool UseAttackColor {
        set => material.SetFloat("_AttackAmount", value ? 1 : 0);
    }

    public void Rebuild(IEnumerable<Vector2Int> input) {
        var positions = input.ToHashSet();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var colors = new List<Color>();
        foreach (var position in positions) {
            bool IsCornerFullyEnclosedTo(Vector2Int direction) {
                for (var xOffset = 0; xOffset <= 1; xOffset++)
                for (var yOffset = 0; yOffset <= 1; yOffset++)
                    if (!positions.Contains(position + direction * new Vector2Int(xOffset, yOffset)))
                        return false;
                return true;
            }

            var center = position.ToVector3();
            var right = position.ToVector3() + Vector2Int.right.ToVector3() / 2;
            var topRight = position.ToVector3() + new Vector2(1, 1).ToVector3() / 2;
            var top = position.ToVector3() + Vector2Int.up.ToVector3() / 2;
            var topLeft = position.ToVector3() + new Vector2(-1, 1).ToVector3() / 2;
            var left = position.ToVector3() + Vector2Int.left.ToVector3() / 2;
            var bottomLeft = position.ToVector3() + new Vector2(-1, -1).ToVector3() / 2;
            var bottom = position.ToVector3() + Vector2Int.down.ToVector3() / 2;
            var bottomRight = position.ToVector3() + new Vector2(1, -1).ToVector3() / 2;

            var centerVertexIndex = vertices.Count;
            vertices.Add(center);
            colors.Add(Color.black);
            vertices.Add(right);
            colors.Add(positions.Contains(position + Vector2Int.right) ? Color.black : Color.white);
            vertices.Add(topRight);
            colors.Add(IsCornerFullyEnclosedTo(new Vector2Int(1, 1)) ? Color.black : Color.white);
            vertices.Add(top);
            colors.Add(positions.Contains(position + Vector2Int.up) ? Color.black : Color.white);
            vertices.Add(topLeft);
            colors.Add(IsCornerFullyEnclosedTo(new Vector2Int(-1, 1)) ? Color.black : Color.white);
            vertices.Add(left);
            colors.Add(positions.Contains(position + Vector2Int.left) ? Color.black : Color.white);
            vertices.Add(bottomLeft);
            colors.Add(IsCornerFullyEnclosedTo(new Vector2Int(-1, -1)) ? Color.black : Color.white);
            vertices.Add(bottom);
            colors.Add(positions.Contains(position + Vector2Int.down) ? Color.black : Color.white);
            vertices.Add(bottomRight);
            colors.Add(IsCornerFullyEnclosedTo(new Vector2Int(1, -1)) ? Color.black : Color.white);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 2);
            triangles.Add(centerVertexIndex + 1);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 3);
            triangles.Add(centerVertexIndex + 2);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 4);
            triangles.Add(centerVertexIndex + 3);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 5);
            triangles.Add(centerVertexIndex + 4);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 6);
            triangles.Add(centerVertexIndex + 5);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 7);
            triangles.Add(centerVertexIndex + 6);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 8);
            triangles.Add(centerVertexIndex + 7);

            triangles.Add(centerVertexIndex + 0);
            triangles.Add(centerVertexIndex + 1);
            triangles.Add(centerVertexIndex + 8);
        }

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }
}