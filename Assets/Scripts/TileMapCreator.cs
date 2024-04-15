using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Butjok.CommandLine;
using Drawing;
using Stable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using static UnityEngine.Object;

public class TileMapCreator : MonoBehaviour {

    public enum Mode {
        EditQuads,
        Placement
    }

    [Header("Placement")]
    public TileType[] tileTypes = {
        TileType.Sea,
        TileType.Plain,
        TileType.River,
        TileType.Beach,
        TileType.Mountain
    };

    public TileType tileType = default;
    public Dictionary<Vector2Int, TileType> tiles = new();

    [Header("Colors")]
    public Color colorAlpha = new(1, 1, 1, .25f);

    public Color seaColor = Color.blue;
    public Color plainColor = Color.green;
    public Color riverColor = Color.cyan;
    public Color beachColor = Color.yellow;
    public Color mountainColor = new(0.56f, 0.23f, 0f);

    [Header("Startup")]
    public Mode mode = Mode.EditQuads;

    public bool loadOnAwake = true;
    public string loadOnAwakeFileName = "TileMap";

    [Header("Dependencies")]
    public CameraRig cameraRig;

    public TerrainMapper terrainMapper;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    [Header("Quad editing")]
    public float quadOverlayHeight;

    public float lineThickness = .5f;
    public Color lineColor = Color.white;

    public List<Quad> quads = new() {
        new Quad(TileType.Sea, TileType.Sea, TileType.Sea, TileType.Plain)
    };

    [Header("Mesh pieces")]
    public Transform piecesRoot;

    public List<MeshRenderer> pieces = new();
    public List<MeshRenderer> placedPieces = new();

    [Header("Mesh postprocessing")]
    public Vector2 noiseScale = new(10, 10);
    public Vector2 noiseScale2 = new(5, 5);

    [Command] public float noiseAmplitude = 1;
    [Command] public float noiseAmplitude2 = .1f;
    [Command] public int noiseOctavesCount = 3;
    [Command] public float slopeLength = 5;

    // Buffers
    public static List<Vector3> vertexBuffer = new() { new Vector2(1, 1) / 2, new Vector2(-1, 1) / 2, new Vector2(-1, -1) / 2, new Vector2(1, -1) / 2 };
    public static List<int> triangleBuffer = new() { 0, 1, 2, 0, 2, 3 };
    public static List<Color> colorBuffer = new();

    public Action finalizeMesh;

    public void Awake() {
        if (loadOnAwake)
            TryLoad(loadOnAwakeFileName);
    }

    public void Update() {
        void ShowPieces(bool value) {
            foreach (var piece in pieces)
                piece.gameObject.SetActive(value);
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                mode = Mode.EditQuads;
                ShowPieces(true);
            }
            else if (Input.GetKeyDown(KeyCode.P)) {
                mode = Mode.Placement;
                ShowPieces(false);
            }
        }

