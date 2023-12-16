using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using FullscreenEditor;
using Stable;
using UnityEngine;
using UnityEngine.Assertions;
using CornerType = TileType;

public class TileMapPacker : MonoBehaviour {

    public CornerType[] cornerTypes = {
        CornerType.Sea,
        CornerType.Plain,
        CornerType.River,
        //TileType.Beach
    };
    public Color seaColor = Color.blue;
    public Color plainColor = Color.green;
    public Color riverColor = Color.cyan;
    public Color beachColor = Color.yellow;
    public Color GetColor(CornerType value) {
        return value switch {
            CornerType.Sea => seaColor,
            CornerType.Plain => plainColor,
            CornerType.River => riverColor,
            CornerType.Beach => beachColor,
            _ => Color.magenta
        };
    }

    public string fileName = "TileMap";
    
    [Command]
    public bool TryLoad() {
        var text = LevelEditorFileSystem.TryReadLatest(fileName);
        if (text == null)
            return false;
        quads.Clear();
        var stack = new Stack();
        foreach (var token in Tokenizer.Tokenize(text.ToPostfix()))
            switch (token) {
                case "add":
                    var position = (Vector2)stack.Pop();
                    var bottomRight = (CornerType)stack.Pop();
                    var bottomLeft = (CornerType)stack.Pop();
                    var topRight = (CornerType)stack.Pop();
                    var topLeft = (CornerType)stack.Pop();
                    quads.Add(new Quad(topLeft, topRight, bottomLeft, bottomRight, position));
                    break;
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
            stringWriter.PostfixWriteLine("add ( {0} {1} {2} {3} {4} )", quad.topLeft, quad.topRight, quad.bottomLeft, quad.bottomRight, quad.position);
        LevelEditorFileSystem.Save(fileName,stringWriter.ToString());
    }

    public record Quad {
        public CornerType topRight, topLeft, bottomLeft, bottomRight;
        public Vector2 position;
        public Quad(CornerType topLeft = default, CornerType topRight = default, CornerType bottomLeft = default, CornerType bottomRight = default, Vector2 position = default) {
            this.topRight = topRight;
            this.topLeft = topLeft;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
            this.position = position;
        }
        public CornerType this[int x, int y] {
            get => (x, y) switch {
                (1, 1) => topRight,
                (-1, 1) => topLeft,
                (-1, -1) => bottomLeft,
                (1, -1) => bottomRight,
                _ => throw new ArgumentOutOfRangeException()
            };
            set {
                switch ((x, y)) {
                    case (1, 1):
                        topRight = value;
                        break;
                    case (-1, 1):
                        topLeft = value;
                        break;
                    case (-1, -1):
                        bottomLeft = value;
                        break;
                    case (1, -1):
                        bottomRight = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Quad Flipped => new(
            this[1, 1], this[-1, 1],
            this[1, -1], this[-1, -1],
            position
        );
        public Quad Rotated => new(
            this[-1, -1], this[-1, 1],
            this[1, -1], this[1, 1],
            position
        );
        public IEnumerable<Quad> AllFlips {
            get {
                yield return this;
                yield return Flipped;
            }
        }
        public IEnumerable<Quad> AllRotations {
            get {
                yield return this;
                yield return Rotated;
                yield return Rotated.Rotated;
                yield return Rotated.Rotated.Rotated;
            }
        }
        public IEnumerable<Quad> AllFlipsAndRotations {
            get {
                foreach (var flipped in AllFlips)
                foreach (var rotated in flipped.AllRotations)
                    yield return rotated;
            }
        }
        public bool IsEqualsTo(Quad other) {
            return topRight == other.topRight && topLeft == other.topLeft && bottomLeft == other.bottomLeft && bottomRight == other.bottomRight;
        }
        public bool HasSameInvariant(Quad other) {
         return AllFlipsAndRotations.Any(modified => modified.IsEqualsTo(other));   
        }
    }

    public List<Quad> quads = new() {
        new Quad(TileType.Sea, TileType.Sea, TileType.Sea, TileType.Plain)
    };
    public Camera camera;

    [Command]
    public void GenerateAllPossibleCombinations() {

        var position = new Vector2(0, 0);
        var step = 1;
        var row = 5;
        var count = 0;

        quads.Clear();
        foreach (var a in cornerTypes)
        foreach (var b in cornerTypes)
        foreach (var c in cornerTypes)
        foreach (var d in cornerTypes) {
            
            var values = new[] {a, b, c, d};
            if (values.Contains(TileType.River) && values.Contains(TileType.Beach))
                continue;
            
            var quad = new Quad(a, b, c, d);
            if (quad.AllFlipsAndRotations.Any(modified => quads.Any(q => q.IsEqualsTo(modified))))
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
        var plane = new Plane(Vector3.forward, Vector3.zero);
        bool TryGetMousePosition(out Vector2 result) {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out var enter)) {
                result = ray.GetPoint(enter);
                return true;
            }
            result = default;
            return false;
        }
        var corners = new[] {
            new Vector2(1, 1) / 2,
            new Vector2(-1, 1) / 2,
            new Vector2(-1, -1) / 2,
            new Vector2(1, -1) / 2
        };
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(Mouse.left)) {
                if (TryGetMousePosition(out var position)) {
                    var selectedQuadIndex = -1;
                    for (var i = 0; i < quads.Count; i++) {
                        var quad = quads[i];
                        var points = corners.Select(corner => quad.position + corner).ToList();
                        var rect = new Rect {
                            xMin = points.Min(p => p.x),
                            xMax = points.Max(p => p.x),
                            yMin = points.Min(p => p.y),
                            yMax = points.Max(p => p.y)
                        };
                        if (rect.Contains(position)) {
                            selectedQuadIndex = i;
                            break;
                        }
                    }
                    if (selectedQuadIndex != -1) {
                        var startPosition = position;
                        var quad = quads[selectedQuadIndex];
                        var originalPosition = quad.position;
                        while (Input.GetMouseButton(Mouse.left) && TryGetMousePosition(out position)) {
                            yield return null;
                            var offset = position - startPosition;
                            quad.position = (originalPosition + offset).RoundToInt();
                            if (Input.GetKeyDown(KeyCode.R))
                                quad = quad.Rotated;
                            else if (Input.GetKeyDown(KeyCode.F))
                                quad = quad.Flipped;
                            else if (Input.GetKeyDown(KeyCode.Delete)) {
                                quads.RemoveAt(selectedQuadIndex);
                                break;
                            }
                            quads[selectedQuadIndex] = quad;
                        }
                    }
                }
            }
        }
    }

    public void Start() {
        TryLoad();
        StartCoroutine(Loop());
    }
    private void OnDestroy() {
        Save();
    }

    public void Update() {
        foreach (var quad in quads)
            DrawQuad(quad);
    }

    public List<Vector3> vertexBuffer = new();
    public List<int> triangleBuffer = new() { 0, 1, 2, 0, 2, 3 };
    public List<Color> colorBuffer = new();
    public float lineThickness = .5f;
    public Color lineColor = Color.white;

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Quads: {quads.Count}");
    }

