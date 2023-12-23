using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class UvMapper : MonoBehaviour {

    public Material material;
    public string uniformName = "_WorldToUv";

    public Vector3? oldPosition, oldLocalScale;
    public Quaternion? oldRotation;

    public InstancedMeshRenderer bushRenderer;

    public Texture2D bushMaskTexture;

    public void RefreshUniforms() {
        if (material) {
            material.SetMatrix(uniformName, transform.worldToLocalMatrix);
        }
    }

    public void Start() {
        RefreshUniforms();
    }

    public void Update() {
        RefreshUniforms();
    }

    public Vector2Int resolution = new(100, 100);
    public int maxBushesCount = 10000;

    [Command]
    public void PlaceBushes() {
        Vector2 ToUv(Vector3 position) {
            var localPosition = transform.InverseTransformPoint(position);
            return new Vector2(localPosition.x, localPosition.z);
        }

        float SampleWeight(Vector2 uv) {
            var pixel = bushMaskTexture.GetPixelBilinear(uv.x, uv.y);
            return pixel.r;
        }

        var samples = new Dictionary<Vector2, (float weight, Vector3 position)>();
        for (var y = 0; y < resolution.y; y++)
        for (var x = 0; x < resolution.x; x++) {
            var localPosition2d = new Vector2(x / (float)resolution.x, y / (float)resolution.y);
            var worldPosition3d = transform.TransformPoint(localPosition2d.ToVector3());
            var uv = ToUv(worldPosition3d);
            var weight = Physics.Raycast(worldPosition3d + Vector3.up * 100, Vector3.down, out var hit, float.MaxValue, LayerMasks.Terrain)
                ? SampleWeight(ToUv(hit.point))
                : 0;
            samples.Add(uv, (weight, hit.point));
        }

        var totalWeight = samples.Sum(sample => sample.Value.weight);
        var selected = new HashSet<Vector2>();
        var targetBushesCount = Mathf.RoundToInt(totalWeight / resolution.x / resolution.y * maxBushesCount);
        while (selected.Count < targetBushesCount)
            selected.Add(samples.Keys.RandomElementByWeight(totalWeight, sample => samples[sample].weight));

        using (Draw.ingame.WithDuration(5))
            foreach (var sample in selected)
                Draw.ingame.WireSphere(samples[sample].position, .1f, Color.yellow);
    }
}