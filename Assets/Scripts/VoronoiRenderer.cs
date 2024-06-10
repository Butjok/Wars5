using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class VoronoiRenderer : MonoBehaviour {

    public Material material;
    public Renderer previewRenderer, finalPreviewRenderer;
    public Material terrainMaterial;
    public RenderTexture fieldMaskRenderTexture, blurredFieldMaskRenderTexture, bushMaskRenderTexture;

    [Command]
    public bool autoRender = true;

    [Command]
    public Vector2Int worldSize = new(5, 5);

    [Command]
    public int pixelsPerUnit = 64;

    [Command]
    public bool renderBushMask = false;

    public void Update() {
        if (autoRender)
            Render();
    }

    [Command]
    [ContextMenu(nameof(Export))]
    public void Export() {
        blurredFieldMaskRenderTexture.ToTexture2D().SaveAsPng("Assets/Textures/FieldMask.png");
        bushMaskRenderTexture.ToTexture2D().SaveAsPng("Assets/Textures/BushMask.png");
    }

    [Command]
    [ContextMenu(nameof(Render))]
    public void Render() {
        void EnsureRenderTexture(ref RenderTexture renderTexture, Vector2Int sizeInPixels, int mipCount = 0) {
            if (renderTexture && (renderTexture.width != sizeInPixels.x || renderTexture.height != sizeInPixels.y)) {
                renderTexture.Release();
                renderTexture = null;
            }

            if (!renderTexture) {
                renderTexture = new RenderTexture(sizeInPixels.x, sizeInPixels.y, 0, RenderTextureFormat.ARGB32, mipCount);
                if (!renderTexture.IsCreated()) {
                    var created = renderTexture.Create();
                    Assert.IsTrue(created);
                }
            }
        }

        var sizeInPixels = worldSize * pixelsPerUnit;
        EnsureRenderTexture(ref fieldMaskRenderTexture, sizeInPixels);
        fieldMaskRenderTexture.filterMode = FilterMode.Point;
        EnsureRenderTexture(ref blurredFieldMaskRenderTexture, sizeInPixels, 3);
        blurredFieldMaskRenderTexture.filterMode = FilterMode.Bilinear;
        EnsureRenderTexture(ref bushMaskRenderTexture, sizeInPixels);

        if (!material)
            material = "Voronoi".LoadAs<Material>();

        material.SetVector("_Size", new Vector4(worldSize.x, worldSize.y, 1, 1));
        Graphics.Blit(null, fieldMaskRenderTexture, material, material.FindPass("Voronoi"));
        Graphics.Blit(fieldMaskRenderTexture, blurredFieldMaskRenderTexture, material, material.FindPass("Blur"));
        Graphics.Blit(fieldMaskRenderTexture, bushMaskRenderTexture, material, material.FindPass("BushMask"));

        if (terrainMaterial)
            terrainMaterial.SetTexture("_Splat2", blurredFieldMaskRenderTexture);

        if (previewRenderer) {
            if (previewRenderer.sharedMaterial)
                previewRenderer.sharedMaterial.mainTexture = fieldMaskRenderTexture;
            previewRenderer.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1);
        }

        if (finalPreviewRenderer) {
            if (finalPreviewRenderer.sharedMaterial)
                finalPreviewRenderer.sharedMaterial.mainTexture = blurredFieldMaskRenderTexture;
            finalPreviewRenderer.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1);
        }
    }

    public Texture2D densityMap;

    public IEnumerable<Vector2> Distribute(RenderTexture renderTexture, Vector2Int worldSize, float densityPerSquareUnit, int seed = 0) {
        if (!densityMap || (densityMap.width != renderTexture.width || densityMap.height != renderTexture.height)) {
            if (densityMap) {
                Destroy(densityMap);
                densityMap = null;
            }

            densityMap = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RFloat, 0, true);
            Assert.IsTrue(densityMap);
        }

        // copy renderTexture to densityMap
        {
            var currentActiveRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            densityMap.ReadPixels(new Rect(0, 0, densityMap.width, densityMap.height), 0, 0);
            densityMap.Apply();
            RenderTexture.active = currentActiveRT;
        }

        Random.InitState(seed);

        var totalPixels = densityMap.width * densityMap.height;
        var densityTotalSum = 0f;

        var pixelsPerSquareUnit = (densityMap.width / this.worldSize.x) * (densityMap.height / this.worldSize.y);

        for (var y = 0; y < densityMap.height; y++)
        for (var x = 0; x < densityMap.width; x++)
            densityTotalSum += densityMap.GetPixel(x, y).r;

        for (var y = 0; y < densityMap.height; y++)
        for (var x = 0; x < densityMap.width; x++) {
            var pixelDensity = densityMap.GetPixel(x, y).r;
            var probability = densityTotalSum / totalPixels * pixelDensity * (densityPerSquareUnit / pixelsPerSquareUnit);
            if (Random.value < probability)
                yield return new Vector2((float)x / densityMap.width, (float)y / densityMap.height);
        }
    }
}