        if (mode == Mode.EditQuads)
            foreach (var quad in quads)
                DrawQuad(quad);
    }

    public void OnEnable() {
        StartCoroutine(Loop());
    }

    public void OnDisable() {
        StopAllCoroutines();
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Tile map creator [{mode} mode]");
        GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
        if (mode == Mode.Placement) {
            GUILayout.Label("[Shift-Q] Switch to quads edit mode");
            GUILayout.Label("[Tab]     Cycle tile type");
            GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
            GUILayout.Label($"Tile type: {tileType}");
        }
        else if (mode == Mode.EditQuads) {
            GUILayout.Label("[Shift-P] Switch to placement mode");
            GUILayout.Label("[R]       Rotate selected quad");
            GUILayout.Label("[F]       Flip selected quad");
            GUILayout.Label("[Delete]  Delete selected quad");
        }
    }

    public Color GetColor(TileType value) {
        return value switch {
            TileType.Sea => seaColor,
            TileType.Plain => plainColor,
            TileType.River => riverColor,
            TileType.Beach => beachColor,
            TileType.Mountain => mountainColor,
            _ => Color.magenta
        } * colorAlpha;
    }

    [Command]
    public bool TryLoad(string fileName) {
        var text = LevelEditorFileSystem.TryReadLatest(fileName);
        if (text == null)
            return false;
        quads.Clear();
        var stack = new Stack();
        foreach (var token in Tokenizer.Tokenize(text.ToPostfix()))
            switch (token) {
                case "add-quad": {
                    var position = (Vector2)stack.Pop();
                    var bottomRight = (TileType)stack.Pop();
                    var bottomLeft = (TileType)stack.Pop();
                    var topRight = (TileType)stack.Pop();
                    var topLeft = (TileType)stack.Pop();
                    quads.Add(new Quad(topLeft, topRight, bottomLeft, bottomRight, position));
                    break;
                }
                case "set-piece-transform": {
                    // ignore
                    break;
                }
                case "set-camera-rig-transform": {
                    var dollyZoom = Convert.ToSingle(stack.Pop());
                    var pitchAngle = Convert.ToSingle(stack.Pop());
                    var rotation = Convert.ToSingle(stack.Pop());
                    var position = (Vector3)stack.Pop();
                    cameraRig.transform.position = position;
                    cameraRig.transform.rotation = Quaternion.Euler(0, rotation, 0);
                    cameraRig.PitchAngle = pitchAngle;
                    cameraRig.DollyZoom = dollyZoom;
                    break;
                }
                case "add-tile": {
                    var position = (Vector2Int)stack.Pop();
                    var tileType = (TileType)stack.Pop();
                    tiles[position] = tileType;
                    break;
                }
                default:
                    stack.ExecuteToken(token);
                    break;
            }

        RebuildPieces();
        FinalizeMesh();

        return true;
    }

    public void Save() {
        Save(loadOnAwakeFileName);
    }

    [Command]
    public void Save(string fileName) {
        var stringWriter = new StringWriter();

        foreach (var quad in quads)
            stringWriter.PostfixWriteLine("add-quad ( {0} {1} {2} {3} {4} )", quad.topLeft, quad.topRight, quad.bottomLeft, quad.bottomRight, quad.position);

        stringWriter.PostfixWriteLine("set-camera-rig-transform ( {0} {1} {2} {3} )", cameraRig.transform.position, cameraRig.transform.rotation.eulerAngles.y, cameraRig.PitchAngle, cameraRig.DollyZoom);

        foreach (var (position, tileType) in tiles)
            stringWriter.PostfixWriteLine("add-tile ( {0} {1} )", tileType, position);

        LevelEditorFileSystem.Save(fileName, stringWriter.ToString());
    }

    [Command]
    public void AddMissingInvariants() {
        var position = new Vector2(0, 0);
        var step = 1;
        var row = 5;
        var count = 0;

        IEnumerable<Quad> EnumerateWith(IReadOnlyCollection<TileType> tileTypes) {
            var result = new HashSet<Quad>();

            foreach (var a in tileTypes)
            foreach (var b in tileTypes)
            foreach (var c in tileTypes)
            foreach (var d in tileTypes) {
                var values = new[] { a, b, c, d };
                if (values.Contains(TileType.River) && values.Contains(TileType.Beach))
                    continue;

                var quad = new Quad(a, b, c, d);
                if (result.Any(q => q.HasSameInvariant(quad)))
                    continue;

                result.Add(quad);
            }

            return result;
        }

        foreach (var quad in EnumerateWith(tileTypes.Except(new[] { TileType.Mountain }).ToHashSet()).Concat(EnumerateWith(new[] { TileType.Sea, TileType.Mountain }))) {
            if (quads.Any(q => q.HasSameInvariant(quad)))
                continue;

            if (count++ % row == 0)
                position = new Vector2(0, position.y - step);
            else
                position += new Vector2(step, 0);

            quad.position = position;
            quads.Add(quad);
        }
    }

    public IEnumerator Loop() {
        while (true) {
            yield return null;

            if (cameraRig.camera.TryPhysicsRaycast(out Vector3 hitPoint) || cameraRig.camera.TryRaycastPlane(out hitPoint)) {
                var mousePosition2d = hitPoint.ToVector2();
                var tilePosition = mousePosition2d.RoundToInt();

                if (tilePosition.TryRaycast(out var hit))
                    Draw.ingame.CircleXZ(hit.point, .5f, Color.white);

                if (mode is Mode.EditQuads) {
                    if (Input.GetMouseButtonDown(Mouse.left) && mode == Mode.EditQuads) {
                        var quad = FindQuad(mousePosition2d, out var selectedQuadIndex);
                        if (quad != null)
                            while (Input.GetMouseButton(Mouse.left)
                                   && (cameraRig.camera.TryPhysicsRaycast(out hitPoint) || cameraRig.camera.TryRaycastPlane(out hitPoint))) {
                                yield return null;

                                mousePosition2d = hitPoint.ToVector2();
                                quad.position = (mousePosition2d + new Vector2(1, 1) / 2).RoundToInt() - new Vector2(1, 1) / 2;
                                if (Input.GetKeyDown(KeyCode.R)) {
                                    quad = quad.RotatedClockwise;
                                }
                                else if (Input.GetKeyDown(KeyCode.F)) {
                                    quad = quad.FlippedHorizontally;
                                }
                                else if (Input.GetKeyDown(KeyCode.Delete)) {
                                    quads.RemoveAt(selectedQuadIndex);
                                    break;
                                }

                                quads[selectedQuadIndex] = quad;

                                using (Draw.ingame.WithMatrix(QuadMatrix(quad)))
                                using (Draw.ingame.WithLineWidth(lineThickness)) {
                                    Draw.ingame.Polyline(vertexBuffer, true, lineColor);
                                }
                            }
                    }
                }

                else if (mode == Mode.Placement) {
                    if (tileType == 0) {
                        Assert.IsTrue(tileTypes.Length > 0);
                        tileType = tileTypes[0];
                    }

                    if (Input.GetKeyDown(KeyCode.Tab)) {
                        var offset = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? -1 : 1;
                        tileType = tileTypes[(Array.IndexOf(tileTypes, tileType) + offset + tileTypes.Length) % tileTypes.Length];
                    }

                    if (Input.GetMouseButton(Mouse.left)) {
                        if (!tiles.TryGetValue(tilePosition, out var type) || type != tileType) {
                            tiles[tilePosition] = tileType;
                            RebuildPieces();
                        }
                    }
                    else if (Input.GetMouseButton(Mouse.right) && tiles.ContainsKey(tilePosition)) {
                        tiles.Remove(tilePosition);
                        RebuildPieces();
                    }
                }
            }
        }
    }
    
    [Command]
    public float landDisplacement = 1;
    [Command]
    public float seaDisplacement = 3;

    [Command]
    public void RebuildPieces() {
        var materials = pieces.SelectMany(r => r.sharedMaterials).Distinct().ToList();
        var submeshCombiners = materials.ToDictionary(m => m, _ => new List<CombineInstance>());

        void AddQuadPiece(Quad quad) {
            // find quad with the same invariant in quads
            var foundQuad = quads.FirstOrDefault(q => q.HasSameInvariant(quad));
            if (foundQuad != null) {
                int TryFindRotation(Quad from, Quad to) {
                    if (from.HasSameCorners(to))
                        return 0;
                    if (from.RotatedClockwise.HasSameCorners(to))
                        return 1;
                    if (from.RotatedClockwise.RotatedClockwise.HasSameCorners(to))
                        return 2;
                    if (from.RotatedClockwise.RotatedClockwise.RotatedClockwise.HasSameCorners(to))
                        return 3;
                    return -1;
                }

                var flipped = false;
                var rotation = TryFindRotation(foundQuad, quad);
                if (rotation == -1) {
                    rotation = TryFindRotation(foundQuad.FlippedHorizontally, quad);
                    flipped = true;
                    Assert.IsTrue(rotation != -1);
                }

                // find piece in the same location as found quad
                var piece = pieces.FirstOrDefault(piece => Vector2.Distance(piece.transform.position.ToVector2(), foundQuad.position) < .1f);
                if (piece) {
                    /*var copy = Instantiate(piece);
                    copy.transform.localScale = new Vector3(flipped ? -1 : 1, 1, 1);
                    copy.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);
                    copy.transform.position = quad.position.ToVector3();
                    copy.gameObject.SetActive(true);
                    placedPieces.Add(copy);*/

                    var matrix = Matrix4x4.TRS(quad.position.ToVector3(), Quaternion.Euler(0, rotation * 90, 0), new Vector3(flipped ? -1 : 1, 1, 1));
                    var mesh = piece.GetComponent<MeshFilter>().sharedMesh;
                    for (var submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
                        submeshCombiners[piece.sharedMaterials[submeshIndex]].Add(new CombineInstance {
                            mesh = mesh,
                            subMeshIndex = submeshIndex,
                            transform = matrix
                        });
                }
                else {
//                    Debug.Log($"No piece found for {foundQuad}, quad position: {foundQuad.position}");
                }
            }
            else {
                Debug.Log($"No quad found for {quad}");
            }
        }

        //-------------

        foreach (var piece in placedPieces)
            Destroy(piece.gameObject);
        placedPieces.Clear();

        if (tiles.Count == 0)
            return;

        var minX = tiles.Keys.Min(p => p.x);
        var maxX = tiles.Keys.Max(p => p.x);
        var minY = tiles.Keys.Min(p => p.y);
        var maxY = tiles.Keys.Max(p => p.y);

        for (var x = minX - .5f; x <= maxX + .5f; x++)
        for (var y = minY - .5f; y <= maxY + .5f; y++) {
            var cornerPosition = new Vector2(x, y);

            TileType GetTileType(int xOffset, int yOffset) {
                var tilePosition = new Vector2(cornerPosition.x + xOffset * .5f, cornerPosition.y + yOffset * .5f).RoundToInt();
                return tiles.TryGetValue(tilePosition, out var result)
                    ? (TileType.Buildings & result) != 0
                        ? TileType.Plain
                        : result switch {
                            TileType.Road or TileType.Forest => TileType.Plain,
                            TileType.Bridge => TileType.River,
                            TileType.BridgeSea => TileType.Sea,
                            _ => result
                        }
                    : TileType.Sea;
            }

            TileType ToGroundTileType(TileType tileType) {
                return tileType == TileType.Mountain ? TileType.Plain : tileType;
            }

            TileType ToMountainTileType(TileType tileType) {
                return tileType == TileType.Mountain ? tileType : TileType.Sea;
            }

            var groundQuad = new Quad(
                ToGroundTileType(GetTileType(-1, 1)), ToGroundTileType(GetTileType(1, 1)),
                ToGroundTileType(GetTileType(-1, -1)), ToGroundTileType(GetTileType(1, -1)),
                cornerPosition
            );
            AddQuadPiece(groundQuad);

            var mountainQuad = new Quad(
                ToMountainTileType(GetTileType(-1, 1)), ToMountainTileType(GetTileType(1, 1)),
                ToMountainTileType(GetTileType(-1, -1)), ToMountainTileType(GetTileType(1, -1)),
                cornerPosition
            );
            if (mountainQuad.Any(t => t == TileType.Mountain))
                AddQuadPiece(mountainQuad);
        }

        if (meshFilter.sharedMesh) {
            Destroy(meshFilter.sharedMesh);
            meshFilter.sharedMesh = null;
        }

        //
        // COMBINING PIECES INTO A SINGLE MESH
        //

        var finalMaterials = new List<Material>();

        var submeshes = new List<Mesh>();
        foreach (var material in materials) {
            var submesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            if (submeshCombiners[material].Count > 0) {
                submesh.CombineMeshes(submeshCombiners[material].ToArray());
                submeshes.Add(submesh);
                finalMaterials.Add(material);
            }
        }

        var combinedMesh = new Mesh();
        combinedMesh.indexFormat = IndexFormat.UInt32;
        combinedMesh.CombineMeshes(submeshes.Select(mesh => new CombineInstance { mesh = mesh }).ToArray(), false, false);

        //
        // DISPLACEMENT
        // 

        finalizeMesh = () => {
            var vertices = combinedMesh.vertices;
            var uvs = new Vector2[vertices.Length];
            var extendedTilePositions = tiles.Keys.GrownBy(1);
            var edgeTilePositions = extendedTilePositions.Where(p => (tiles.TryGetValue(p, out var t) ? t : TileType.Sea) is TileType.Beach or TileType.Sea or TileType.River).ToList();
            var landPositions =  extendedTilePositions.Where(p => (tiles.TryGetValue(p, out var t) ? t : TileType.Sea) != TileType.Sea).ToList();

            Parallel.For(0, vertices.Length, i => {
                var vertex2d = vertices[i].ToVector2();
                uvs[i] = vertex2d;
                uvs[i] = Vector2.zero;

                /*var tilePosition = vertex2d.RoundToInt();
                if (!tiles.TryGetValue(tilePosition, out var t) || t == TileType.Sea)
                    return;*/

                var distanceToSea = edgeTilePositions.Aggregate<Vector2Int, float>(9999, (current, position) => Mathf.Min(current, (vertex2d - position).SignedDistanceBox(.5f.ToVector2())));
                var distanceToLand = landPositions.Aggregate<Vector2Int, float>(9999, (current, position) => Mathf.Min(current, (vertex2d - position).SignedDistanceBox(.5f.ToVector2())));

                Displace(distanceToSea, Vector3.up * landDisplacement, noiseAmplitude, noiseOctavesCount);
                //Displace(distanceToLand, Vector3.down * seaDisplacement, noiseAmplitude/2, noiseOctavesCount/2);
                
                void Displace(float distance, Vector3 offset, float noiseAmplitude, int noiseOctavesCount) {
                    var displacementMask = Mathf.Clamp01(distance / slopeLength);
                    if (displacementMask < Mathf.Epsilon)
                        return;

                    var displacementAmount = 0f;
                    var noiseScale = this.noiseScale;
                    for (var j = 0; j < noiseOctavesCount; j++) {
                        displacementAmount += Mathf.PerlinNoise(vertex2d.x / noiseScale.x, vertex2d.y / noiseScale.y) * noiseAmplitude;
                        noiseScale /= 2;
                        noiseAmplitude /= 2;
                    }

                    vertices[i] += offset * (displacementMask * (displacementAmount + Mathf.PerlinNoise(vertex2d.x / noiseScale2.x, vertex2d.y / noiseScale2.y) * .2f));    
                }
                
                {
                    var displacementMask = Mathf.Clamp01(distanceToLand / slopeLength);
                    if (displacementMask < Mathf.Epsilon)
                        return;

                    vertices[i] += Vector3.down * (displacementMask);
                    //vertices[i] += Vector3.down *  Mathf.Clamp01(distanceToLand / slopeLength)*Mathf.PerlinNoise(vertex2d.x / noiseScale2.x, vertex2d.y / noiseScale2.y) * .2f; 
                }
            });

            combinedMesh.vertices = vertices;
            combinedMesh.uv = uvs;
            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            // smoothen normals

            var normals = combinedMesh.normals;
            var newNormals = new Vector3[normals.Length];
            Parallel.For(0, normals.Length, i => {
                var accumulator = Vector3.zero;
                var count = 0;
                for (var j = 0; j < normals.Length; j++)
                    if (Vector3.Distance(vertices[i], vertices[j]) < .0001f) {
                        accumulator += normals[j];
                        count++;
                    }

                newNormals[i] = accumulator / count;
            });
            combinedMesh.normals = newNormals;

            combinedMesh.RecalculateTangents();

            meshFilter.sharedMesh = combinedMesh;
            meshRenderer.sharedMaterials = finalMaterials.ToArray();
            meshCollider.sharedMesh = combinedMesh;
        };

        meshFilter.sharedMesh = combinedMesh;
        meshRenderer.sharedMaterials = finalMaterials.ToArray();
        meshCollider.sharedMesh = combinedMesh;

        if (terrainMapper)
            terrainMapper.transform.position = new Vector2(minX - 1, minY - 1).ToVector3();
    }

    [Command]
    public void FinalizeMesh() {
        finalizeMesh?.Invoke();
        finalizeMesh = null;
    }

    public Quad FindQuad(Vector2 position, out int index) {
        var cornerOffsets = new[] {
            new Vector2(1, 1) / 2,
            new Vector2(-1, 1) / 2,
            new Vector2(-1, -1) / 2,
            new Vector2(1, -1) / 2
        };
        for (var i = 0; i < quads.Count; i++) {
            var quad = quads[i];
            var corners = cornerOffsets.Select(corner => quad.position + corner).ToList();
            var rect = new Rect {
                xMin = corners.Min(p => p.x),
                xMax = corners.Max(p => p.x),
                yMin = corners.Min(p => p.y),
                yMax = corners.Max(p => p.y)
            };
            if (rect.Contains(position)) {
                index = i;
                return quad;
            }
        }

        index = -1;
        return null;
    }

    [Command]
    [ContextMenu(nameof(ParsePieces))]
    public void ParsePieces() {
        if (piecesRoot) {
            pieces.Clear();
            pieces.AddRange(piecesRoot.GetComponentsInChildren<MeshRenderer>());
        }
    }

    public MeshRenderer FindPiece(Vector2 position) {
        foreach (var piece in pieces) {
            var bounds = piece.bounds;
            var min = bounds.min.ToVector2();
            var max = bounds.max.ToVector2();
            var rect = new Rect {
                xMin = min.x,
                xMax = max.x,
                yMin = min.y,
                yMax = max.y
            };
            if (rect.Contains(position))
                return piece;
        }

        return null;
    }

    public Matrix4x4 QuadMatrix(Quad quad) {
        return Matrix4x4.TRS(quad.position.ToVector3() + Vector3.up * quadOverlayHeight, Quaternion.Euler(90, 0, 0), Vector3.one);
    }

    public void DrawQuad(Quad quad) {
        colorBuffer.Clear();
        if (quads.Count(q => q.position == quad.position) > 1) {
            colorBuffer.AddRange(Enumerable.Repeat(Color.red, 4));
        }
        else {
            colorBuffer.Add(GetColor(quad[1, 1]));
            colorBuffer.Add(GetColor(quad[-1, 1]));
            colorBuffer.Add(GetColor(quad[-1, -1]));
            colorBuffer.Add(GetColor(quad[1, -1]));
        }

        using (Draw.ingame.WithMatrix(QuadMatrix(quad))) {
            Draw.ingame.SolidMesh(vertexBuffer, triangleBuffer, colorBuffer);
        }

        // using (Draw.ingame.WithLineWidth(lineThickness))
        // using (Draw.ingame.WithColor(lineColor)) {
        //     Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(1, 1) / 2), Quaternion.identity, (Vector3)Vector2.one);
        //     Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(-1, 1) / 2), Quaternion.identity, (Vector3)Vector2.one);
        //     Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(1, -1) / 2), Quaternion.identity, (Vector3)Vector2.one);
        //     Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(-1, -1) / 2), Quaternion.identity, (Vector3)Vector2.one);
        // }

        // if () {
        //     colorBuffer[0] = colorBuffer[1] = colorBuffer[2] = colorBuffer[3] = Color.red;
        //     Draw.ingame.SolidMesh(vertexBuffer, triangleBuffer, colorBuffer);
        // }
    }

    [Serializable]
    public class Quad : IEnumerable<TileType> {
        public TileType topRight, topLeft, bottomLeft, bottomRight;
        public Vector2 position;

        public Quad(TileType topLeft = default, TileType topRight = default, TileType bottomLeft = default, TileType bottomRight = default, Vector2 position = default) {
            this.topRight = topRight;
            this.topLeft = topLeft;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
            this.position = position;
        }

        public TileType this[int x, int y] => (x, y) switch {
            (1, 1) => topRight,
            (-1, 1) => topLeft,
            (-1, -1) => bottomLeft,
            (1, -1) => bottomRight,
            _ => throw new ArgumentOutOfRangeException()
        };

        public Quad FlippedHorizontally => new(
            this[1, 1], this[-1, 1],
            this[1, -1], this[-1, -1],
            position
        );

        public Quad RotatedClockwise => new(
            this[-1, -1], this[-1, 1],
            this[1, -1], this[1, 1],
            position
        );

        public IEnumerable<Quad> AllFlips {
            get {
                yield return this;
                yield return FlippedHorizontally;
            }
        }

        public IEnumerable<Quad> AllRotations {
            get {
                yield return this;
                yield return RotatedClockwise;
                yield return RotatedClockwise.RotatedClockwise;
                yield return RotatedClockwise.RotatedClockwise.RotatedClockwise;
            }
        }

        public IEnumerable<Quad> AllFlipsAndRotations {
            get {
                foreach (var flipped in AllFlips)
                foreach (var rotated in flipped.AllRotations)
                    yield return rotated;
            }
        }

        public IEnumerator<TileType> GetEnumerator() {
            yield return topRight;
            yield return topLeft;
            yield return bottomLeft;
            yield return bottomRight;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool HasSameCorners(Quad other) {
            return topRight == other.topRight && topLeft == other.topLeft && bottomLeft == other.bottomLeft && bottomRight == other.bottomRight;
        }

        public bool HasSameInvariant(Quad other) {
            return AllFlipsAndRotations.Any(modified => modified.HasSameCorners(other));
        }

        public override string ToString() {
            return $"{topLeft} {topRight} {bottomLeft} {bottomRight}";
        }
    }
}

public static class MeshCombiner {
    public static Mesh CombineInstances(this Mesh mesh, IReadOnlyList<Matrix4x4> transforms) {
        var submeshes = new  List<Mesh>();
        for (var i = 0; i < mesh.subMeshCount; i++) {
            var combineInstances = transforms.Select(transform => new CombineInstance {
                mesh = mesh,
                subMeshIndex = i,
                transform = transform
            }).ToArray();
            var submesh = new Mesh();
            submesh.indexFormat = IndexFormat.UInt32;
            submesh.CombineMeshes(combineInstances);
            submeshes.Add(submesh);
        }
        
        var combinedMesh = new Mesh();
        combinedMesh.indexFormat = IndexFormat.UInt32;
        combinedMesh.CombineMeshes(submeshes.Select(submesh => new CombineInstance { mesh = submesh }).ToArray(), false, false);
        return combinedMesh;
    }
}