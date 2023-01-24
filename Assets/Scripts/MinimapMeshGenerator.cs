using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class MinimapMeshGenerator : MonoBehaviour {

	public Main main;
	public Mesh mesh;
	public MeshFilter meshFilter;
	public Camera renderCamera;

	public TextMeshPro textMeshPro;
	public MeshRenderer meshRenderer;

	/*public void Awake() {

		var size = new Vector2Int(5, 5);

		IEnumerable<Vector2Int> randomPositions(int maxNumber) {
			var positions = new HashSet<Vector2Int>();
			for (var i = 0; i < maxNumber; i++)
				positions.Add(new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y)));
			return positions;
		}

		var game = new Game();
		level = new Level(game);

		var red = new Player(level, Color.red);
		var blue = new Player(level, Color.blue);
		level.players = new List<Player> { red, blue };

		foreach (var position in randomPositions(10))
			level.units[position] = new Unit(level, level.players.Random(), position: position,
				type: new[] { UnitType.Infantry, UnitType.Recon, UnitType.LightTank }.Random());

		for (var y = 0; y < size.y; y++)
		for (var x = 0; x < size.x; x++) {
			var type = new[] { TileType.Plain, TileType.Road, TileType.Sea, TileType.Mountain }.Random();
			level.tiles.Add(new Vector2Int(x, y), type);
		}

		foreach (var position in randomPositions(10)) {
			var type = new[] { TileType.Plant }.Random();
			level.buildings[position] = new Building(level, position, player: level.players.Random(), type: type);
			level.tiles[position] = type;
		}

		Rebuild();
	}*/

	public static readonly Dictionary<TileType, byte> tileIds = new() {

		[TileType.Plain] = 0,
		[TileType.Road] = 1,
		[TileType.Sea] = 2,
		[TileType.Mountain] = 3,

		[TileType.City] = 4,
		[TileType.Hq] = 5,
		[TileType.Factory] = 6,
		[TileType.Airport] = 7
	};

	public static readonly Dictionary<UnitType, byte> unitIds = new() {
		[UnitType.Infantry] = 0,
		[UnitType.AntiTank] = 1,
		[UnitType.Artillery] = 2,
		[UnitType.Apc] = 3,
		[UnitType.Recon] = 8,
		[UnitType.LightTank] = 9,
		[UnitType.MediumTank] = 10,
	};

	public Mesh BuildTilesMesh() {

		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var colors = new List<Color>();
		var uvs = new List<Vector2>();

		foreach (var position in main.tiles.Keys) {
			Color color = main.TryGetBuilding(position, out var building) && building.Player != null ? building.Player.color : default;
			color.a = tileIds[main.tiles[position]];
			foreach (var vertex in Quad(position.ToVector3Int())) {
				vertices.Add(vertex);
				triangles.Add(triangles.Count);
				colors.Add(color);
			}
			foreach (var uv in Quad(position.ToVector3Int() + Vector3.one / 2))
				uvs.Add(uv.ToVector2());
		}

		var mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.colors = colors.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		return mesh;
	}

	public Mesh BuildUnitsMesh() {

		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var colors = new List<Color>();
		var uvs = new List<Vector2>();

		foreach (var unit in main.units.Values) {
			Color color = unit.Player.color;
			color.a = unitIds[unit.type];
			if (unit.Position is not { } position)
				continue;
			foreach (var vertex in Quad(position.ToVector3Int())) {
				vertices.Add(vertex + Vector3.up * .1f);
				triangles.Add(triangles.Count);
				colors.Add(color);
			}
			foreach (var uv in Quad(position.ToVector3Int() + Vector3.one / 2))
				uvs.Add(uv.ToVector2());
		}

		var mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.colors = colors.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		return mesh;
	}

	public static IEnumerable<Vector3> Quad(Vector3 position) {
		var right = Vector3.right / 2;
		var forward = Vector3.forward / 2;

		yield return position + right + forward;
		yield return position - right - forward;
		yield return position - right + forward;

		yield return position - right - forward;
		yield return position + right + forward;
		yield return position + right - forward;
	}

	public Mesh MakeTextMesh(string text, Vector2Int position) {

		textMeshPro.enabled = true;
		
		textMeshPro.transform.position = position.ToVector3Int() + Vector3.up * .01f;
		textMeshPro.text = text;
		textMeshPro.ForceMeshUpdate();

		var mesh = new Mesh();
		mesh.CombineMeshes(new[] {
			new CombineInstance {
				mesh = textMeshPro.mesh,
				transform = textMeshPro.transform.localToWorldMatrix
			}
		});

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		
		textMeshPro.enabled = false;

		return mesh;
	}

	[ContextMenu(nameof(Rebuild))]
	public void Rebuild() {

		var tilesMesh = BuildTilesMesh();
		var unitsMesh = BuildUnitsMesh();
		var textMeshes = new List<Mesh>();

		var positions = new HashSet<Vector2Int>();
		positions.UnionWith(main.tiles.Keys);
		positions.UnionWith(main.buildings.Keys);
		positions.UnionWith(main.units.Keys);

		foreach (var position in positions) {

			var text = "";

			//if (level.TryGetBuilding(position, out var building))
			//text += $"{building.type}";
			//else if (level.TryGetTile(position, out var tileType))
			//text += $"{tileType}";

			text += $"{position.x}, {position.y}\n";

			//if (level.TryGetUnit(position, out var unit))
			//	text += $"<color=#{ColorUtility.ToHtmlStringRGB(unit.player.color)}>{unit.type}</color>\n";

			textMeshes.Add(MakeTextMesh(text, position));
		}

		mesh = new Mesh();

		mesh.CombineMeshes(new[] {
			new CombineInstance {
				mesh = tilesMesh,
				transform = Matrix4x4.identity
			},
			new CombineInstance {
				mesh = unitsMesh,
				transform = Matrix4x4.identity
			},
		}.Concat(textMeshes.Select(textMesh => new CombineInstance {
			mesh = textMesh,
			transform = Matrix4x4.identity
		})).ToArray());

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		var subMesh0Count = (int)tilesMesh.GetIndexCount(0);
		var subMesh1Count = (int)unitsMesh.GetIndexCount(0);
		var subMesh2Count = (int)mesh.GetIndexCount(0) - subMesh0Count - subMesh1Count;

		mesh.subMeshCount = 3;
		mesh.SetSubMesh(0, new SubMeshDescriptor(0, subMesh0Count));
		mesh.SetSubMesh(1, new SubMeshDescriptor(subMesh0Count, subMesh1Count));
		mesh.SetSubMesh(2, new SubMeshDescriptor(subMesh0Count + subMesh1Count, subMesh2Count));

		if (meshFilter)
			meshFilter.mesh = mesh;
		
		Debug.Log("Rebuilt minimap");
	}

	public void Update() {
		if (Input.GetKeyDown(KeyCode.M)) {
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				Rebuild();
			else if (renderCamera && meshRenderer) {
				renderCamera.enabled = !renderCamera.enabled;
				meshRenderer.enabled = renderCamera.enabled;
				if (renderCamera.enabled)
					Rebuild();
			}
		}
	}
}