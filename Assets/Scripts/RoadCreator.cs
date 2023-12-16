using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Butjok.CommandLine;
using Drawing;
using Stable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class RoadCreator : MonoBehaviour {

    public const string autosaveName = "Autosave";

    public Camera camera;

    public Mesh pieceI, pieceL, pieceT, pieceX, pieceIsland, pieceCap;
    public int rotateI, rotateL, rotateT, rotateX, rotateIsland, rotateCap;

    public Mesh mesh, projectedMesh;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    public HashSet<Vector2Int> positions = new();

    public Color color = Color.grey;
    public TerrainCreator terrainCreator;
    
    public string loadOnAwake = autosaveName;

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

    

    public static readonly List<Vector3> vertices = new();
    public static readonly List<int> triangles = new();
    public static readonly List<Vector2> uvs0 = new();

    [Command]
    public void Rebuild() {

        if (positions.Count == 0) {
            meshFilter.mesh = null;
            meshCollider.sharedMesh = null;
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
            var (type, forward) = RoadTiles.DetermineTile(position, positions);
            var (mesh, actualForward) = type switch {
                RoadTiles.Type.I => (pieceI, forward.Rotate(rotateI)),
                RoadTiles.Type.L => (pieceL, forward.Rotate(rotateL)),
                RoadTiles.Type.T => (pieceT, forward.Rotate(rotateT)),
                RoadTiles.Type.X => (pieceX, forward.Rotate(rotateX)),
                RoadTiles.Type.Isolated => (pieceIsland, forward.Rotate(rotateIsland)),
                RoadTiles.Type.Cap => (pieceCap, forward.Rotate(rotateCap)),
                _ => throw new ArgumentOutOfRangeException()
            };
            var matrix = Matrix4x4.TRS(position.ToVector3(), Quaternion.LookRotation(actualForward.ToVector3(), Vector3.up), Vector3.one);
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
        meshCollider.sharedMesh = projectedMesh;
    }

    public float offset = .05f;

    public void Awake() {
        TryLoad(loadOnAwake);
        Rebuild();
    }

    [Command]
    public void Clear() {
        positions.Clear();
        Rebuild();
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Roads: {positions.Count}");
        GUILayout.Space(15);
        GUILayout.Label("[R] Rebuild");
    }

    public void Read(string input) {
        positions.Clear();
        var stack = new Stack();
        foreach (var token in Tokenizer.Tokenize(input.ToPostfix()))
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
    }

    public bool TryLoad(string name) {
        var input = LevelEditorFileSystem.TryReadLatest(name + "Roads");
        if (input == null)
            return false;
        Read(input);
        Rebuild();
        return true;
    }

    public void Save(string name) {
        var output = new StringWriter();
        foreach (var position in positions)
            output.PostfixWriteLine("add ( {0} )", position);
        LevelEditorFileSystem.Save(name + "Roads", output.ToString());
    }
}