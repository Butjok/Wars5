using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Stable;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Assertions;

public class TileMapPacker : MonoBehaviour {

    public TileType[] tileTypes = {
        TileType.Sea,
        TileType.Plain,
        TileType.River,
        TileType.Beach
    };
    public Color seaColor = Color.blue;
    public Color plainColor = Color.green;
    public Color riverColor = Color.cyan;
    public Color beachColor = Color.yellow;
    public Color GetColor(TileType value) {
        return value switch {
            TileType.Sea => seaColor,
            TileType.Plain => plainColor,
            TileType.River => riverColor,
            TileType.Beach => beachColor,
            _ => Color.magenta
        } * colorAlpha;
    }
    public Color colorAlpha = new(1, 1, 1, .25f);

    public string fileName = "TileMap";
    public CameraRig cameraRig;

    [Command]
    public bool TryLoad() {
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
                    /*var localScaleX = Convert.ToSingle(stack.Pop());
                    var rotationY = Convert.ToSingle(stack.Pop());
                    var position = (Vector3)stack.Pop();
                    var name = (string)stack.Pop();
                    var piece = pieces.SingleOrDefault(p => p.name == name);
                    if (piece) {
                        piece.transform.position = position;
                        piece.transform.rotation = Quaternion.Euler(0, rotationY, 0);
                        piece.transform.localScale = new Vector3(localScaleX, 1, 1);
                    }
                    else
                        Debug.LogError($"Piece with name {name} not found");*/
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
                default:
                    stack.ExecuteToken(token);
                    break;
            }
        return true;
    }

    [Command]
    public void Save() {
        var stringWriter = new StringWriter();

        foreach (var quad in quads)
            stringWriter.PostfixWriteLine("add-quad ( {0} {1} {2} {3} {4} )", quad.topLeft, quad.topRight, quad.bottomLeft, quad.bottomRight, quad.position);
        foreach (var piece in pieces)
            stringWriter.PostfixWriteLine("set-piece-transform ( {0} {1} {2} {3} )", piece.name, piece.transform.position, piece.transform.rotation.eulerAngles.y, piece.transform.localScale.x);

        stringWriter.PostfixWriteLine("set-camera-rig-transform ( {0} {1} {2} {3} )", cameraRig.transform.position, cameraRig.transform.rotation.eulerAngles.y, cameraRig.PitchAngle, cameraRig.DollyZoom);

        LevelEditorFileSystem.Save(fileName, stringWriter.ToString());
    }

    [Serializable]
    public class Quad {
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

    public List<Quad> quads = new() {
        new Quad(TileType.Sea, TileType.Sea, TileType.Sea, TileType.Plain)
    };

    [Command]
    public void GenerateAllValidInvariants() {

        var position = new Vector2(0, 0);
        var step = 1;
        var row = 5;
        var count = 0;

        quads.Clear();
        foreach (var a in tileTypes)
        foreach (var b in tileTypes)
        foreach (var c in tileTypes)
        foreach (var d in tileTypes) {

            var values = new[] { a, b, c, d };
            if (values.Contains(TileType.River) && values.Contains(TileType.Beach))
                continue;

            var quad = new Quad(a, b, c, d);
            if (quad.AllFlipsAndRotations.Any(modified => quads.Any(q => q.HasSameCorners(modified))))
                continue;

            if (count++ % row == 0)
                position = new Vector2(0, position.y - step);
            else
                position += new Vector2(step, 0);

            quad.position = position;
            quads.Add(quad);
        }
    }

    [Command]
    public float schemeHeight = 0;

    public IEnumerator Loop() {
        bool TryGetMousePosition(out Vector2 result) {
            var ray = cameraRig.camera.ScreenPointToRay(Input.mousePosition);
            var mousePlane = new Plane(Vector3.up, Vector3.up * schemeHeight);
            if (mousePlane.Raycast(ray, out var enter)) {
                result = ray.GetPoint(enter).ToVector2();
                return true;
            }
            result = default;
            return false;
        }

        while (true) {
            yield return null;

            if (mode is Mode.Quads or Mode.Pieces) {
                if (Input.GetMouseButtonDown(Mouse.left) && TryGetMousePosition(out var position)) {
                    if (drawScheme && mode == Mode.Quads) {
                        var quad = FindQuad(position, out var selectedQuadIndex);
                        if (quad != null) {
                            while (Input.GetMouseButton(Mouse.left) && TryGetMousePosition(out position)) {
                                yield return null;
                                quad.position = (position + new Vector2(1, 1) / 2).RoundToInt() - new Vector2(1, 1) / 2;
                                if (Input.GetKeyDown(KeyCode.R))
                                    quad = quad.RotatedClockwise;
                                else if (Input.GetKeyDown(KeyCode.F))
                                    quad = quad.FlippedHorizontally;
                                else if (Input.GetKeyDown(KeyCode.Delete)) {
                                    quads.RemoveAt(selectedQuadIndex);
                                    break;
                                }
                                quads[selectedQuadIndex] = quad;

                                using (Draw.ingame.WithMatrix(QuadMatrix(quad)))
                                using (Draw.ingame.WithLineWidth(lineThickness))
                                    Draw.ingame.Polyline(vertexBuffer, true, lineColor);
                            }
                        }
                    }
                    /*else if (mode == Mode.Pieces) {
                        var piece = FindPiece(position);
                        if (piece) {
                            while (Input.GetMouseButton(Mouse.left) && TryGetMousePosition(out position)) {
                                yield return null;
                                piece.transform.position = ((position + new Vector2(1, 1) / 2).RoundToInt() - new Vector2(1, 1) / 2).ToVector3();
                                // if (Input.GetKeyDown(KeyCode.R)) {
                                //     var angle = piece.transform.rotation.eulerAngles.y;
                                //     var roundedAngle = Mathf.RoundToInt(angle / 90) * 90;
                                //     piece.transform.rotation = Quaternion.Euler(0, roundedAngle + 90, 0);
                                // }
                                // else if (Input.GetKeyDown(KeyCode.F)) {
                                //     var localScale = piece.transform.localScale;
                                //     localScale.x *= -1;
                                //     piece.transform.localScale = localScale;
                                // }

                                var mesh = piece.GetComponent<MeshFilter>().sharedMesh;
                                if (mesh)
                                    using (Draw.ingame.WithMatrix(piece.transform.localToWorldMatrix))
                                    using (Draw.ingame.WithLineWidth(lineThickness))
                                        Draw.ingame.WireMesh(mesh, lineColor);
                            }
                        }
                    }*/
                }
            }

            else if (mode == Mode.Test) {
                if (tileType == 0) {
                    Assert.IsTrue(tileTypes.Length > 0);
                    tileType = tileTypes[0];
                }
                if (Input.GetKeyDown(KeyCode.Tab))
                    tileType = tileTypes[(Array.IndexOf(tileTypes, tileType) + 1) % tileTypes.Length];
                if (TryGetMousePosition(out var position)) {
                    var tilePosition = position.RoundToInt();
                    if (Input.GetMouseButton(Mouse.left)) {
                        if (!tiles.TryGetValue(tilePosition, out var type) || type != tileType) {
                            tiles[tilePosition] = tileType;
                            RebuildPieces();
                        }
                    }
                    else if (Input.GetMouseButton(Mouse.right)) {
                        if (tiles.ContainsKey(tilePosition)) {
                            tiles.Remove(tilePosition);
                            RebuildPieces();
                        }
                    }
                }
            }
        }
    }

    [Command]
    public void RebuildPieces() {

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
                return tiles.TryGetValue(tilePosition, out var result) ? result : TileType.Sea;
            }
            var quad = new Quad(
                GetTileType(-1, 1), GetTileType(1, 1),
                GetTileType(-1, -1), GetTileType(1, -1),
                cornerPosition
            );

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
                    var copy = Instantiate(piece);
                    copy.transform.localScale = new Vector3(flipped ? -1 : 1, 1, 1);
                    copy.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);
                    copy.transform.position = quad.position.ToVector3();
                    copy.gameObject.SetActive(true);
                    placedPieces.Add(copy);
                }
                else 
                    Debug.Log($"No piece found for {foundQuad}, quad position: {foundQuad.position}");
            }
            else 
                Debug.Log($"No quad found for {quad}");
        }
    }

    public Dictionary<Vector2Int, TileType> tiles = new();
    public List<MeshRenderer> placedPieces = new();

    public enum Mode { Quads, Pieces, Test }
    public Mode mode = Mode.Quads;

    public Transform piecesRoot;
    public List<MeshRenderer> pieces = new();
    public TileType tileType = 0;

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

    public void Start() {
        TryLoad();
        StartCoroutine(Loop());
    }
    private void OnApplicationQuit() {
        Save();
    }

    [Command]
    public bool drawScheme = true;
    public void Update() {

        var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.H))
            if (shift)
                foreach (var piece in pieces)
                    piece.gameObject.SetActive(!piece.gameObject.activeSelf);
            else
                drawScheme = !drawScheme;

        if (shift) {
            if (Input.GetKeyDown(KeyCode.Q))
                mode = Mode.Quads;
            else if (Input.GetKeyDown(KeyCode.P))
                mode = Mode.Pieces;
            else if (Input.GetKeyDown(KeyCode.T))
                mode = Mode.Test;
        }

        if (drawScheme)
            foreach (var quad in quads)
                DrawQuad(quad);

        // foreach (var (position, tileType) in tiles) {
        //     var color = GetColor(tileType);
        //     Draw.ingame.WireBox(position.ToVector3(), Quaternion.identity, Vector2.one.ToVector3(), color);
        //     Draw.ingame.Label3D(position.ToVector3(), Quaternion.Euler(90, 0, 0), tileType.ToString(), labelSize, color);
        // }
    }

    public float labelSize = .1f;
    public List<Vector3> vertexBuffer = new() { new Vector2(1, 1) / 2, new Vector2(-1, 1) / 2, new Vector2(-1, -1) / 2, new Vector2(1, -1) / 2 };
    public List<int> triangleBuffer = new() { 0, 1, 2, 0, 2, 3 };
    public List<Color> colorBuffer = new();
    public float lineThickness = .5f;
    public Color lineColor = Color.white;


    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Mode:  {mode}");
        GUILayout.Label($"Quads: {quads.Count}");
        if(mode == Mode.Test)
            GUILayout.Label($"TileType: {tileType}");
    }

    public Matrix4x4 QuadMatrix(Quad quad) {
        return Matrix4x4.TRS(quad.position.ToVector3() + Vector3.up * schemeHeight, Quaternion.Euler(90, 0, 0), Vector3.one);
    }

    public void DrawQuad(Quad quad) {

        colorBuffer.Clear();
        if (quads.Count(q => q.position == quad.position) > 1)
            colorBuffer.AddRange(Enumerable.Repeat(Color.red, 4));
        else {
            colorBuffer.Add(GetColor(quad[1, 1]));
            colorBuffer.Add(GetColor(quad[-1, 1]));
            colorBuffer.Add(GetColor(quad[-1, -1]));
            colorBuffer.Add(GetColor(quad[1, -1]));
        }

        using (Draw.ingame.WithMatrix(QuadMatrix(quad)))
            Draw.ingame.SolidMesh(vertexBuffer, triangleBuffer, colorBuffer);

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
}

public static class TransformExtensions
{
    public static void SetFromMatrix(this Transform transform, Matrix4x4 matrix)
    {
        transform.localScale = matrix.ExtractScale();
        transform.rotation = matrix.ExtractRotation();
        transform.position = matrix.ExtractPosition();
    }
}

public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;
 
        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
 
        return Quaternion.LookRotation(forward, upwards);
    }
 
    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }
 
    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}