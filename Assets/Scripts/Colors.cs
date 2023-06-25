using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class Colors {

    private static Dictionary<ColorName, Color> palette;
    public static Dictionary<ColorName, Color> Palette {
        get {
            if (palette != null)
                return palette;

            palette = new Dictionary<ColorName, Color>();

            var stack = new Stack();
            foreach (var token in Tokenizer.Tokenize("Colors".LoadAs<TextAsset>().text))
                stack.ExecuteToken(token);

            while (stack.Count > 0) {
                var b = (dynamic)stack.Pop();
                var g = (dynamic)stack.Pop();
                var r = (dynamic)stack.Pop();
                var name = (ColorName)stack.Pop();
                Assert.IsFalse(palette.ContainsKey(name), name.ToString());
                palette.Add(name, new Color(r, g, b));
            }

#if DEBUG
            foreach (ColorName name in Enum.GetValues(typeof(ColorName)))
                Assert.IsTrue(palette.ContainsKey(name));
#endif

            return palette;
        }
    }

    public static Color Get(ColorName colorName) {
        var found = Palette.TryGetValue(colorName, out var color);
        Assert.IsTrue(found, colorName.ToString());
        return color;
    }

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