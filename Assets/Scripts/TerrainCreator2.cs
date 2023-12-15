using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Stable;
using UnityEngine;
using static MarchingSquares;

public class TerrainCreator2 : MonoBehaviour {

    public const string fileName = "NewTerrain";
    public Camera camera;

    [Command]
    public void Save() {
        var stringWriter = new StringWriter();
        foreach (var (position, height) in corners)
            stringWriter.PostfixWriteLine("add-corner ( {0} {1} )", position, height);
        LevelEditorFileSystem.Save(fileName, stringWriter.ToString());
    }

    [Command]
    public bool TryLoad() {

        var text = LevelEditorFileSystem.TryReadLatest(fileName);
        if (text == null)
            return false;

        var stack = new Stack();
        corners.Clear();
        foreach (var token in Tokenizer.Tokenize(text.ToPostfix()))
            switch (token) {
                case "add-corner":
                    var height = (int)stack.Pop();
                    var position = (Vector2Int)stack.Pop();
                    corners.Add(position, height);
                    break;
                default:
                    stack.ExecuteToken(token);
                    break;
            }

        RebuildMesh();

        return true;
    }

    public void Start() {
        TryLoad();
    }
    private void OnApplicationQuit() {
        Save();
    }

    public Dictionary<Vector2Int, int> corners = new();
    public int height = 1;
    public float pointRadius = .1f;
    public float pointColorHueMin = 0;
    public float pointColorHueMax = 1;
    public float labelSize = 1;
    public SquareSet shoreSquareSet = new();
    public Color shoreColor = Color.green;
    public Color mountainColor = new(0.52f, 0.41f, 0.28f);

    public void Update() {

        var ray = camera.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out var enter)) {
            var position = ray.GetPoint(enter).ToVector2Int();
            Draw.ingame.Circle(position.ToVector3(), Vector3.up, pointRadius * 2, Color.yellow);
            if (Input.GetMouseButton(Mouse.right) || height == 0) {
                if (corners.ContainsKey(position)) {
                    corners.Remove(position);
                    RebuildMesh();
                }
            }
            else if (Input.GetMouseButton(Mouse.left)) {
                if (!corners.TryGetValue(position, out var currentHeight) || currentHeight != height) {
                    corners[position] = height;
                    RebuildMesh();
                }
            }
        }

        if (corners.Count > 0) {
            var minHeight = corners.Values.Min();
            var maxHeight = corners.Values.Max();
            var range = maxHeight - minHeight;
            foreach (var (position, height) in corners) {
                var hue = range == 0 ? pointColorHueMax : Mathf.Lerp(pointColorHueMin, pointColorHueMax, (height - minHeight) / (float)range);
                var color = Color.HSVToRGB(hue, 1, 1);
                Draw.ingame.SolidCircle(position.ToVector3(), Vector3.up, pointRadius, color);
            }
        }

        if (Input.GetKeyDown(KeyCode.Equals))
            height++;
        else if (Input.GetKeyDown(KeyCode.Minus))
            height--;

        // DrawSquares(EnumerateSquares(corners, .5f), shoreSquareSet, shoreColor);
        // DrawSquares(EnumerateSquares(corners, 1.5f), shoreSquareSet, mountainColor);
    }

    public MeshFilter shoreMeshFilter;
    public MeshCollider shoreMeshCollider;
    public Mesh shoreMesh;

    public void RebuildMesh() {
        if (!shoreMesh) {
            shoreMesh = new Mesh { name = "Shore" };
            shoreMeshFilter.sharedMesh = shoreMesh;
            shoreMeshCollider.sharedMesh = shoreMesh;
        }
        shoreMesh.Clear();

        var meshBuilder = new ProceduralMeshBuilder();
        foreach (var square in EnumerateSquares(corners, .5f)) {

            void AppendMesh(int rotation) {
                var position = square.position.ToVector3();
                var squareMesh = shoreSquareSet[square.type];
                var rotationOffset = shoreSquareSet.GetRotationOffset(square.type);
                var matrix = Matrix4x4.TRS(position, (rotation + rotationOffset).ToQuaternion(), Vector3.one);
                meshBuilder.AppendVertices(squareMesh, matrix);
            }

            AppendMesh(square.rotation);
            if (square.rotation2 is { } actualRotation2)
                AppendMesh(actualRotation2);
        }
        meshBuilder.PopulateMesh(ref shoreMesh);
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Height: {height}");
    }
}