using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using static MarchingSquares;

public class TerrainCreator2 : MonoBehaviour {

    public const string fileName = "NewTerrain";
    public Camera camera;
    public VoronoiRenderer voronoiRenderer;

    // [Command]
    // public void Save() {
    //     var stringWriter = new StringWriter();
    //     foreach (var (position, height) in corners)
    //         stringWriter.PostfixWriteLine("add-corner ( {0} {1} )", position, height);
    //     LevelEditorFileSystem.Save(fileName, stringWriter.ToString());
    // }
    //
    // [Command]
    // public bool TryLoad() {
    //
    //     var text = LevelEditorFileSystem.TryReadLatest(fileName);
    //     if (text == null)
    //         return false;
    //
    //     var stack = new Stack();
    //     corners.Clear();
    //     foreach (var token in Tokenizer.Tokenize(text.ToPostfix()))
    //         switch (token) {
    //             case "add-corner":
    //                 var height = (int)stack.Pop();
    //                 var position = (Vector2Int)stack.Pop();
    //                 corners.Add(position, height);
    //                 break;
    //             default:
    //                 stack.ExecuteToken(token);
    //                 break;
    //         }
    //
    //     RebuildMesh();
    //
    //     return true;
    // }
    //
    // public void Start() {
    //     TryLoad();
    // }
    // private void OnApplicationQuit() {
    //     Save();
    // }

    public Dictionary<Vector2Int, TileType> tiles = new();
    public TileType tileType = TileType.Plain;
    public List<TileType> tileTypes = new() {TileType.Plain ,TileType.Road,TileType.Mountain,TileType.Forest,TileType.River};
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
            if (Input.GetMouseButton(Mouse.right)) {
                if (tiles.ContainsKey(position)) {
                    tiles.Remove(position);
                    RebuildMesh();
                }
            }
            else if (Input.GetMouseButton(Mouse.left)) {
                if (!tiles.TryGetValue(position, out var currentTileType) || currentTileType != tileType) {
                    tiles[position] = tileType;
                    RebuildMesh();
                }
            }
        }

        // if (corners.Count > 0) {
        //     var minHeight = corners.Values.Min();
        //     var maxHeight = corners.Values.Max();
        //     var range = maxHeight - minHeight;
        //     foreach (var (position, height) in corners) {
        //         var hue = range == 0 ? pointColorHueMax : Mathf.Lerp(pointColorHueMin, pointColorHueMax, (height - minHeight) / (float)range);
        //         var color = Color.HSVToRGB(hue, 1, 1);
        //         Draw.ingame.SolidCircle(position.ToVector3(), Vector3.up, pointRadius, color);
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.Tab))
            tileType = tileTypes[(tileTypes.IndexOf(tileType) + 1) % tileTypes.Count];

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
        foreach (var square in EnumerateSquares(tiles.Keys.Range(), (square, corner) => tiles.TryGetValue(square + corner, out var t) && t is TileType.Plain or TileType.River)) {

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
        if (meshBuilder.vertices.Count > 0) {
            var min = new Vector2(
                meshBuilder.vertices.Min(x => x.x),
                meshBuilder.vertices.Min(x => x.z)
            );
            meshBuilder.uv[0] = new List<Vector2>();
            foreach (var vertex in meshBuilder.vertices)
                meshBuilder.uv[0].Add(vertex.ToVector2() - min);
        }
        meshBuilder.PopulateMesh(ref shoreMesh);

        // var bounds = new RectInt {
        //     xMin = corners.Keys.Min(x => x.x),
        //     xMax = corners.Keys.Max(x => x.x),
        //     yMin = corners.Keys.Min(x => x.y),
        //     yMax = corners.Keys.Max(x => x.y)
        // };
        // var size = bounds.size + Vector2Int.one;
        //
        // if (voronoiRenderer && corners.Count > 0) {
        //     voronoiRenderer.worldSize = size;
        //     voronoiRenderer.Render2(size, voronoiRenderer.pixelsPerUnit);
        //     voronoiRenderer.terrainMaterial.SetVector("_Splat2Size", (Vector2)size);
        // }
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        foreach (var tileType in tileTypes)
            GUILayout.Label(this.tileType == tileType ? $"[{tileType}]" : tileType.ToString());
        //GUILayout.Label($"Height: {height}");
    }
}