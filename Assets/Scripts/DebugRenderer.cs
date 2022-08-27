using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class DebugRenderer : MonoBehaviour {

	public Level level;
	public int tileSize = 64;
	public GUISkin guiSkin;
	public string guiStyleName = "box";
	public Texture2D texture2D;

	public static readonly Dictionary<TileType, Color> tileTypeColors = new Dictionary<TileType, Color> {
		[TileType.Plain]=Color.green,
		[TileType.Road]=new Color(0.8f, 0.77f, 0.68f),
		[TileType.Sea]=Color.blue,
		[TileType.Mountain]=new Color(0.63f, 0.47f, 0.38f),
	};
	public static readonly Color neutalColor = Color.grey;

	public void OnGUI() {

		if (level == null)
			return;

		var positions = new HashSet<Vector2Int>();
		positions.UnionWith(level.tiles.Keys);
		positions.UnionWith(level.units.Keys);
		positions.UnionWith(level.buildings.Keys);

		if (positions.Count == 0)
			return;

		var xRange = new Vector2Int(positions.Min(position => position.x), positions.Max(position => position.x));
		var yRange = new Vector2Int(positions.Min(position => position.y), positions.Max(position => position.y));

		if (guiSkin)
			GUI.skin = guiSkin;

		for (var y = yRange[0]; y <= yRange[1]; y++)
		for (var x = xRange[0]; x <= xRange[1]; x++) {

			var position = new Vector2Int(x, y);

			var text = $"({x},{y}) ";
			if (level.TryGetTile(position, out var tileType))
				text += $"{tileType}\n";
			if (level.TryGetBuilding(position, out var building))
				text += $"{building}\n";
			if (level.TryGetUnit(position, out var unit))
				text += $"{unit}\n";

			var rect = new Rect(new Vector2Int(x * tileSize, y * tileSize), new Vector2Int(tileSize, tileSize));
			GUI.Box(rect, text, GUI.skin.GetStyle(guiStyleName));
		}
	}
}