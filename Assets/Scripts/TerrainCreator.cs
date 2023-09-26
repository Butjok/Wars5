using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Torec;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class TerrainCreator : MonoBehaviour {

    public const string autoSavePath = "Assets/TerrainCreation/autosave.save";

    public Camera camera;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public Mesh mesh;

    public Color cursorColor = Color.yellow;
    public float cursorThickness = 2;

    public List<(Vector3 position, Quaternion rotation, Vector3 scale)> bushes = new();
    public List<(Vector3 position, Quaternion rotation, Vector3 scale)> trees = new();
    [Command] public float bushSize = .5f;
    [Command] public Color bushColor = Color.yellow;
    [FormerlySerializedAs("bushesRenderer")]
    public InstancedMeshRenderer bushRenderer;
    [Command] public Vector2 bushSizeRange = new(.25f, 1.5f);

    public RoadCreator roadCreator;
    public Material bushMaterial;

    [FormerlySerializedAs("treesRenderer")]
    public InstancedMeshRenderer treeRenderer;
    public float treeRadius = .1f;
    [Command] public Vector2 treeSizeRange = new(.25f, 1.5f);

    public Bird birdPrefab;
    public List<Bird> birds = new();

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
        TryLoad(autoSavePath);
        RebuildTerrain(false);
        UpdateBushRenderer();
        UpdateTreeRenderer();
    }

    private int subdivideLevel = 1;

    [Command]
    public int SubdivideLevel {
        get => subdivideLevel;
        set {
            value = Mathf.Clamp(value, 0, 2);
            subdivideLevel = value;
            RebuildTerrain();
        }
    }

    [Command]
    public void ClearTrees() {
        trees.Clear();
        UpdateTreeRenderer();
    }

    public void Update() {

        var ray = camera.FixedScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, Vector3.zero);

        Vector3? point = null;
        var normal = Vector3.up;
        if (Physics.Raycast(ray, out var hit, float.MaxValue, 1 << gameObject.layer)) {
            point = hit.point;
            normal = hit.normal;
        }
        else if (plane.Raycast(ray, out var enter)) {
            point = ray.GetPoint(enter);
            normal = Vector3.up;
        }

        if (point is { } actualPoint) {
            var position = actualPoint.ToVector2Int();

            Vector3? treePosition = null;
            foreach (var (pos, _, _) in trees)
                if (Vector3.Distance(pos, actualPoint) < treeRadius) {
                    treePosition = pos;
                    break;
                }

            using (Draw.ingame.WithLineWidth(cursorThickness)) {

                Draw.ingame.CircleXZ(position.ToVector3() + Vector3.up * actualPoint.y, .25f, cursorColor);
                Draw.ingame.Ray(actualPoint, normal, cursorColor);

                if (treePosition is { } actualTreePosition) {
                    Draw.ingame.Line(actualPoint, actualTreePosition, Color.red);
                    Draw.ingame.CircleXZ(actualTreePosition, treeRadius, Color.red);
                }
            }

            if (Input.GetMouseButton(Mouse.right)) {
                if (quads.ContainsKey(position)) {
                    quads.Remove(position);
                    RebuildTerrain();
                }
            }

            else if (Input.GetMouseButton(Mouse.left)) {

                MeshUtils2.Vertex UpdateOrCreateVertex(Vector2Int position, Vector2Int corner, float elevation) {
                    if (!vertices.TryGetValue(position * 2 + corner, out var vertex))
                        vertices[(position * 2 + corner)] = vertex = new MeshUtils2.Vertex {
                            position = position.ToVector3() + new Vector3(corner.x * .5f, 0, corner.y * .5f)
                        };
                    vertex.position.y = elevation;
                    return vertex;
                }

                var a = UpdateOrCreateVertex(position, new Vector2Int(-1, -1), elevation);
                var b = UpdateOrCreateVertex(position, new Vector2Int(-1, 1), elevation);
                var c = UpdateOrCreateVertex(position, new Vector2Int(1, 1), elevation);
                var d = UpdateOrCreateVertex(position, new Vector2Int(1, -1), elevation);

                quads[position] = new MeshUtils2.Quad { a = a, b = b, c = c, d = d };
                RebuildTerrain();
            }

            else if (Input.GetKeyDown(KeyCode.T)) {
                if (treePosition is { } actualTreePosition)
                    trees.RemoveAll(t => t.position == actualTreePosition);
                else
                    trees.Add((actualPoint, Quaternion.LookRotation(Vector3.forward, Vector3.down) * Quaternion.Euler(0, Random.value * 360, 0), Vector3.one * Random.Range(treeSizeRange.x, treeSizeRange.y)));
                UpdateTreeRenderer();
            }
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
            elevation += elevationStep;
        else if (Input.GetKeyDown(KeyCode.PageDown))
            elevation -= elevationStep;

        // draw bushes
        /*foreach (var (position, rotation) in bushes)
            using (Draw.ingame.WithMatrix(Matrix4x4.TRS(position, rotation, Vector3.one)))
                Draw.ingame.Cross(Vector3.zero, bushSize, bushColor);*/
    }

    public VoronoiRenderer voronoiRenderer;

    private void RebuildTerrain(bool clearBushes = true) {

        if (quads.Count == 0) {
            if (mesh)
                mesh.Clear();
        }

        else {

            var minX = quads.Values.SelectMany(quad => quad.Vertices).Min(v => v.position.x);
            var minZ = quads.Values.SelectMany(quad => quad.Vertices).Min(v => v.position.z);
            var maxX = quads.Values.SelectMany(quad => quad.Vertices).Max(v => v.position.x);
            var maxZ = quads.Values.SelectMany(quad => quad.Vertices).Max(v => v.position.z);
            var size = new Vector2(maxX - minX, maxZ - minZ).RoundToInt();

            foreach (var vertex in vertices.Values)
                vertex.uv0 = vertex.position.ToVector2() - new Vector2(minX, minZ);

            if (!mesh)
                mesh = new Mesh();
            mesh = MeshUtils2.Construct(quads.Values, mesh);
            if (subdivideLevel > 0)
                mesh = CatmullClark.Subdivide(mesh, subdivideLevel);
            mesh.name = "Terrain";
            mesh.RecalculateNormals(30);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            if (meshRenderer && meshRenderer.sharedMaterial)
                meshRenderer.sharedMaterial.SetVector("_Splat2Size", (Vector2)size);

            if (voronoiRenderer) {
                voronoiRenderer.worldSize = size;
                voronoiRenderer.Render2(size, voronoiRenderer.pixelsPerUnit);
            }
        }

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        if (clearBushes) {
            bushes.Clear();
            if (bushRenderer)
                bushRenderer.transformList = null;
        }

        if (roadCreator)
            roadCreator.Rebuild();

        if (bushMaterial) {
            var bounds = meshRenderer.bounds;
            var localToWorld = Matrix4x4.TRS(bounds.min, Quaternion.identity, new Vector3(bounds.size.x, 1, bounds.size.z));
            bushMaterial.SetMatrix("_WorldToLocal", localToWorld.inverse);
            if (voronoiRenderer)
                bushMaterial.SetTexture("_Splat2", voronoiRenderer.blurredFieldMaskRenderTexture);
        }

        RespawnBirds();
    }

    
    [Command]
    public void RespawnBirds(int count = 3) {
        
        foreach (var bird in birds)
            Destroy(bird.gameObject);
        birds.Clear();

        if (!birdPrefab)
            return;
        
        var bounds = meshRenderer.bounds;
        var min = bounds.min.ToVector2();
        var max = bounds.max.ToVector2();

        for (var i = 0; i < count; i++) {
            var bird = Instantiate(birdPrefab);
            birds.Add(bird);
            bird.transform.position = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y)).ToVector3();
            bird.transform.rotation = Quaternion.Euler(0, Random.value * 360, 0);
            bird.bounds = bounds;
        }
    }

    [Command]
    public float bushDensityPerUnit = 1;
    [Command]
    public int bushSeed = 0;

    [Command]
    public void PlaceBushes() {

        var origin = meshFilter.sharedMesh ? meshFilter.sharedMesh.bounds.min.ToVector2() : Vector2.zero;
        var uvs = voronoiRenderer.Distribute(voronoiRenderer.bushMaskRenderTexture, voronoiRenderer.worldSize, bushDensityPerUnit, bushSeed).ToList();

        bushes.Clear();
        foreach (var uv in uvs) {
            var position2d = origin + uv * voronoiRenderer.worldSize;
            var scale = Vector3.one * Mathf.Lerp(bushSizeRange[0], bushSizeRange[1], Random.value);
            if (PlaceOnTerrain.TryRaycast(position2d, out var hit) && hit.point.y > -.01) {
                if (roadCreator && roadCreator.positions.Contains(position2d.RoundToInt()))
                    continue;
                var position3d = hit.point;
                var rotation = (-hit.normal).ToRotation(Random.value * 360);
                bushes.Add((position3d, rotation, scale));
            }
        }

        UpdateBushRenderer();
    }

    public void UpdateBushRenderer() {
        if (bushRenderer) {
            if (bushRenderer.transformList) {
                Destroy(bushRenderer.transformList);
                bushRenderer.transformList = null;
            }
            bushRenderer.transformList = ScriptableObject.CreateInstance<TransformList>();
            bushRenderer.transformList.name = "Bushes";
            bushRenderer.transformList.matrices = bushes.Select(bush => Matrix4x4.TRS(bush.position, bush.rotation, bush.scale)).ToArray();
            bushRenderer.transformList.bounds = new Bounds(Vector3.zero, Vector3.one * 100);
            bushRenderer.ResetGpuBuffers();
        }
    }

    public void UpdateTreeRenderer() {
        if (treeRenderer) {
            if (treeRenderer.transformList) {
                Destroy(treeRenderer.transformList);
                treeRenderer.transformList = null;
            }
            treeRenderer.transformList = ScriptableObject.CreateInstance<TransformList>();
            treeRenderer.transformList.name = "Trees";
            treeRenderer.transformList.matrices = trees.Select(tree => Matrix4x4.TRS(tree.position, tree.rotation, tree.scale)).ToArray();
            treeRenderer.transformList.bounds = new Bounds(Vector3.zero, Vector3.one * 100);
            treeRenderer.ResetGpuBuffers();
        }
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"Terrain Elevation: {elevation}");
        GUILayout.Space(15);
        GUILayout.Label($"[T] Place Or Remove Tree");
        //GUILayout.Label($"{vertices.Count} vertices");
    }

    private void OnApplicationQuit() {
        Save(autoSavePath);
    }

    public Dictionary<Vector2Int, MeshUtils2.Vertex> vertices = new();
    public Dictionary<Vector2Int, MeshUtils2.Quad> quads = new();

    public void Save(string path) {

        var vertexList = vertices.Values.ToList();

        var stringWriter = new StringWriter();

        stringWriter.PostfixWriteLine("set-subdivide-level ( {0} )", subdivideLevel);

        foreach (var (key, vertex) in vertices) {
            stringWriter.PostfixWriteLine("vertex.add ( {0} {1} {2} {3} {4} {5} )", key, vertex.position, vertex.uv0, vertex.uv1, vertex.uv2, vertex.color);
            vertexList.Add(vertex);
        }

        foreach (var (key, quad) in quads)
            stringWriter.PostfixWriteLine("quad.add ( {0} {1} {2} {3} {4} )", key, vertexList.IndexOf(quad.a), vertexList.IndexOf(quad.b), vertexList.IndexOf(quad.c), vertexList.IndexOf(quad.d));

        foreach (var (position, rotation, scale) in bushes)
            stringWriter.PostfixWriteLine("bush.add ( {0} {1} {2} )", position, rotation, scale);

        foreach (var (position, rotation, scale) in trees)
            stringWriter.PostfixWriteLine("tree.add ( {0} {1} {2} )", position, rotation, scale);

        File.WriteAllText(path, stringWriter.ToString());
    }

    public bool TryLoad(string path) {

        if (!File.Exists(path))
            return false;

        var input = File.ReadAllText(path).ToPostfix();
        var stack = new Stack();
        var verticesList = new List<MeshUtils2.Vertex>();

        vertices.Clear();
        quads.Clear();

        foreach (var token in Tokenizer.Tokenize(input))
            switch (token) {
                case "set-subdivide-level": {
                    var value = (int)stack.Pop();
                    subdivideLevel = value;
                    break;
                }
                case "vertex.add": {
                    var color = (Color?)stack.Pop();
                    var uv2 = (Vector2?)stack.Pop();
                    var uv1 = (Vector2?)stack.Pop();
                    var uv0 = (Vector2?)stack.Pop();
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
                case "bush.add": {
                    var scale = (Vector3)stack.Pop();
                    var rotation = (Quaternion)stack.Pop();
                    var position = (Vector3)stack.Pop();
                    bushes.Add((position, rotation, scale));
                    break;
                }
                case "tree.add": {
                    var scale = (Vector3)stack.Pop();
                    var rotation = (Quaternion)stack.Pop();
                    var position = (Vector3)stack.Pop();
                    trees.Add((position, rotation, scale));
                    break;
                }
                default:
                    stack.ExecuteToken(token);
                    break;
            }

        return true;
    }
}