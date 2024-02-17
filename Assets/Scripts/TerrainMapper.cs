using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using KaimiraGames;
using Stable;
using UnityEngine;
using UnityEngine.Serialization;

public class TerrainMapper : MonoBehaviour {

    [Header("Bush mesh and raycasting")]
    public Mesh bushMesh;
    public List<Vector3> bushMeshRaycastOrigins = new();
    public bool drawBushRaycastOrigins;
    public float bushSeaLevelThreshold = .15f;
    public float maxBushGap = .01f;
    public Vector2Int resolution = new(1000, 1000);
    public int maxBushesCount = 500000;
    public Vector2 bushScaleRange = new(.2f, 1.5f);

    [Header("Bush rendering")]
    public InstancedMeshRenderer2 bushRenderer;

    [Header("Uv mapping")]
    public Texture2D bushMaskTexture;
    public Material[] materials = { };
    public string worldToSplatUniformName = "_Splat_WorldToLocal";

    public const string defaultLoadOnAwakeFileName = "Bushes";
    [Header("Startup")]
    public bool loadOnAwake = true;
    public string loadOnAwakeFileName = defaultLoadOnAwakeFileName;
    
    [FormerlySerializedAs("updateRealTime")] [Header("Editing")]
    public bool updateInRealTime;
    public bool autoSave = true;
    public bool bushesWereModified;

    public void Awake() {
        if (loadOnAwake)
            TryLoadBushes(loadOnAwakeFileName);
    }

    public void Start() {
        RefreshUniforms();
    }

    public void Update() {
        if (updateInRealTime)
            RefreshUniforms();

        if (drawBushRaycastOrigins)
            using (Draw.editor.WithLineWidth(2)) {
                Draw.editor.SolidMesh(bushMesh);
                foreach (var origin in bushMeshRaycastOrigins)
                    Draw.editor.WireSphere(origin, .05f, Color.red);
            }
    }

    public void OnApplicationQuit() {
        if (autoSave && bushesWereModified)
            SaveBushes(loadOnAwakeFileName);
    }

    public void RefreshUniforms() {
        foreach (var material in materials)
            material.SetMatrix(worldToSplatUniformName, transform.worldToLocalMatrix);
    }

    public void SaveBushes(string saveName) {
        if (!bushRenderer)
            return;
        var stringWriter = new StringWriter();
        foreach (var matrix in bushRenderer.transforms)
            stringWriter.PostfixWriteLine("add ( {0} )", matrix);
        LevelEditorFileSystem.Save(saveName, stringWriter.ToString());
    }

    [Command()]
    public void SaveBushes() {
        SaveBushes(loadOnAwakeFileName);
    }

    public bool TryLoadBushes(string saveName) {
        if (!bushRenderer)
            return false;
        var text = LevelEditorFileSystem.TryReadLatest(loadOnAwakeFileName);
        if (text == null)
            return false;
        var stack = new Stack();
        bushRenderer.transforms.Clear();
        foreach (var token in Tokenizer.Tokenize(text.ToPostfix()))
            switch (token) {
                case "add": {
                    var matrix = (Matrix4x4)stack.Pop();
                    bushRenderer.transforms.Add(matrix);
                    break;
                }
                default: {
                    stack.ExecuteToken(token);
                    break;
                }
            }

        bushRenderer.RecalculateBounds();
        bushRenderer.UpdateGpuData();
        return true;
    }

    [Command]
    public void ClearBushes() {
        if (bushRenderer) {
            bushRenderer.transforms.Clear();
            bushRenderer.RecalculateBounds();
            bushRenderer.UpdateGpuData();
        }
    }

    [Command]
    public void PlaceBushes() {
        if (!bushRenderer)
            return;

        Vector2 ToUv(Vector3 position) {
            var localPosition = transform.InverseTransformPoint(position);
            return new Vector2(localPosition.x, localPosition.z);
        }

        float SampleMask(Vector2 uv) {
            var pixel = bushMaskTexture.GetPixelBilinear(uv.x, uv.y);
            return pixel.r;
        }

        bool TryRaycastTerrainAndRoads(Vector3 worldPosition3d, out RaycastHit hit) {
            if (Physics.Raycast(worldPosition3d + Vector3.up * 100, Vector3.down, out hit, float.MaxValue, LayerMasks.Terrain | LayerMasks.Roads)) {
                var meshFilter = hit.collider.GetComponent<MeshFilter>();
                var meshRenderer = hit.collider.GetComponent<MeshRenderer>();
                if (meshFilter && meshRenderer) {
                    var subMeshIndex = meshFilter.sharedMesh.GetSubMeshIndex(hit.triangleIndex);
                    if (subMeshIndex != -1 && meshRenderer.sharedMaterials.Length > subMeshIndex && meshRenderer.sharedMaterials[subMeshIndex] == materials[0])
                        return true;
                }
            }

            return false;
        }

        var samples = new List<WeightedListItem<(RaycastHit hit, float yaw, float scale)>>();
        var totalWeight = 0.0;
        var triesCount = 8;
        for (var y = 0; y < resolution.y; y++)
        for (var x = 0; x < resolution.x; x++) {
            var localPosition2d = new Vector2(x / (float)resolution.x, y / (float)resolution.y);
            var centerRayOrigin = transform.TransformPoint(localPosition2d.ToVector3());
            if (TryRaycastTerrainAndRoads(centerRayOrigin, out var hit) && hit.point.y > bushSeaLevelThreshold)
                for (var i = 0; i < triesCount; i++) {
                    var yaw = Random.value * 360;
                    var scale = Random.Range(bushScaleRange[0], bushScaleRange[1]);
                    var matrix = Matrix4x4.TRS(hit.point, (-hit.normal).ToRotation(yaw), Vector3.one * scale);
                    var isValidPlacement = true;
                    foreach (var bushRayOriginLocal in bushMeshRaycastOrigins) {
                        var bushRayOriginWorld = matrix.MultiplyPoint(bushRayOriginLocal);
                        if (!TryRaycastTerrainAndRoads(bushRayOriginWorld, out var hit2) || Vector3.Distance(bushRayOriginWorld, hit2.point) > maxBushGap) {
                            isValidPlacement = false;
                            break;
                        }
                    }

                    if (isValidPlacement) {
                        var mask = SampleMask(ToUv(hit.point));
                        var integerWeight = (int)(mask * 255);
                        if (integerWeight > 0)
                            samples.Add(new WeightedListItem<(RaycastHit hit, float yaw, float scale)>((hit, yaw, scale), integerWeight));
                        totalWeight += mask;
                        break;
                    }
                }
        }

        var weightedList = new WeightedList<(RaycastHit hit, float yaw, float scale)>(samples);

        var targetBushCount = (int)(totalWeight / resolution.x / resolution.y * maxBushesCount);
        var selected = new List<(RaycastHit hit, float yaw, float scale)>();
        while (weightedList.Count > 0 && selected.Count < targetBushCount)
            selected.Add(weightedList.Next());

        bushRenderer.transforms.Clear();
        bushRenderer.transforms.AddRange(selected.Select(s => Matrix4x4.TRS(s.hit.point, (-s.hit.normal).ToRotation(s.yaw), Vector3.one * s.scale)));
        bushRenderer.RecalculateBounds();
        bushRenderer.UpdateGpuData();

        bushesWereModified = true;
    }
}