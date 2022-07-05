using System.Collections.Generic;
using UnityEngine;

public static class Palette {

	public static Color red = new Color(1f, 0.17f, 0f);
	public static Color green = new Color(0.53f, 1f, 0f);
	public static Color blue = new Color(0f, 0.67f, 1f);

	public static Dictionary<Color, string> names = new() {
		[red] = "Red",
		[green] = "Green",
		[blue] = "Blue",
	};

	public static string ToString(Color color) {
		return names.TryGetValue(color, out var name) ? name : color.ToString();
	}
}