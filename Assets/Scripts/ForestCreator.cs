using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Stable;
using UnityEngine;
using UnityEngine.Serialization;

public class ForestCreator : MonoBehaviour {

    public const string defaultAutoFileName = "Forests";
    
    [Header("Startup")]
    public bool loadOnAwake = true;
    [FormerlySerializedAs("autoSaveName")] public string loadOnAwakeFileName = defaultAutoFileName;

    [Header("Autosave")]
    public bool autoSave = true;
    public bool wasModified;

    [Header("Dependencies")]
    [FormerlySerializedAs("renderer")] public InstancedMeshRenderer2 treeRenderer;
    public Camera camera;

    [Header("Editing")]
    public Dictionary<Vector2Int, List<Matrix4x4>> trees = new();
    public Vector2Int treeCountRange = new(1, 2);
    public Vector2 treeScaleRange = new(.5f, 1);
    [Range(0, 1)] public float jitterAmount = 1;

    [Header("Materials")]
    public Material terrainMaterial;
    public string terrainMaterialForestMaskUniformName = "_ForestMask";
    public Material bushMaterial;
    public string bushMaterialForestMaskUniformName = "_ForestMask";
    public Texture forestMaskTexture;
    public Matrix4x4 forestMaskTransform;

    public void Awake() {
        TryLoad(loadOnAwakeFileName);
    }

    public void Update() {
        if (!camera)
            return;
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, float.MaxValue, LayerMasks.Terrain))
            return;
        var tilePosition = hit.point.ToVector2Int();
        if (!Physics.Raycast(tilePosition.ToVector3() + Vector3.up * 100, Vector3.down, out var hit2, float.MaxValue, LayerMasks.Terrain))
            return;
        var tileCenterPosition3D = hit2.point;
        Draw.ingame.WireBox(tileCenterPosition3D, Vector2.one.ToVector3(), Color.white);

        if (Input.GetMouseButton(Mouse.left)) {
            if (!trees.ContainsKey(tilePosition))
                PlaceTreesAt(tilePosition);
        }
        else if (Input.GetMouseButton(Mouse.right)) {
            if (trees.ContainsKey(tilePosition))
                RemoveTreesAt(tilePosition);
        }
    }

    public void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label("Forest creator");
        GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
        GUILayout.Label("[LMB] Place trees");
        GUILayout.Label("[RMB] Remove trees");
    }

    public void OnApplicationQuit() {
        if (autoSave && wasModified)
            Save(loadOnAwakeFileName);
    }

    public bool TryLoad(string saveName) {
        var text = LevelEditorFileSystem.TryReadLatest(saveName);
        if (text == null)
            return false;
        var stack = new Stack();
        foreach (var token in Tokenizer.Tokenize(text.ToPostfix()))
            switch (token) {
                case "add": {
                    var matrix = (Matrix4x4)stack.Pop();
                    var position = (Vector2Int)stack.Pop();
                    if (!trees.ContainsKey(position))
                        trees[position] = new List<Matrix4x4>();
                    trees[position].Add(matrix);
                    break;
                }
                default: {
                    stack.ExecuteToken(token);
                    break;
                }
            }

        UpdateTreeRenderer();
        UpdateTerrainMaterialForestMask();
        return true;
    }

    public void Save(string saveName) {
        var stringWriter = new StringWriter();
        foreach (var (position, list) in trees)
        foreach (var matrix in list)
            stringWriter.PostfixWriteLine("add ( {0} {1} )", position, matrix);
        LevelEditorFileSystem.Save(saveName, stringWriter.ToString());
    }

    public static IEnumerable<Vector2> UniformPointsAt(Vector2Int position, Vector2Int count) {
        var size = new Vector2(1f / count.x, 1f / count.y);
        var margin = size / 2;
        for (var y = 0; y < count.y; y++)
        for (var x = 0; x < count.x; x++)
            yield return position - Vector2.one / 2 + margin + new Vector2(x * size.x, y * size.y);
    }

    public static Vector2 Jitter(Vector2 point, Vector2Int count, float amount = 1) {
        var xAmplitude = 1f / count.x / 2;
        var yAmplitude = 1f / count.y / 2;
        var randomOffset = new Vector2(Random.Range(-1f, 1f) * xAmplitude, Random.Range(-1f, 1f) * yAmplitude);
        return point + randomOffset * amount;
    }

    public void PlaceTreesAt(Vector2Int position) {
        if (!Physics.Raycast(position.ToVector3() + Vector3.up * 100, Vector3.down, out var centerHit, float.MaxValue, LayerMasks.Terrain))
            return;

        var list = new List<Matrix4x4>();

        var count = new Vector2Int(Random.Range(treeCountRange[0], treeCountRange[1] + 1), Random.Range(treeCountRange[0], treeCountRange[1] + 1));
        foreach (var point in UniformPointsAt(position, count)) {
            var jitteredPoint = Jitter(point, count, jitterAmount);
            if (Physics.Raycast(jitteredPoint.ToVector3() + Vector3.up * 100, Vector3.down, out var hit, float.MaxValue, LayerMasks.Terrain))
                list.Add(Matrix4x4.TRS(hit.point, Quaternion.Euler(180, Random.value * 360, 0), Vector3.one * Random.Range(treeScaleRange[0], treeScaleRange[1])));
        }

        trees.Add(position, list);
        UpdateTreeRenderer();
        UpdateTerrainMaterialForestMask();

        wasModified = true;
    }

    public void RemoveTreesAt(Vector2Int position) {
        trees.Remove(position);
        UpdateTreeRenderer();
        UpdateTerrainMaterialForestMask();

        wasModified = true;
    }

    public void UpdateTreeRenderer() {
        treeRenderer.transforms.Clear();
        foreach (var list in trees.Values)
        foreach (var matrix in list)
            treeRenderer.transforms.Add(matrix);
        treeRenderer.RecalculateBounds();
        treeRenderer.UpdateGpuData();
    }

    public void UpdateTerrainMaterialForestMask() {
        if (terrainMaterial || bushMaterial)
            (forestMaskTexture, forestMaskTransform) = TileMaskTexture.Create(trees.Keys.ToHashSet(), 4, filterMode: FilterMode.Bilinear);
        if (terrainMaterial)
            terrainMaterial.SetTileMask(terrainMaterialForestMaskUniformName, forestMaskTexture, forestMaskTransform);
        if (bushMaterial)
            bushMaterial.SetTileMask(bushMaterialForestMaskUniformName, forestMaskTexture, forestMaskTransform);
    }

    [Command]
    public void Clear() {
        trees.Clear();
        UpdateTreeRenderer();
    }
}