    public void DrawQuad(Quad quad) {
        
        vertexBuffer.Clear();
        vertexBuffer.Add(quad.position + new Vector2(1, 1) / 2);
        vertexBuffer.Add(quad.position + new Vector2(-1, 1) / 2);
        vertexBuffer.Add(quad.position + new Vector2(-1, -1) / 2);
        vertexBuffer.Add(quad.position + new Vector2(1, -1) / 2);

        colorBuffer.Clear();
        colorBuffer.Add(GetColor(quad[1, 1]));
        colorBuffer.Add(GetColor(quad[-1, 1]));
        colorBuffer.Add(GetColor(quad[-1, -1]));
        colorBuffer.Add(GetColor(quad[1, -1]));

        Draw.ingame.SolidMesh(vertexBuffer, triangleBuffer, colorBuffer);
        
        using (Draw.ingame.WithLineWidth(lineThickness))
        using (Draw.ingame.WithColor(lineColor)) {
            Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(1, 1) / 2), Quaternion.identity, (Vector3)Vector2.one);
            Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(-1, 1) / 2), Quaternion.identity, (Vector3)Vector2.one);
            Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(1, -1) / 2), Quaternion.identity, (Vector3)Vector2.one);
            Draw.ingame.WireBox((Vector3)(quad.position + new Vector2(-1, -1) / 2), Quaternion.identity, (Vector3)Vector2.one);
        }
        
        if (quads.Count(q=>q.position == quad.position) > 1) {
            colorBuffer[0] = colorBuffer[1] = colorBuffer[2] = colorBuffer[3] = Color.red;
            Draw.ingame.SolidMesh(vertexBuffer, triangleBuffer, colorBuffer);
        }
    }
}