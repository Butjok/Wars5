using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class UnitThumbnailRenderer : MonoBehaviour {

    public Camera camera;
    public Color greenScreenColor = Color.green;
    public float threshold = .1f;
    public GameObject[] units = { };
    public Image[] targetImages = { };

    public bool overrideColor = true;
    public Color color = Color.white;
    public string propertyName = "_PlayerColor";

    public void Reset() {
        camera = GetComponent<Camera>();
        Assert.IsTrue(camera);
    }

    public void RenderThumbnail(Image targetImage) {

        camera.Render();

        var renderTexture = camera.targetTexture;
        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAHalf, true);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        var colors = new List<Color>();

        for (var y = 0; y < texture.height; y++)
        for (var x = 0; x < texture.width; x++) {
            var color = texture.GetPixel(x, y);
            if (Vector4.Distance(color, greenScreenColor) < threshold) {
                texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                colors.Add(color);
            }
        }
        texture.Apply();

        if (colors.Count > 0)
            Debug.Log(colors.Aggregate(Color.black, (a, b) => a + b) / colors.Count);

        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        targetImage.sprite = sprite;
    }

    [ContextMenu(nameof(RenderThumbnails))]
    public void RenderThumbnails() {

        var propertyBlock = new MaterialPropertyBlock();
        if (overrideColor)
            propertyBlock.SetColor(propertyName, color);

        foreach (var unit in units)
            unit.SetActive(false);

        foreach (var unit in units)
        foreach (var targetImage in targetImages.Where(i => i.name == unit.name)) {

            unit.SetActive(true);
            foreach (var renderer in unit.GetComponentsInChildren<Renderer>())
                renderer.SetPropertyBlock(propertyBlock);

            RenderThumbnail(targetImage);
            unit.SetActive(false);
        }

        if (units.Length > 0)
            units[0].SetActive(true);
    }
}