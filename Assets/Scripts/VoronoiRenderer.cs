using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;

public class VoronoiRenderer : MonoBehaviour {

    public  Material material;
    public Renderer previewRenderer, finalPreviewRenderer;
    public Material terrainMaterial;
    public RenderTexture renderTexture, finalRenderTexture;

    [Command]
    public bool autoRender = false;
    [Command]
    public Vector2Int size = new(5, 5);
    [Command]
    public int pixelsPerUnit = 64;

    public void Update() {
        if (autoRender)
            Render2(size, pixelsPerUnit);
    }

    public void Render2(
        Vector2Int size, int pixelsPerUnit) {

        void EnsureRenderTexture(ref RenderTexture renderTexture, Vector2Int sizeInPixels) {
            if (renderTexture && (renderTexture.width != sizeInPixels.x || renderTexture.height != sizeInPixels.y)) {
                renderTexture.Release();
                renderTexture = null;
            }
            if (!renderTexture) {
                renderTexture = new RenderTexture(sizeInPixels.x, sizeInPixels.y, 0, RenderTextureFormat.ARGB32, 0);
                if (!renderTexture.IsCreated()) {
                    var created = renderTexture.Create();
                    Assert.IsTrue(created);
                }
            }
        }

        var sizeInPixels = size * pixelsPerUnit;
        EnsureRenderTexture(ref renderTexture, sizeInPixels);
        renderTexture.filterMode = FilterMode.Point;
        EnsureRenderTexture(ref finalRenderTexture, sizeInPixels);

        if (!material)
            material = "Voronoi".LoadAs<Material>();

        material.SetVector("_Size", new Vector4(size.x, size.y, 1, 1));
        Graphics.Blit(null, renderTexture, material, material.FindPass("Voronoi"));
        Graphics.Blit(renderTexture, finalRenderTexture, material, material.FindPass("Blur"));

        if(terrainMaterial)
            terrainMaterial.SetTexture("_Splat2", finalRenderTexture);
        
        if (previewRenderer) {
            if (previewRenderer.sharedMaterial)
                previewRenderer.sharedMaterial.mainTexture = renderTexture;
            previewRenderer.transform.localScale = new Vector3(size.x, size.y, 1);
        }
        if (finalPreviewRenderer) {
            if (finalPreviewRenderer.sharedMaterial)
                finalPreviewRenderer.sharedMaterial.mainTexture = finalRenderTexture;
            finalPreviewRenderer.transform.localScale = new Vector3(size.x, size.y, 1);
        }
    }
}