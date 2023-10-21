using UnityEngine;

public static class TextureExtensions
{
    public static Texture2D ToTexture2D(this RenderTexture renderTexture)
    {
        var tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        var oldRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;

        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        RenderTexture.active = oldRenderTexture;
        return tex;
    }
}