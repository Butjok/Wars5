using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Torec;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class TerrainCreator : MonoBehaviour {

    public const string autoSavePath = "Assets/TerrainCreation/autosave.save";

    public Camera camera;
    public CameraRig cameraRig;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public MeshFilter edgeMeshFilter;
    public Material edgeMaterial;

    public Mesh mesh;
    public Mesh edgeMesh;

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
    private int birdsCount;
    public Transform water;
    public RectTransform overlay3d;
    public TMP_Text missionNameText;

    public MeshFilter bushesMeshFilter;
    public Mesh bushesMesh;

    public Transform borderTopLeft, borderTopRight, borderBottomLeft, borderBottomRight;

    [Command]
    public int BirdsCount {
        get => birdsCount;
        set {
            birdsCount = value;
            RespawnBirds();
        }
    }

    [Command]
    public void ToggleBushRendering() {
        if (!bushContainer || !bushRenderer)
            return;
        bushRenderer.enabled = !bushRenderer.enabled;
        bushContainer.gameObject.SetActive(!bushRenderer.enabled);
    }

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
        /*if (Physics.Raycast(ray, out var hit, float.MaxValue, 1 << gameObject.layer)) {
            point = hit.point;
            normal = hit.normal;
        }
        else*/
        if (plane.Raycast(ray, out var enter)) {
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
                    SubdivideLevel = 0;
                }
                if (roadCreator && roadCreator.positions.Contains(position)) {
                    roadCreator.positions.Remove(position);
                    roadCreator.Rebuild();
                }
            }

            else if (Input.GetMouseButton(Mouse.left)) {

                bool UpdateOrCreateVertex(Vector2Int position, Vector2Int corner, float elevation, out MeshUtils2.Vertex vertex) {
                    var modified = false;
                    if (!vertices.TryGetValue(position * 2 + corner, out vertex)) {
                        modified = true;
                        vertices[(position * 2 + corner)] = vertex = new MeshUtils2.Vertex {
                            position = position.ToVector3() + new Vector3(corner.x * .5f, 0, corner.y * .5f)
                        };
                    }
                    modified = modified || Math.Abs(vertex.position.y - elevation) > 0.001f;
                    vertex.position.y = elevation;
                    return modified;
                }

                var modified = false;
                modified = UpdateOrCreateVertex(position, new Vector2Int(-1, -1), elevation, out var a) || modified;
                modified = UpdateOrCreateVertex(position, new Vector2Int(-1, 1), elevation, out var b) || modified;
                modified = UpdateOrCreateVertex(position, new Vector2Int(1, 1), elevation, out var c) || modified;
                modified = UpdateOrCreateVertex(position, new Vector2Int(1, -1), elevation, out var d) || modified;
                if (modified || !quads.ContainsKey(position)) {
                    quads[position] = new MeshUtils2.Quad { a = a, b = b, c = c, d = d };
                    SubdivideLevel = 0;
                }
            }

            else if (Input.GetKeyDown(KeyCode.T)) {
                if (treePosition is { } actualTreePosition)
                    trees.RemoveAll(t => t.position == actualTreePosition);
                else
                    trees.Add((actualPoint, Quaternion.LookRotation(Vector3.forward, Vector3.down) * Quaternion.Euler(0, Random.value * 360, 0), Vector3.one * Random.Range(treeSizeRange.x, treeSizeRange.y)));
                UpdateTreeRenderer();
            }
        }

        if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.Equals))
            elevation += elevationStep;
        else if (Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.Minus))
            elevation -= elevationStep;

        else if (Input.GetKeyDown(KeyCode.B))
            PlaceBushes();

        // draw bushes
        /*foreach (var (position, rotation) in bushes)
            using (Draw.ingame.WithMatrix(Matrix4x4.TRS(position, rotation, Vector3.one)))
                Draw.ingame.Cross(Vector3.zero, bushSize, bushColor);*/
    }

    public VoronoiRenderer voronoiRenderer;
    public float edgeThickness = 100;

    private HashSet<MeshUtils2.Vertex> usedVertices = new();
    private List<(Vector2Int position, MeshUtils2.Vertex vertex)> vertices2 = new();

    [Command]
    public void TrimRoads() {
        if (!roadCreator)
            return;
        roadCreator.positions.IntersectWith(quads.Keys);
        roadCreator.Rebuild();
    }

    private void RebuildTerrain(bool clearBushes = true) {

        //remove unused vertices
        {
            usedVertices.Clear();
            foreach (var quad in quads.Values) {
                usedVertices.Add(quad.a);
                usedVertices.Add(quad.b);
                usedVertices.Add(quad.c);
                usedVertices.Add(quad.d);
            }

            vertices2.Clear();
            foreach (var (position, vertex) in vertices)
                vertices2.Add((position, vertex));
            foreach (var (position, vertex)in vertices2)
                if (!usedVertices.Contains(vertex))
                    vertices.Remove(position);
        }

        if (quads.Count == 0) {
            if (mesh)
                mesh.Clear();
            if (edgeMesh)
                edgeMesh.Clear();
        }

        else {

            float minX = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxZ = float.MinValue;
            foreach (var quad in quads.Values) {

                minX = Mathf.Min(minX, quad.a.position.x);
                minX = Mathf.Min(minX, quad.b.position.x);
                minX = Mathf.Min(minX, quad.c.position.x);
                minX = Mathf.Min(minX, quad.d.position.x);

                minZ = Mathf.Min(minZ, quad.a.position.z);
                minZ = Mathf.Min(minZ, quad.b.position.z);
                minZ = Mathf.Min(minZ, quad.c.position.z);
                minZ = Mathf.Min(minZ, quad.d.position.z);

                maxX = Mathf.Max(maxX, quad.a.position.x);
                maxX = Mathf.Max(maxX, quad.b.position.x);
                maxX = Mathf.Max(maxX, quad.c.position.x);
                maxX = Mathf.Max(maxX, quad.d.position.x);

                maxZ = Mathf.Max(maxZ, quad.a.position.z);
                maxZ = Mathf.Max(maxZ, quad.b.position.z);
                maxZ = Mathf.Max(maxZ, quad.c.position.z);
                maxZ = Mathf.Max(maxZ, quad.d.position.z);
            }
            var size = new Vector2(maxX - minX, maxZ - minZ).RoundToInt();

            foreach (var vertex in vertices.Values)
                vertex.uv0 = vertex.position.ToVector2() - new Vector2(minX, minZ);

            if (!mesh)
                mesh = new Mesh();
            mesh = MeshUtils2.Construct(quads.Values, mesh);
            if (subdivideLevel > 0)
                mesh = CatmullClark.Subdivide(mesh, subdivideLevel);
            mesh.name = "Terrain";
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            if (meshRenderer && meshRenderer.sharedMaterial) {
                meshRenderer.sharedMaterial.SetVector("_Splat2Size", (Vector2)size);
            }

            if (voronoiRenderer) {
                voronoiRenderer.worldSize = size;
                voronoiRenderer.Render2(size, voronoiRenderer.pixelsPerUnit);
            }

            borderTopLeft.position = new Vector3(minX + 2, 0, maxZ - 2);
            borderTopRight.position = new Vector3(maxX - 2, 0, maxZ - 2);
            borderBottomLeft.position = new Vector3(minX + 2, 0, minZ + 2);
            borderBottomRight.position = new Vector3(maxX - 2, 0, minZ + 2);

            {
                var edgePositions = new HashSet<Vector2Int>();
                foreach (var position in quads.Keys)
                    for (var y = -1; y <= 1; y++)
                    for (var x = -1; x <= 1; x++)
                        edgePositions.Add(position + new Vector2Int(x, y));
                edgePositions.ExceptWith(quads.Keys);
                var edgeVertices = new Dictionary<Vector2Int, MeshUtils2.Vertex>();
                var xMax = edgePositions.Max(p => p.x);
                var xMin = edgePositions.Min(p => p.x);
                var yMax = edgePositions.Max(p => p.y);
                var yMin = edgePositions.Min(p => p.y);
                foreach (var position in edgePositions) {
                    var a = edgeVertices[position * 2 + new Vector2Int(-1, -1)] = new MeshUtils2.Vertex { position = position.ToVector3() + new Vector3(-.5f, 0, -.5f) };
                    var b = edgeVertices[position * 2 + new Vector2Int(-1, 1)] = new MeshUtils2.Vertex { position = position.ToVector3() + new Vector3(-.5f, 0, .5f) };
                    var c = edgeVertices[position * 2 + new Vector2Int(1, 1)] = new MeshUtils2.Vertex { position = position.ToVector3() + new Vector3(.5f, 0, .5f) };
                    var d = edgeVertices[position * 2 + new Vector2Int(1, -1)] = new MeshUtils2.Vertex { position = position.ToVector3() + new Vector3(.5f, 0, -.5f) };
                    if (position.x == xMin) {
                        // a.position.x -= edgeThickness;
                        // b.position.x -= edgeThickness;
                        a.position.y = edgeThickness;
                        b.position.y = edgeThickness;
                    }
                    if (position.x == xMax) {
                        // c.position.x += edgeThickness;
                        // d.position.x += edgeThickness;
                        c.position.y = edgeThickness;
                        d.position.y = edgeThickness;
                    }
                    if (position.y == yMin) {
                        // a.position.z -= edgeThickness;
                        // d.position.z -= edgeThickness;
                        a.position.y = edgeThickness;
                        d.position.y = edgeThickness;
                    }
                    if (position.y == yMax) {
                        // b.position.z += edgeThickness;
                        // c.position.z += edgeThickness;
                        b.position.y = edgeThickness;
                        c.position.y = edgeThickness;
                    }
                }
                var edgeQuads = new List<MeshUtils2.Quad>();
                foreach (var position in edgePositions)
                    edgeQuads.Add(new MeshUtils2.Quad {
                        a = vertices.TryGetValue(position * 2 + new Vector2Int(-1, -1), out var a) ? a : edgeVertices[position * 2 + new Vector2Int(-1, -1)],
                        b = vertices.TryGetValue(position * 2 + new Vector2Int(-1, 1), out var b) ? b : edgeVertices[position * 2 + new Vector2Int(-1, 1)],
                        c = vertices.TryGetValue(position * 2 + new Vector2Int(1, 1), out var c) ? c : edgeVertices[position * 2 + new Vector2Int(1, 1)],
                        d = vertices.TryGetValue(position * 2 + new Vector2Int(1, -1), out var d) ? d : edgeVertices[position * 2 + new Vector2Int(1, -1)]
                    });

                if (!edgeMesh)
                    edgeMesh = new Mesh();
                edgeMesh = MeshUtils2.Construct(edgeQuads, edgeMesh);
                if (subdivideLevel > 0)
                    edgeMesh = CatmullClark.Subdivide(edgeMesh, subdivideLevel);
                edgeMesh.name = "TerrainEdge";
                edgeMesh.RecalculateNormals();
                edgeMesh.RecalculateBounds();
                edgeMesh.RecalculateTangents();

                edgeMeshFilter.sharedMesh = edgeMesh;
            }
        }

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        if (edgeMeshFilter)
            edgeMeshFilter.sharedMesh = edgeMesh;
        if (edgeMaterial) {
            var bounds = meshRenderer.bounds;
            edgeMaterial.SetVector("_Min", bounds.min.ToVector2() + new Vector2(.5f, .5f));
            edgeMaterial.SetVector("_Size", bounds.size.ToVector2() - new Vector2(1, 1));
        }

        if (clearBushes) {
            bushes.Clear();
            if (bushRenderer)
                bushRenderer.transformList = null;
        }

        //if (roadCreator)
        //roadCreator.Rebuild();

        if (bushMaterial) {
            var bounds = meshRenderer.bounds;
            var localToWorld = Matrix4x4.TRS(bounds.min, Quaternion.identity, new Vector3(bounds.size.x, 1, bounds.size.z));
            bushMaterial.SetMatrix("_WorldToLocal", localToWorld.inverse);
            bushMaterial.SetVector("_Min", bounds.min.ToVector2() + new Vector2(.5f, .5f));
            bushMaterial.SetVector("_Size", bounds.size.ToVector2() - new Vector2(1, 1));
            if (voronoiRenderer)
                bushMaterial.SetTexture("_Splat2", voronoiRenderer.blurredFieldMaskRenderTexture);
        }

        if (water) {
            var bounds = meshRenderer.bounds;
            water.position = bounds.center.ToVector2().ToVector3();
            water.localScale = new Vector3(bounds.size.x, 1, bounds.size.z);
        }

        if (overlay3d) {
            var bounds = meshRenderer.bounds;
            overlay3d.position = bounds.center.ToVector2().ToVector3();
            overlay3d.sizeDelta = bounds.size.ToVector2();
        }

        RespawnBirds();

        if (cameraRig)
            cameraRig.bounds = meshRenderer.bounds;
    }


    [Command]
    public void RespawnBirds() {

        foreach (var bird in birds)
            Destroy(bird.gameObject);
        birds.Clear();

        if (!birdPrefab)
            return;

        var bounds = meshRenderer.bounds;
        var min = bounds.min.ToVector2();
        var max = bounds.max.ToVector2();

        for (var i = 0; i < birdsCount; i++) {
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

    public Mesh bushMesh;
    public Material bushMaterial2;

    [Command]
    public void PlaceBushes() {

        var origin = meshFilter.sharedMesh ? meshFilter.sharedMesh.bounds.min.ToVector2() : Vector2.zero;
        var uvs = voronoiRenderer.Distribute(voronoiRenderer.bushMaskRenderTexture, voronoiRenderer.worldSize, bushDensityPerUnit, bushSeed).ToList();

        bushes.Clear();
        foreach (var uv in uvs) {
            var position2d = origin + uv * voronoiRenderer.worldSize;
            var scale = Vector3.one * Mathf.Lerp(bushSizeRange[0], bushSizeRange[1], Random.value);
            if (PlaceOnTerrain.TryRaycast(position2d, out var hit) && hit.point.y > -.01) {
                var skip = false;
                if (roadCreator)
                    for (var y = -1; y <= 1; y++)
                    for (var x = -1; x <= 1; x++)
                        if (!skip && roadCreator.positions.Contains((position2d + new Vector2(x, y) * .33f).RoundToInt())) {
                            skip = true;
                            break;
                        }
                if (skip)
                    continue;
                var position3d = hit.point;
                var rotation = (-hit.normal).ToRotation(Random.value * 360);
                bushes.Add((position3d, rotation, scale));
            }
        }

        /*if (!bushesMesh)
            bushesMesh = new Mesh();
        bushesMesh.Clear();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        foreach (var bush in bushes) {
            var matrix = Matrix4x4.TRS(bush.position, bush.rotation, bush.scale);
            foreach (var vertex in )
        }*/

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

    public Transform bushContainer;
    public ParticleSystem bushesParticleSystem;

    [Command]
    public void CreateBushes() {
        if (!bushesParticleSystem)
            return;
        var particles = new ParticleSystem.Particle[bushes.Count];
        for (var i = 0; i < bushes.Count; i++) {
            var bush = bushes[i];
            particles[i] = new ParticleSystem.Particle {
                position = bush.position,
                rotation3D = bush.rotation.eulerAngles,
                startSize3D = bush.scale,
                startColor = Color.white
            };
        }
        bushesParticleSystem.SetParticles(particles, particles.Length);
    }

    [Command]
    public void ToggleBushes() {
        if (bushesParticleSystem)
            bushesParticleSystem.gameObject.SetActive(!bushesParticleSystem.gameObject.activeSelf);
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
        GUILayout.Label("[T] Place Or Remove Tree");
        GUILayout.Label("[B] Place bushes");
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

        stringWriter.PostfixWriteLine("set-birds-count ( {0} )", birdsCount);

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
                case "set-birds-count": {
                    birdsCount = (int)stack.Pop();
                    RespawnBirds();
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