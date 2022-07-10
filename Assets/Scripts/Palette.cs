using System.Collections.Generic;
using UnityEngine;

public static class Palette {

	public static Color32 red = new Color32(255, 44, 0, 0);
	public static Color32 green = new Color32(134, 255, 0, 0);
	public static Color32 blue = new Color32(36, 96, 255, 255);

	public static Dictionary<Color32, string> names = new() {
		[red] = "Red",
		[green] = "Green",
		[blue] = "Blue",
	};

	public static string ToString(Color32 color) {
		return names.TryGetValue(color, out var name) ? name : color.ToString();
	}
}