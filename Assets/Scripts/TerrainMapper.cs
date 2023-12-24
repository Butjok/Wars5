using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using KaimiraGames;
using UnityEngine;

public class TerrainMapper : MonoBehaviour {

    public Material[] materials = { };
    public string uniformName = "_Splat_WorldToLocal";
    public Vector3? oldPosition, oldLocalScale;
    public Quaternion? oldRotation;
    public InstancedMeshRenderer2 bushRenderer;
    public Texture2D bushMaskTexture;
    public bool updateRealTime = false;
    public Vector2Int resolution = new(1000, 1000);
    public int maxBushesCount = 500000;

    public Mesh bushMesh;
    public List<Vector3> bushRaycastOrigins = new();
    public bool drawBushRaycastOrigins = false;
    public Vector2 bushScaleRange = new(.2f, 1.5f);

    public void RefreshUniforms() {
        foreach (var material in materials)
            material.SetMatrix(uniformName, transform.worldToLocalMatrix);
    }

    public void Start() {
        RefreshUniforms();
    }

    public void Update() {
        if (updateRealTime)
            RefreshUniforms();

        if (drawBushRaycastOrigins) {
            using (Draw.editor.WithLineWidth(2)) {
                Draw.editor.SolidMesh(bushMesh);
                foreach (var origin in bushRaycastOrigins)
                    Draw.editor.WireSphere(origin, .05f, Color.red);
            }
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
            if (TryRaycastTerrainAndRoads(centerRayOrigin, out var hit))
                for (var i = 0; i < triesCount; i++) {
                    var yaw = Random.value * 360;
                    var scale = Random.Range(bushScaleRange[0], bushScaleRange[1]);
                    var matrix = Matrix4x4.TRS(hit.point, (-hit.normal).ToRotation(yaw), Vector3.one * scale);
                    var isValidPlacement = true;
                    foreach (var bushRayOriginLocal in bushRaycastOrigins) {
                        var bushRayOriginWorld = matrix.MultiplyPoint(bushRayOriginLocal);
                        if (!TryRaycastTerrainAndRoads(bushRayOriginWorld, out _)) {
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
    }
}