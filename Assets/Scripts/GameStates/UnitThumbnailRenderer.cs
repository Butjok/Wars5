using System;
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

        for (var y = 0; y < texture.height; y++)
        for (var x = 0; x < texture.width; x++) {
            var color = texture.GetPixel(x, y);
            if (Vector4.Distance(color, greenScreenColor) < threshold)
                texture.SetPixel(x, y, new Color(0, 0, 0, 0));
        }
        texture.Apply();

        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        targetImage.sprite = sprite;
    }

    [ContextMenu(nameof(RenderThumbnails))]
    public void RenderThumbnails() {

        foreach (var unit in units)
            unit.SetActive(false);

        foreach (var unit in units) {
            var targetImage = targetImages.SingleOrDefault(i => i.name == unit.name);
            if (targetImage) {
                unit.SetActive(true);
                RenderThumbnail(targetImage);
                unit.SetActive(false);
            }
        }

        foreach (var unit in units)
            unit.SetActive(true);
    }
}