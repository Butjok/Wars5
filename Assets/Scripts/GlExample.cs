using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;

public class GlExample : MonoBehaviour {

    public Material mat;
    public RenderTexture voronoiRenderTexture;
    public RenderTexture blurredRenderTexture;
    public Material voronoiPreviewMaterial;
    public Material blurredPreviewMaterial;
    public Transform voronoiPreviewTransform;
    public Transform blurredPreviewTransform;
    public Material terrainMaterial;

    [Command]
    public void Render(Vector2Int size, int pixelsPerUnit) {

        if (voronoiRenderTexture && (voronoiRenderTexture.width != size.x * pixelsPerUnit || voronoiRenderTexture.height != size.y * pixelsPerUnit)) {
            voronoiRenderTexture.Release();
            voronoiRenderTexture = null;
        }
        if (blurredRenderTexture && (blurredRenderTexture.width != size.x * pixelsPerUnit || blurredRenderTexture.height != size.y * pixelsPerUnit)) {
            blurredRenderTexture.Release();
            blurredRenderTexture = null;
        }

        if (!voronoiRenderTexture) {
            voronoiRenderTexture = new RenderTexture(size.x * pixelsPerUnit, size.y * pixelsPerUnit, 0, RenderTextureFormat.ARGB32, 0) {
                filterMode = FilterMode.Point
            };
            if (!voronoiRenderTexture.IsCreated())
                voronoiRenderTexture.Create();
        }
        if (!blurredRenderTexture) {
            blurredRenderTexture = new RenderTexture(size.x * pixelsPerUnit, size.y * pixelsPerUnit, 0, RenderTextureFormat.ARGB32);
            if (!blurredRenderTexture.IsCreated())
                blurredRenderTexture.Create();
            if (terrainMaterial)
                terrainMaterial.SetTexture("_Splat2", blurredRenderTexture);
        }

        mat.SetVector("_Size", new Vector4(size.x, size.y, 1, 1));
        Graphics.Blit(null, voronoiRenderTexture, mat, mat.FindPass("Voronoi"));
        Graphics.Blit(voronoiRenderTexture, blurredRenderTexture, mat, mat.FindPass("Blur"));

        if (voronoiPreviewMaterial)
            voronoiPreviewMaterial.mainTexture = voronoiRenderTexture;
        if (blurredPreviewMaterial)
            blurredPreviewMaterial.mainTexture = blurredRenderTexture;

        if (voronoiPreviewTransform)
            voronoiPreviewTransform.localScale = new Vector3(size.x, size.y, 1);
        if (blurredPreviewTransform)
            blurredPreviewTransform.localScale = new Vector3(size.x, size.y, 1);
    }

    [Command]
    public bool autoRender = true;
    [Command]
    public Vector2Int size = new(5, 5);
    [Command]
    public int pixelsPerUnit = 64;

    public void Update() {
        if (autoRender)
            Render(size, pixelsPerUnit);
    }
}