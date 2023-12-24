using System.Collections.Generic;
using Butjok.CommandLine;
using Drawing;
using KaimiraGames;
using UnityEngine;

public class UvMapper : MonoBehaviour {

    public Material material;
    public string uniformName = "_WorldToUv";
    public Vector3? oldPosition, oldLocalScale;
    public Quaternion? oldRotation;
    public InstancedMeshRenderer bushRenderer;
    public Texture2D bushMaskTexture;
    public bool updateRealTime = true;
    public Vector2Int resolution = new(1000, 1000);
    public int maxBushesCount = 250000;
    
    public void RefreshUniforms() {
        if (material)
            material.SetMatrix(uniformName, transform.worldToLocalMatrix);
    }

    public void Start() {
        RefreshUniforms();
    }

    public void Update() {
        if (updateRealTime)
            RefreshUniforms();
    }

    [Command]
    public void PlaceBushes() {

        Vector2 ToUv(Vector3 position) {
            var localPosition = transform.InverseTransformPoint(position);
            return new Vector2(localPosition.x, localPosition.z);
        }
        float SampleMask(Vector2 uv) {
            var pixel = bushMaskTexture.GetPixelBilinear(uv.x, uv.y);
            return pixel.r;
        }

        var samples = new List<WeightedListItem<RaycastHit>>();
        var totalWeight = 0.0;
        for (var y = 0; y < resolution.y; y++)
        for (var x = 0; x < resolution.x; x++) {
            var localPosition2d = new Vector2(x / (float)resolution.x, y / (float)resolution.y);
            var worldPosition3d = transform.TransformPoint(localPosition2d.ToVector3());
            var mask = Physics.Raycast(worldPosition3d + Vector3.up * 100, Vector3.down, out var hit, float.MaxValue, LayerMasks.Terrain)
                ? SampleMask(ToUv(hit.point))
                : 0;
            samples.Add(new WeightedListItem<RaycastHit>(hit, (int)(mask * 255)));
            totalWeight += mask;
        }

        var weightedList = new WeightedList<RaycastHit>(samples);

        var targetBushCount = (int)(totalWeight / resolution.x / resolution.y * maxBushesCount);
        var selected = new List<RaycastHit>();
        while (weightedList.Count > 0 && selected.Count < targetBushCount) {
            var item = weightedList.Next();
            selected.Add(item);
        }

        using (Draw.ingame.WithDuration(5))
            foreach (var sample in selected)
                Draw.ingame.WireSphere(sample.point, .1f, Color.yellow);
    }
}