using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using Object = UnityEngine.Object;

public static class TileMask {

    public static (Texture2D texture, Matrix4x4 transform) Create(IEnumerable<Vector2Int> positions, int resolution = 1, Func<Vector2Int, Color> color = null, FilterMode filterMode = FilterMode.Point) {
        var list = positions.ToList();
        var min = new Vector2Int(int.MaxValue, int.MaxValue);
        var max = new Vector2Int(int.MinValue, int.MinValue);
        foreach (var position in list) {
            min = Vector2Int.Min(min, position);
            max = Vector2Int.Max(max, position);
        }
        if (min == new Vector2Int(int.MaxValue, int.MaxValue) || max == new Vector2Int(int.MinValue, int.MinValue))
            return (null, Matrix4x4.identity);
        var size = max - min + Vector2Int.one;
        min -= Vector2Int.one;
        size += 2 * Vector2Int.one;
        var transform = Matrix4x4.TRS((min - Vector2.one / 2).ToVector3(), Quaternion.identity, new Vector3(size.x, 1, size.y));
        return (Create(size, list.Select(p => p - min), resolution, color != null ? pixel => color(pixel + min) : null, filterMode: filterMode), transform);
    }

    public static Texture2D Create(Vector2Int size, IEnumerable<Vector2Int> pixels, int resolution = 1, Func<Vector2Int, Color> color = null, bool linear = true,
        FilterMode filterMode = FilterMode.Point) {
        var pixelSet = pixels.ToHashSet();
        color ??= position => pixelSet.Contains(position) ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 0);
        if (size.x <= 0 || size.y <= 0)
            return null;
        var texture = new Texture2D(size.x * resolution, size.y * resolution, TextureFormat.RGBA32, false, linear) {
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Clamp
        };
        for (var y = 0; y < size.y; y++)
        for (var x = 0; x < size.x; x++) {
            var pixelColor = color(new Vector2Int(x, y));
            for (var i = 0; i < resolution; i++)
            for (var j = 0; j < resolution; j++)
                texture.SetPixel(x * resolution + i, y * resolution + j, pixelColor);
        }
        texture.Apply();
        return texture;
    }

    public static void SetTileMask(this Material material, string uniformName, Texture patchTexture, Matrix4x4 patchTransform) {
        material.SetTexture(uniformName, patchTexture);
        material.SetMatrix(uniformName + "_WorldToLocal", patchTransform.inverse);
    }
    public static void UnsetTileMask(this Material material, string uniformName) {
        SetTileMask(material, uniformName, null, Matrix4x4.identity);
    }

    [Command]
    public static void Test(string json) {
        var material = "Custom_UvMapperTestShader".LoadAs<Material>();
        var positions = json.FromJson<int[][]>().Select(p => new Vector2Int(p[0], p[1])).ToHashSet();
        var (texture, transform) = Create(positions, 8);
        SetTileMask(material, "_Visibility", texture, transform);
    }

    public const string tileMaskUniformName = "_TileMask";

    public static void ReplaceGlobal(IEnumerable<Vector2Int> positions, string uniformName = tileMaskUniformName, Func<Vector2Int, Color> color = null) {
        var (texture, transform) = Create(positions, 1, color);
        var oldTexture = Shader.GetGlobalTexture(tileMaskUniformName);
        if (oldTexture && oldTexture != Texture2D.blackTexture)
            Object.Destroy(oldTexture);
        Shader.SetGlobalTexture(tileMaskUniformName, texture);
        Shader.SetGlobalMatrix(tileMaskUniformName + "_WorldToLocal", transform.inverse);
    }
    public static void UnsetGlobal(string uniformName = tileMaskUniformName) {
        var oldTexture = Shader.GetGlobalTexture(tileMaskUniformName);
        if (oldTexture && oldTexture != Texture2D.blackTexture)
            Object.Destroy(oldTexture);
        Shader.SetGlobalTexture(tileMaskUniformName, Texture2D.blackTexture);
    }
}