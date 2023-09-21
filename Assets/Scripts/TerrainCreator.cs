using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Torec;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class TerrainCreator : MonoBehaviour {

    public const string savePath = "Assets/TerrainCreation/autosave.save";

    public Camera camera;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public Mesh mesh;

    public Color cursorColor = Color.yellow;
    public float cursorThickness = 2;

    private void Reset() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        Assert.IsTrue(meshFilter);
        Assert.IsTrue(meshRenderer);
        Assert.IsTrue(meshCollider);
        gameObject.layer = LayerMask.NameToLayer("Terrain");
    }

    public float elevation = 0;
    public float elevationStep = .25f;

    private void Awake() {
        TryLoad();
        Rebuild();
    }

    [Command]
    public static int subdivideLevel = 1;

    public void Update() {

        var ray = camera.FixedScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, Vector3.zero);

        Vector3? point = null;
        if (Physics.Raycast(ray, out var hit, float.MaxValue, 1 << gameObject.layer))
            point = hit.point;
        else if (plane.Raycast(ray, out var enter))
            point = ray.GetPoint(enter);

        if (point is { } actualPoint) {
            var position = actualPoint.ToVector2Int();

            Draw.ingame.CircleXZ(position.ToVector3() + Vector3.up * actualPoint.y, .25f, cursorColor);

            var needsRebuild = false;

            if (Input.GetMouseButton(Mouse.right)) {
                if (quads.ContainsKey(position)) {
                    quads.Remove(position);
                    needsRebuild = true;
                }
            }

            else if (Input.GetMouseButton(Mouse.left)) {

                MeshUtils2.Vertex UpdateOrCreateVertex(Vector2Int position, Vector2Int corner, float elevation) {
                    if (!vertices.TryGetValue(position * 2 + corner, out var vertex))
                        vertices[(position * 2 + corner)] = vertex = new MeshUtils2.Vertex {
                            position = position.ToVector3() + new Vector3(corner.x * .5f, 0, corner.y * .5f)
                        };
                    vertex.position.y = elevation;
                    vertex.uv0 = vertex.position.ToVector2() + new Vector2(.5f, .5f);
                    return vertex;
                }

                var a = UpdateOrCreateVertex(position, new Vector2Int(-1, -1), elevation);
                var b = UpdateOrCreateVertex(position, new Vector2Int(-1, 1), elevation);
                var c = UpdateOrCreateVertex(position, new Vector2Int(1, 1), elevation);
                var d = UpdateOrCreateVertex(position, new Vector2Int(1, -1), elevation);

                quads[position] = new MeshUtils2.Quad { a = a, b = b, c = c, d = d };
                needsRebuild = true;
            }

            if (needsRebuild)
                Rebuild();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
            elevation += elevationStep;
        else if (Input.GetKeyDown(KeyCode.PageDown))
            elevation -= elevationStep;
    }

    private void Rebuild() {
        mesh = CatmullClark.Subdivide(MeshUtils2.Construct(quads.Values, mesh), subdivideLevel);
        mesh.name = "Terrain";
        mesh.RecalculateNormals(30);
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Elevation: {elevation}");
        GUILayout.Label($"{vertices.Count} vertices");
    }

    private void OnApplicationQuit() {
        Save();
    }

    public Dictionary<Vector2Int, MeshUtils2.Vertex> vertices = new();
    public Dictionary<Vector2Int, MeshUtils2.Quad> quads = new();

    [Command]
    public void Save() {

        var vertexList = vertices.Values.ToList();

        var stringWriter = new StringWriter();
        foreach (var (key, vertex) in vertices) {
            stringWriter.PostfixWriteLine("vertex.add ( {0} {1} {2} {3} {4} {5} )", key, vertex.position, vertex.uv0, vertex.uv1, vertex.uv2, vertex.color);
            vertexList.Add(vertex);
        }

        foreach (var (key, quad) in quads)
            stringWriter.PostfixWriteLine("quad.add ( {0} {1} {2} {3} {4} )", key, vertexList.IndexOf(quad.a), vertexList.IndexOf(quad.b), vertexList.IndexOf(quad.c), vertexList.IndexOf(quad.d));

        File.WriteAllText(savePath, stringWriter.ToString());
    }

    [Command]
    public bool TryLoad() {

        if (!File.Exists(savePath))
            return false;

        var input = File.ReadAllText(savePath).ToPostfix();
        var stack = new Stack();
        var verticesList = new List<MeshUtils2.Vertex>();

        vertices.Clear();
        quads.Clear();
        
        foreach (var token in Tokenizer.Tokenize(input))
            switch (token) {
                case "vertex.add": {
                    var color = (Color)stack.Pop();
                    var uv2 = (Vector2)stack.Pop();
                    var uv1 = (Vector2)stack.Pop();
                    var uv0 = (Vector2)stack.Pop();
                    var position = (Vector3)stack.Pop();
                    var key = (Vector2Int)stack.Pop();
                    var vertex = new MeshUtils2.Vertex {
                        position = position,
                        uv0 = uv0,
                        uv1 = uv1,
                        uv2 = uv2,
                        color = color
                    };
                    verticesList.Add(vertex);
                    vertices.Add(key, vertex);
                    break;
                }
                case "quad.add": {
                    var dIndex = (int)stack.Pop();
                    var cIndex = (int)stack.Pop();
                    var bIndex = (int)stack.Pop();
                    var aIndex = (int)stack.Pop();
                    var key = (Vector2Int)stack.Pop();
                    quads.Add(key, new MeshUtils2.Quad {
                        a = verticesList[aIndex],
                        b = verticesList[bIndex],
                        c = verticesList[cIndex],
                        d = verticesList[dIndex]
                    });
                    break;
                }
                default:
                    stack.ExecuteToken(token);
                    break;
            }

        return true;
    }
}