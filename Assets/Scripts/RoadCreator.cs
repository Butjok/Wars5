using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class RoadCreator : MonoBehaviour {

    public const string autoSavePath = "Assets/RoadAutosave.save";

    public Camera camera;

    public Mesh pieceI, pieceL, pieceT, pieceX, pieceIsland, pieceCap;
    public int rotateI, rotateL, rotateT, rotateX, rotateIsland, rotateCap;

    public Mesh mesh, projectedMesh;
    public MeshFilter meshFilter;

    public HashSet<Vector2Int> positions = new();

    public Color color = Color.grey;
    public TerrainCreator terrainCreator;

    private void Reset() {
        meshFilter = GetComponent<MeshFilter>();
        Assert.IsTrue(meshFilter);
    }

    public void Update() {

        var ray = camera.FixedScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, float.MaxValue, LayerMask.GetMask("Terrain"))) {
            var position = hit.point.ToVector2Int();
            Draw.ingame.Ray(position.ToVector3() + hit.point.y * Vector3.up, hit.normal, Color.yellow);
            Draw.ingame.CircleXZ(position.ToVector3() + hit.point.y * Vector3.up, .5f, Color.yellow);

            if (Input.GetMouseButton(Mouse.left)) {
                if (!positions.Contains(position)) {
                    positions.Add(position);
                    Rebuild();
                }
            }
            else if (Input.GetMouseButton(Mouse.right)) {
                if (positions.Contains(position)) {
                    positions.Remove(position);
                    Rebuild();
                }
            }
        }

        foreach (var position in positions)
            Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, color);
        
        if (Input.GetKeyDown(KeyCode.R))
            Rebuild();
    }

    public enum RoadTileType { I, L, T, X, Island, Cap }
    public static readonly List<Vector2Int> neighbors = new();
    public static readonly Vector2Int[] offsets = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    public (RoadTileType type, Vector2Int forward) DetermineTile(Vector2Int position) {
        neighbors.Clear();
        neighbors.AddRange(positions.Where(p => (position - p).ManhattanLength() == 1).ToList());
        if (neighbors.Count == 0)
            return (RoadTileType.Island, Vector2Int.up.Rotate(rotateIsland));
        if (neighbors.Count == 1)
            return (RoadTileType.Cap, (position - neighbors[0]).Rotate(rotateCap));
        if (neighbors.Count == 4)
            return (RoadTileType.X, Vector2Int.up.Rotate(rotateX));
        if (neighbors.Count == 3) {
            var missingPosition = offsets.Select(offset => offset + position).Except(neighbors).Single();
            return (RoadTileType.T, (position - missingPosition).Rotate(rotateT));
        }
        if (neighbors.Count == 2) {
            var offset = neighbors[0] - position;
            // I
            if (neighbors.Contains(position - offset))
                return (RoadTileType.I, offset.Rotate(rotateI));
            // L
            var up = ((neighbors[0] - position).Rotate(3) == (neighbors[1] - position) ? neighbors[0] : neighbors[1]) - position;
            return (RoadTileType.L, up.Rotate(rotateL));
        }
        throw new Exception("should be unreachable");
    }

    public static readonly List<Vector3> vertices = new();
    public static readonly List<int> triangles = new();
    public static readonly List<Vector2> uvs0 = new();

    [Command]
    public void Rebuild() {

        if (positions.Count == 0) {
            meshFilter.mesh = null;
            return;
        }

        if (!mesh)
            mesh = new Mesh { name = "Roads" };
        else
            mesh.Clear();

        if (!projectedMesh)
            projectedMesh = new Mesh { name = "ProjectedRoads" };
        else
            projectedMesh.Clear();
        
        vertices.Clear();
        triangles.Clear();
        uvs0.Clear();

        foreach (var position in positions) {
            var (type, forward) = DetermineTile(position);
            var mesh = type switch {
                RoadTileType.I => pieceI,
                RoadTileType.L => pieceL,
                RoadTileType.T => pieceT,
                RoadTileType.X => pieceX,
                RoadTileType.Island => pieceIsland,
                RoadTileType.Cap => pieceCap,
                _ => throw new ArgumentOutOfRangeException()
            };
            var matrix = Matrix4x4.TRS(position.ToVector3(), Quaternion.LookRotation(forward.ToVector3(), Vector3.up), Vector3.one);
            var verticesStart = vertices.Count;
            foreach (var vertex in mesh.vertices)
                vertices.Add(matrix.MultiplyPoint(vertex));
            foreach (var uv0 in mesh.uv)
                uvs0.Add(uv0);
            foreach (var vertexIndex in mesh.triangles)
                triangles.Add(verticesStart + vertexIndex);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs0.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        MeshProjector.TryProjectDown(projectedMesh, mesh, Vector3.zero, 0, 1 << LayerMask.NameToLayer("Terrain"), offset);
        meshFilter.sharedMesh = projectedMesh;
    }

    public float offset = .05f;

    public void Awake() {
        TryLoad(autoSavePath);
        Rebuild();
    }

    private void OnApplicationQuit() {
        Save(autoSavePath);
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Roads: {positions.Count}");
        GUILayout.Space(15);
        GUILayout.Label("[R] Rebuild");
    }

    public bool TryLoad(string path) {

        if (!File.Exists(path))
            return false;

        var input = File.ReadAllText(path).ToPostfix();
        var stack = new Stack();
        foreach (var token in Tokenizer.Tokenize(input))
            switch (token) {
                case "add": {
                    positions.Add((Vector2Int)stack.Pop());
                    break;
                }
                default: {
                    stack.ExecuteToken(token);
                    break;
                }
            }

        return true;
    }

    public void Save(string path) {

        var output = new StringWriter();

        foreach (var position in positions)
            output.PostfixWriteLine("add ( {0} )", position);

        File.WriteAllText(path, output.ToString());
    }
}