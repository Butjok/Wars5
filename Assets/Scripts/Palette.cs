using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Palette {

    public static Color32 red = new Color32(255, 87, 17, 255);
    public static Color32 green = new Color32(134, 255, 0, 255);
    public static Color32 blue = new Color32(2, 81, 255, 255);

    public static IReadOnlyDictionary<Color32, string> names = new Dictionary<Color32, string> {
        [red] = "Red",
        [green] = "Green",
        [blue] = "Blue",
    };
    public static IReadOnlyDictionary<string, Color32> colors;

    static Palette() {
        var colors = new Dictionary<string, Color32>();
        foreach (var (color, name) in names)
            colors.Add(name, color);
        Palette.colors = colors;
    }

    public static bool TryGetColor(string name, out Color32 result) {
        return colors.TryGetValue(name, out result);
    }
    public static string Name(this Color32 color) {
        return names.TryGetValue(color, out var name) ? name : color.ToString();
    }
}