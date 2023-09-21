using System;
using System.Collections;
using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class Colors {

    public struct Entry {
        public string name;
        public float[] color, uiColor;
    }

    [Command]
    public static void Reset() {
        palette = null;
    }
    
    public static Dictionary<ColorName, Entry> palette;
    public static Dictionary<ColorName, Entry> Palette {
        get {
            if (palette != null)
                return palette;
            var data = "Colors2".LoadAs<TextAsset>().text.FromJson<Entry[]>();
            palette = new Dictionary<ColorName, Entry>();
            foreach (var entry in data) {
                Assert.IsTrue(entry.color.Length == 3);
                Assert.IsTrue(entry.uiColor.Length == 3);
                palette.Add((ColorName)Enum.Parse(typeof(ColorName), entry.name), entry);
            }
            return palette;
        }
    }

    public static Color Get(ColorName colorName) {
        var found = Palette.TryGetValue(colorName, out var entry);
        Assert.IsTrue(found);
        return new Color(entry.color[0], entry.color[1], entry.color[2]);
    }

    public static Color GetUi(ColorName colorName) {
        var found = Palette.TryGetValue(colorName, out var entry);
        Assert.IsTrue(found);
        return new Color(entry.uiColor[0], entry.uiColor[1], entry.uiColor[2]);
    }
}

public static class ColorExtensions {
    // https://www.shadertoy.com/view/ll2GD3
    public static Color ToColor(float t, in Color a, Color b, Color c, Color d) {
        Color Cos(Color c) {
            return new Color(Mathf.Cos(c.r), Mathf.Cos(c.g), Mathf.Cos(c.b));
        }
        // return a + b * Cos(2 * Mathf.PI * (c * t + d));
        return Color.HSVToRGB(.25f * (1 - t) + .05f, Mathf.Pow(1 - t, .25f), Mathf.Pow(1 - t, .5f));
    }
    public static Color ToColor(this float t, bool inverse = false) {
        return ToColor((inverse ? 1 - t : t), new Color(0.5f, 0.5f, 0.5f), new Color(0.5f, 0.5f, 0.5f), new Color(1.0f, 0.7f, 0.4f), new Color(0.0f, 0.15f, 0.20f));
    }
    public static Color ToColor(this float value, float min, float max, bool inverse = false) {
        return ToColor((value - min) / (max - min), inverse);
    }
    public static Color ToColor(this int t, bool inverse = false) => ToColor((float)t, inverse);
    public static Color ToColor(this int value, float min, float max, bool inverse = false) => ToColor((float)value, min, max, inverse);
}