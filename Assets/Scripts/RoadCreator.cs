using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Butjok.CommandLine;
using Drawing;
using Stable;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter))]
public class RoadCreator : MonoBehaviour {

    public const string defaultLoadOnAwakeFileName = "Autosave";

    public static readonly List<Vector3> vertices = new();
    public static readonly List<int> triangles = new();
    public static readonly List<Vector2> uvs0 = new();

    public bool placeBridges = false;

    [Header("Mesh pieces")]
    public Mesh pieceI;

    public Mesh pieceL, pieceT, pieceX, pieceIsland, pieceCap;

    [Header("Rotation shifts")]
    public int rotateI;

    public int rotateL, rotateT, rotateX, rotateIsland, rotateCap;

    [Header("Dependencies")]
    public Camera camera;

    public Mesh mesh, projectedMesh;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    [Header("Startup")]
    public bool loadOnAwake = true;

    public string loadOnAwakeFileName = defaultLoadOnAwakeFileName;
    public bool autoSave = true;

    [Header("Editing")]
    public float offset = .05f;

    public Color debugColor = Color.grey;
    public Dictionary<Vector2Int, TileType> tiles = new();

    public TileType[] tileTypes = { TileType.Road , TileType.Bridge, TileType.BridgeSea};
    public TileType tileType = TileType.Road;

    public void Awake() {
        if (loadOnAwake)
            TryLoad(loadOnAwakeFileName);
    }

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
                if (!tiles.TryGetValue(position, out var oldTileType) || oldTileType != tileType) {
                    tiles[position] = tileType;
                    Rebuild();
                }
            }
            else if (Input.GetMouseButton(Mouse.right)) {
                if (tiles.ContainsKey(position)) {
                    tiles.Remove(position);
                    Rebuild();
                }
            }
        }

        foreach (var position in tiles.Keys)
            Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, debugColor);

        if (Input.GetKeyDown(KeyCode.R))
            Rebuild();
        else if (Input.GetKeyDown(KeyCode.Tab)) {
            var index = Array.IndexOf(tileTypes, tileType);
            tileType = tileTypes[(index+1).PositiveModulo(tileTypes.Length)];
        }
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Road creator [{tileType}]");
        GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
        GUILayout.Label($"[{DefaultGuiSkin.leftClick}] Place roads");
        GUILayout.Label($"[{DefaultGuiSkin.rightClick}] Remove roads");
        GUILayout.Label($"[{KeyCode.R}] Rebuild");
    }

    /*private void OnApplicationQuit() {
        if (autoSave) {
            loadedPositions.SymmetricExceptWith(positions);
            if (loadedPositions.Count > 0)
                Save(loadOnAwakeFileName);
        }
    }*/

    [Command]
    public void Rebuild() {
        if (tiles.Count == 0) {
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

        foreach (var position in tiles.Keys) {
            if (!placeBridges && tiles[position] is TileType.Bridge or TileType.BridgeSea)
                continue;
            var (type, forward) = RoadTiles.DetermineTile(position, tiles.Keys);
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

        MeshProjector.TryProjectDown(projectedMesh, mesh, Vector3.zero, 0, LayerMasks.Terrain | LayerMasks.Water, offset);
        if (projectedMesh && projectedMesh.vertexCount > 0) {
            meshFilter.sharedMesh = projectedMesh;
            meshCollider.sharedMesh = projectedMesh;
        }
        else {
            meshFilter.sharedMesh = null;
            meshCollider.sharedMesh = null;
        }
    }

    [Command]
    public void Clear() {
        tiles.Clear();
        Rebuild();
    }

    public void Read(string input) {
        tiles.Clear();
        var stack = new Stack();
        foreach (var token in Tokenizer.Tokenize(input.ToPostfix()))
            switch (token) {
                case "add": {
                    tiles.Add((Vector2Int)stack.Pop(), TileType.Road);
                    break;
                }
                case "add-tile": {
                    var tileType = (TileType)stack.Pop();
                    var position = (Vector2Int)stack.Pop();
                    tiles.Add(position, tileType);
                    break;
                }
                default: {
                    stack.ExecuteToken(token);
                    break;
                }
            }
    }

    public bool TryLoad(string name) {
        var input = LevelEditorFileSystem.TryReadLatest(name);
        if (input == null)
            return false;
        Read(input);
        Rebuild();
        return true;
    }

    public void Save(string name) {
        var output = new StringWriter();
        foreach (var (position, tileType) in tiles)
            output.PostfixWriteLine("add-tile ( {0} {1} )", position, tileType);
        LevelEditorFileSystem.Save(name, output.ToString());
    }

    [Command]
    public void Save() {
        Save(loadOnAwakeFileName);
    }
}