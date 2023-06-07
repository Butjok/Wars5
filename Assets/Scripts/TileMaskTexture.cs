using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class TileMaskTexture {

    public static HashSet<Texture> textures = new();

    public static (Texture texture, Matrix4x4 transform) Create(IEnumerable<(Vector2Int position, bool isSet)> positions, int resolution = 1) {
        var min = new Vector2Int(int.MaxValue, int.MaxValue);
        var max = new Vector2Int(int.MinValue, int.MinValue);
        var set = new HashSet<Vector2Int>();
        foreach (var (point, isSet) in positions) {
            min = Vector2Int.Min(min, point);
            max = Vector2Int.Max(max, point);
            if (isSet)
                set.Add(point);
        }
        if (min == new Vector2Int(int.MaxValue, int.MaxValue) || max == new Vector2Int(int.MinValue, int.MinValue))
            return (null, Matrix4x4.identity);
        var size = max - min + Vector2Int.one;
        min -= Vector2Int.one;
        size += 2 * Vector2Int.one;
        var transform = Matrix4x4.TRS((min - Vector2.one / 2).ToVector3(), Quaternion.identity, new Vector3(size.x, 1, size.y));
        return (Create(size, set.Select(p => p + Vector2Int.one).ToHashSet(), resolution), transform);
    }

    public static Texture Create(Vector2Int size, ICollection<Vector2Int> setPixels, int resolution = 1) {
        if (size.x <= 0 || size.y <= 0)
            return null;
        var texture = new Texture2D(size.x * resolution, size.y * resolution, TextureFormat.R8, false, false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        textures.Add(texture);
        for (var y = 0; y < size.y; y++)
        for (var x = 0; x < size.x; x++) {
            var color = setPixels.Contains(new Vector2Int(x, y)) ? new Color(-1, 0, 0) : new Color(1, 0, 0);
            for (var i = 0; i < resolution; i++)
            for (var j = 0; j < resolution; j++)
                texture.SetPixel(x * resolution + i, y * resolution + j, color);
        }
        texture.Apply();
        return texture;
    }

    public static void Set(Material material, string uniformName, Texture patchTexture, Matrix4x4 patchTransform, bool destroyOld = true) {
        if (destroyOld) {
            var oldTexture = material.GetTexture(uniformName);
            if (oldTexture) {
                Assert.IsTrue(textures.Contains(oldTexture));
                textures.Remove(oldTexture);
                Object.Destroy(oldTexture);
            }
        }
        material.SetTexture(uniformName, patchTexture);
        material.SetMatrix(uniformName + "_WorldToLocal", patchTransform.inverse);
    }

    [Command]
    public static void Test(string json) {
        var material = "Custom_UvMapperTestShader".LoadAs<Material>();
        var positions = json.FromJson<int[][]>();
        var (texture, transform) = Create(positions.Select(p => (new Vector2Int(p[0], p[1]), true)), 8);
        Set(material, "_Visibility", texture, transform);
    }
}