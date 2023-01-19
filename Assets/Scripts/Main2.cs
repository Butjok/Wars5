using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class Main2 : Main {

	public MeshFilter meshFilter;
	public MeshCollider meshCollider;
	public MeshFilter triggersMeshFilter;

	public UnitTypeUnitViewDictionary unitPrefabs = new();
	public TileTypeBuildingViewDictionary buildingPrefabs = new();
	public int autosaveLifespanInDays = 30;

	private void Start() {
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		StartCoroutine(Run());
	}

	[Command]
	public void DebugSetPlayerColor(int index, Color color) {
		if (index >= 0 && index < players.Count) {
			var player = players[index];
			player.color = color;
			foreach (var unit in units.Values)
				if (unit.player == player && unit.view)
					unit.view.PlayerColor = color;
			foreach (var building in FindBuildingsOf(player))
				building.view.PlayerColor = color;
		}
	}

	public Dictionary<string, Func<object>> screenText = new();
	public HashSet<Predicate<string>> screenTextFilters = new() { _ => true };

	public void ClearScreenTextFilters() {
		screenTextFilters.Clear();
	}
	public void ResetScreenTextFilters() {
		ClearScreenTextFilters();
		screenTextFilters.Add(_ => true);
	}
	public void AddScreenTextPrefixFilter(params string[] prefixes) {
		foreach (var prefix in prefixes)
			screenTextFilters.Add(name => name.StartsWith(prefix));
	}
	public void AddScreenTextFilter(params string[] names) {
		foreach (var name in names)
			screenTextFilters.Add(names.Contains);
	}
	protected override void OnGUI() {
		base.OnGUI();

		foreach (var (name, value) in screenText.Where(kv => screenTextFilters.Any(filter => filter(kv.Key))).OrderBy(kv => kv.Key))
			GUILayout.Label($"{name}: {value()}");
	}
	[Command]
	public int DeleteOldAutosaves() {
		var count = 0;
		foreach (var filePath in GetSaveFilePaths("autosave").ToArray()) {
			var lastAccessTime = File.GetLastAccessTime(filePath);
			if (DateTime.Now.Subtract(lastAccessTime).Days > autosaveLifespanInDays) {
				File.Delete(filePath);
				count++;
			}
		}
		return count;
	}

	protected override void OnApplicationQuit() {
		Save("autosave");
		DeleteOldAutosaves();
		base.OnApplicationQuit();
	}


	public const string prefix = "level-editor.";

	public const string selectTilesMode = prefix + "select-tiles-mode";
	public const string selectUnitsMode = prefix + "select-units-mode";
	public const string selectTriggersMode = prefix + "select-triggers-mode";
	public const string selectBridgesMode = prefix + "select-bridges-mode";

	public const string cycleTileType = prefix + "cycle-tile-type";
	public const string placeTile = prefix + "place-tile";
	public const string removeTile = prefix + "remove-tile";

	public const string placeTrigger = prefix + "place-trigger";
	public const string removeTrigger = prefix + "remove-trigger";

	public const string cycleUnitType = prefix + "cycle-unit";
	public const string placeUnit = prefix + "place-unit";
	public const string removeUnit = prefix + "remove-unit";
	public const string cycleLookDirection = prefix + "cycle-look-direction";

	public const string pickTile = prefix + "pick-tile";
	public const string pickTrigger = prefix + "pick-trigger";
	public const string pickUnit = prefix + "pick-unit";

	public const string cyclePlayer = prefix + "cycle-players";
	public const string play = prefix + "play";

	public const string cycleTrigger = prefix + "cycle-trigger";

	public const string mode = nameof(mode);
	public const string autosave = prefix + "autosave";

	public Stack<(Action perform, Action revert)> undos = new();
	public Stack<(Action perform, Action revert)> redos = new();

	public Vector2Int lookDirection = Vector2Int.right;
	public Vector2Int[] lookDirections = Rules.offsets;
	public Trigger[] triggerNames = { Trigger.A, Trigger.B, Trigger.C };
	public Trigger trigger = Trigger.A;

	public ThicknessSpace triggerThicknessSpace = ThicknessSpace.Pixels;
	public float triggerThickness = 5;
	public ShapesBlendMode triggerBlendMode = ShapesBlendMode.Additive;
	public CompareFunction triggerCompareFunction = CompareFunction.Always;
	public float triggerFillAlpha = .25f;
	public float triggerBorderRadius = .1f;

	[ColorUsage(false)] public Color triggerColorA = Color.red;
	[ColorUsage(false)] public Color triggerColorB = Color.green;
	[ColorUsage(false)] public Color triggerColorC = Color.blue;

	public Dictionary<Vector2Int, TextElement> textElements = new();
	public float triggerTextSize = 1;
	public Vector3 triggerTextOffset = new Vector3(0, 0.01f, 0);

	public bool showBridges;
	[Command]
	public void ToggleBridges() {
		showBridges = !showBridges;
	}

	public ThicknessSpace bridgesThicknessSpace = ThicknessSpace.Pixels;
	public float bridgesThickness = 5;
	public ShapesBlendMode bridgesBlendMode = ShapesBlendMode.Additive;
	public CompareFunction bridgesCompareFunction = CompareFunction.Always;
	public float bridgesFillAlpha = .25f;
	public float bridgesBorderRadius = .1f;
	public Color[] bridgesColorsCycle = new[] { Color.red, Color.green, Color.blue, };

	public int bridgeIndex = 0;
	public int maxBridges = 3;

	[Command]
	public void SetBridgeHp(Vector2Int position, int hp) {
		var bridge = bridges.SingleOrDefault(b => b.tiles.Keys.Contains(position));
		if (bridge == null)
			return;
		bridge.Hp = hp;
	}

	public override void DrawShapes(Camera cam) {
		using (Draw.Command(cam)) {

			if (showTriggers) {

				Draw.ThicknessSpace = triggerThicknessSpace;

				foreach (var (position, trigger) in triggers) {

					var color = Color.black;
					if (trigger.HasFlag(Trigger.A))
						color += triggerColorA;
					if (trigger.HasFlag(Trigger.B))
						color += triggerColorB;
					if (trigger.HasFlag(Trigger.C))
						color += triggerColorC;

					Draw.BlendMode = triggerBlendMode;
					Draw.ZTest = triggerCompareFunction;

					var position3d = position.ToVector3Int();
					var rotation = Quaternion.Euler(90, 0, 0);
					var quad = new Rect(-Vector2.one / 2, Vector2.one);

					Draw.RectangleBorder(position3d, rotation, quad, triggerThickness, triggerBorderRadius, color);

					var fillColor = color;
					fillColor.a = triggerFillAlpha;

					Draw.Rectangle(position3d, rotation, quad, triggerBorderRadius, fillColor);
					if (!textElements.TryGetValue(position, out var textElement)) {
						textElement = new TextElement();
						textElements.Add(position, textElement);
					}

					Draw.Text(textElement, position3d + triggerTextOffset, rotation, trigger.ToString(), triggerTextSize, color);
				}
			}

			if (showBridges) {

				Draw.ThicknessSpace = bridgesThicknessSpace;

				var index = 0;
				foreach (var bridge in bridges) {
					var color = bridgesColorsCycle.Length == 0 ? Color.white : bridgesColorsCycle[(index++) % bridgesColorsCycle.Length];

					foreach (var position in bridge.tiles.Keys) {

						Draw.BlendMode = bridgesBlendMode;
						Draw.ZTest = bridgesCompareFunction;

						var position3d = position.ToVector3Int();
						var rotation = Quaternion.Euler(90, 0, 0);
						var quad = new Rect(-Vector2.one / 2, Vector2.one);

						Draw.RectangleBorder(position3d, rotation, quad, bridgesThickness, bridgesBorderRadius, color);

						var fillColor = color;
						fillColor.a = bridgesFillAlpha;

						Draw.Rectangle(position3d, rotation, quad, bridgesBorderRadius, fillColor);
					}
				}
			}
		}
	}

	public override void OnDisable() {
		foreach (var textElement in textElements.Values)
			textElement.Dispose();
		textElements.Clear();
	}

	public IEnumerator Run() {

		Clear();

		if (!TryGetLatestSaveFilePath("autosave", out var path)) {
			var red = new Player(this, Palette.red, Team.Alpha, credits: 16000, unitLookDirection: Vector2Int.right);
			var blue = new Player(this, Palette.blue, Team.Bravo, credits: 16000, unitLookDirection: Vector2Int.left);
			localPlayer = red;
			player = red;

			var min = new Vector2Int(-5, -5);
			var max = new Vector2Int(5, 5);

			for (var y = min.y; y <= max.y; y++)
			for (var x = min.x; x <= max.x; x++)
				tiles.Add(new Vector2Int(x, y), TileType.Plain);

			new Building(this, min, TileType.Hq, red, viewPrefab: "WbFactory".LoadAs<BuildingView>());
			new Building(this, max, TileType.Hq, blue, viewPrefab: "WbFactory".LoadAs<BuildingView>());

			new Unit(red, UnitType.Infantry, min);
			new Unit(blue, UnitType.Infantry, max);

			LoadColors();
			RebuildTilemapMesh();

			if (players.Count > 0)
				lookDirection = players[0].unitLookDirection;
		}
		else
			Load("autosave");

		screenText["tile-type"] = () => tileType;
		screenText["player"] = () => players.IndexOf(player);
		screenText["look-direction"] = () => lookDirection;
		screenText["unit-type"] = () => unitType;
		screenText["trigger-name"] = () => trigger;
		screenText["bridge-index"] = () => bridgeIndex;

		yield return TilesMode();
	}

	public TileType tileType = TileType.Plain;
	public TileType[] tileTypes = { TileType.Plain, TileType.Road, TileType.Forest, TileType.Mountain, TileType.River, TileType.Sea, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard };

	public Player player;

	public IEnumerator TilesMode() {

		ClearScreenTextFilters();
		AddScreenTextPrefixFilter("tiles-mode.");
		AddScreenTextFilter("tile-type", "player", "look-direction");

		if (CursorView.TryFind(out var cursorView))
			cursorView.Visible = true;

		while (true) {
			yield return null;

			if (Input.GetKeyDown(KeyCode.F8))
				commands.Enqueue(selectUnitsMode);
			else if (Input.GetKeyDown(KeyCode.Tab)) {
				stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
				commands.Enqueue(cycleTileType);
			}
			else if (Input.GetKeyDown(KeyCode.F2)) {
				stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
				commands.Enqueue(cyclePlayer);
			}
			else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int addPosition)) {
				stack.Push(player);
				stack.Push(tileType);
				stack.Push(lookDirection);
				stack.Push(addPosition);
				commands.Enqueue(placeTile);
			}
			else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int removePosition)) {
				stack.Push(removePosition);
				commands.Enqueue(removeTile);
			}
			else if (Input.GetKeyDown(KeyCode.F5))
				commands.Enqueue(play);

			else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
				stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
				commands.Enqueue(cycleLookDirection);
			}
			else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
				stack.Push(pickPosition);
				commands.Enqueue(pickTile);
			}

			while (commands.TryDequeue(out var command))
				foreach (var token in command.Tokenize())
					switch (token) {

						case selectUnitsMode:
							yield return UnitsMode();
							yield break;

						case cycleTileType:
							tileType = CycleValue(tileType, tileTypes, stack.Pop<int>());
							break;

						case cyclePlayer: {
							var playersWithNull = new List<Player>(players);
							playersWithNull.Add(null);
							player = CycleValue(player, playersWithNull, stack.Pop<int>());
							lookDirection = player?.unitLookDirection ?? Vector2Int.up;
							break;
						}

						case placeTile: {

							var position = stack.Pop<Vector2Int>();
							var lookDirection = stack.Pop<Vector2Int>();
							var tileType = stack.Pop<TileType>();
							var player = stack.Pop<Player>();

							if (tiles.ContainsKey(position))
								TryRemoveTile(position, false);

							tiles.Add(position, tileType);
							if (TileType.Buildings.HasFlag(tileType))
								new Building(this, position, tileType, player, viewPrefab: buildingPrefabs[tileType],
									lookDirection: lookDirection);

							RebuildTilemapMesh();

							break;
						}

						case removeTile:
							TryRemoveTile(stack.Pop<Vector2Int>(), true);
							RebuildTilemapMesh();
							break;

						case cycleLookDirection:
							lookDirection = CycleValue(lookDirection, lookDirections, stack.Pop<int>());
							break;

						case play:
							yield return Play();
							yield return TilesMode();
							yield break;

						case pickTile: {
							var position = stack.Pop<Vector2Int>();
							if (tiles.TryGetValue(position, out var pickedTileType))
								tileType = pickedTileType;
							if (buildings.TryGetValue(position, out var building)) {
								player = building.player.v;
							}
							break;
						}

						default:
							stack.ExecuteToken(token);
							break;
					}
		}
	}

	public Dictionary<TileType, string> colors = new();

	public void LoadColors() {
		colors = "TileTypeColors".LoadAs<TextAsset>().text.FromJson<Dictionary<TileType, string>>();
		RebuildTilemapMesh();
	}

	public void RebuildTilemapMesh() {
		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var colors = new List<Color>();
		foreach (var position in tiles.Keys) {
			var tileType = tiles[position];
			var color = buildings.TryGetValue(position, out var building) && building.player.v != null
				? building.player.v.color
				: this.colors.TryGetValue(tileType, out var htmlColor) && ColorUtility.TryParseHtmlString(htmlColor, out var c)
					? c
					: Palette.white;
			color.a = (int)tiles[position];
			foreach (var vertex in MeshUtils.QuadAt(position.ToVector3Int())) {
				vertices.Add(vertex);
				triangles.Add(triangles.Count);
				colors.Add(color);
			}
		}
		var mesh = new Mesh {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			colors = colors.ToArray()
		};
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		meshFilter.sharedMesh = mesh;
		meshCollider.sharedMesh = mesh;
	}

	public bool TryRemoveTile(Vector2Int position, bool removeUnit) {
		if (!tiles.ContainsKey(position))
			return false;
		tiles.Remove(position);
		if (buildings.TryGetValue(position, out var building))
			building.Dispose();
		if (removeUnit && units.TryGetValue(position, out var unit))
			unit.Dispose();
		return true;
	}

	public UnitType unitType = UnitType.Infantry;
	public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.Recon, UnitType.LightTank, UnitType.MediumTank, UnitType.Rockets, };

	public IEnumerator UnitsMode() {

		if (player == null) {
			Assert.AreNotEqual(0, players.Count);
			player = players[0];
			lookDirection = player.unitLookDirection;
		}

		ClearScreenTextFilters();
		AddScreenTextPrefixFilter("units-mode.");
		AddScreenTextFilter("unit-type", "look-direction", "player");

		if (CursorView.TryFind(out var cursorView))
			cursorView.Visible = true;

		while (true) {
			yield return null;

			if (Input.GetKeyDown(KeyCode.F8))
				commands.Enqueue(selectTriggersMode);

			else if (Input.GetKeyDown(KeyCode.Tab)) {
				stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
				commands.Enqueue(cycleUnitType);
			}
			else if (Input.GetKeyDown(KeyCode.F2)) {
				stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
				commands.Enqueue(cyclePlayer);
			}
			else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int position) && tiles.ContainsKey(position)) {
				stack.Push(player);
				stack.Push(unitType);
				stack.Push(position);
				stack.Push(lookDirection);
				commands.Enqueue(placeUnit);
			}
			else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int position2)) {
				stack.Push(position2);
				commands.Enqueue(removeUnit);
			}
			else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
				stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
				commands.Enqueue(cycleLookDirection);
			}
			else if (Input.GetKeyDown(KeyCode.F5))
				commands.Enqueue(play);

			else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
				stack.Push(pickPosition);
				commands.Enqueue(pickUnit);
			}

			while (commands.TryDequeue(out var command))
				foreach (var token in command.Tokenize())
					switch (token) {

						case selectTriggersMode:
							yield return TriggersMode();
							yield break;

						case cyclePlayer:
							player = CycleValue(player, players, stack.Pop<int>());
							lookDirection = player.unitLookDirection;
							break;

						case cycleUnitType:
							unitType = CycleValue(unitType, unitTypes, stack.Pop<int>());
							break;

						case cycleLookDirection:
							lookDirection = CycleValue(lookDirection, lookDirections, stack.Pop<int>());
							break;

						case play:
							yield return Play();
							yield return UnitsMode();
							yield break;

						case placeUnit: {

							var lookDirection = stack.Pop<Vector2Int>();
							var position = stack.Pop<Vector2Int>();
							var unitType = stack.Pop<UnitType>();
							var player = stack.Pop<Player>();

							if (units.ContainsKey(position))
								TryRemoveUnit(position);

							var viewPrefab = unitPrefabs.TryGetValue(unitType, out var p) ? p : UnitView.DefaultPrefab;
							new Unit(player, unitType, position, lookDirection, viewPrefab: viewPrefab);

							break;
						}

						case removeUnit:
							TryRemoveUnit(stack.Pop<Vector2Int>());
							break;

						case pickUnit: {
							var position = stack.Pop<Vector2Int>();
							if (units.TryGetValue(position, out var unit)) {
								unitType = unit.type;
								player = unit.player;
								lookDirection = player.unitLookDirection;
							}
							break;
						}

						default:
							stack.ExecuteToken(token);
							break;
					}
		}
	}

	public bool TryRemoveUnit(Vector2Int position) {
		if (!units.TryGetValue(position, out var unit))
			return false;
		unit.Dispose();
		return true;
	}

	[Command]
	public bool showTriggers;

	public IEnumerator TriggersMode() {

		ClearScreenTextFilters();
		AddScreenTextPrefixFilter("triggers-mode.");
		AddScreenTextFilter("trigger-name");

		if (CursorView.TryFind(out var cursorView))
			cursorView.Visible = true;

		showTriggers = true;

		while (true) {
			yield return null;

			if (Input.GetKeyDown(KeyCode.F8))
				commands.Enqueue(selectTilesMode);

			else if (Input.GetKeyDown(KeyCode.Tab)) {
				stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
				commands.Enqueue(cycleTrigger);
			}
			else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int addPosition)) {
				stack.Push(addPosition);
				commands.Enqueue(placeTrigger);
			}
			else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int removePosition)) {
				stack.Push(removePosition);
				commands.Enqueue(removeTrigger);
			}
			else if (Input.GetKeyDown(KeyCode.F5))
				commands.Enqueue(play);

			else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
				stack.Push(pickPosition);
				commands.Enqueue(pickTrigger);
			}

			while (commands.TryDequeue(out var command))
				foreach (var token in command.Tokenize())
					switch (token) {

						case selectTilesMode:
							showTriggers = false;
							yield return TilesMode();
							yield break;

						case cycleTrigger:
							trigger = CycleValue(trigger, triggerNames, stack.Pop<int>());
							break;

						case placeTrigger: {

							var position = stack.Pop<Vector2Int>();

							triggers.TryGetValue(position, out var oldValue);
							var newValue = oldValue | trigger;
							triggers[position] = newValue;

							RebuildTriggersMesh();
							break;
						}

						case removeTrigger: {

							var position = stack.Pop<Vector2Int>();

							triggers.Remove(position);
							// this.triggers.TryGetValue(position, out var oldValue);
							// var newValue = oldValue & ~trigger;
							// if (newValue == 0)
							//     this.triggers.Remove(position);
							// else
							//     this.triggers[position] = newValue;

							RebuildTriggersMesh();
							break;
						}

						case play:
							showTriggers = false;
							yield return Play();
							yield return TriggersMode();
							yield break;

						case pickTrigger: {
							var position = stack.Pop<Vector2Int>();
							if (triggers.TryGetValue(position, out var newTrigger))
								trigger = newTrigger;
							break;
						}

						default:
							stack.ExecuteToken(token);
							break;
					}
		}
	}

	public void RebuildTriggersMesh() {
		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var colors = new List<Color>();
		foreach (var (position, trigger) in triggers) {
			var color = new Color(0, 0, 0, (int)trigger);
			foreach (var vertex in MeshUtils.QuadAt(position.ToVector3Int())) {
				vertices.Add(vertex);
				triangles.Add(triangles.Count);
				colors.Add(color);
			}
		}
		var mesh = new Mesh {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			colors = colors.ToArray()
		};
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		triggersMeshFilter.sharedMesh = mesh;
	}

	public IEnumerator Play() {

		ClearScreenTextFilters();
		AddScreenTextPrefixFilter("play-mode.");

		if (CursorView.TryFind(out var cursorView))
			cursorView.Visible = false;

		using var tw = new StringWriter();
		GameWriter.Write(tw, this);
		var save = tw.ToString();
		// Debug.Log(save);
		var playerIndex = players.IndexOf(player);
		levelLogic = new LevelLogic();
		var play = SelectionState.Run(this, true);
		StartCoroutine(play);
		while (true) {
			yield return null;
			if (Input.GetKeyDown(KeyCode.F5)) {
				StopCoroutine(play);
				break;
			}
		}

		Clear();
		GameReader.LoadInto(this, save, true);
		player = playerIndex == -1 ? null : players[playerIndex];
	}

	public static string SaveRootDirectoryPath => Path.Combine(Application.dataPath, "Saves");
	public static string GetSavePath(string name) => Path.Combine(SaveRootDirectoryPath, name);

	[Command]
	public void Save(string name) {
		using var tw = new StringWriter();
		GameWriter.Write(tw, this);
		var text = tw.ToString();
		if (!Directory.Exists(SaveRootDirectoryPath))
			Directory.CreateDirectory(SaveRootDirectoryPath);
		var path = GetSavePath(name);
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
		var saveName = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE")).Replace(":", ".").Replace(" ", "-") + ".txt";
		var filePath = Path.Combine(path, saveName);
		File.WriteAllText(filePath, text);
		Debug.Log($"Saved to: {filePath}");
	}

	public static bool TryGetLatestSaveFilePath(string name, out string filePath) {
		filePath = default;
		var path = GetSavePath(name);
		if (!Directory.Exists(path))
			return false;
		var files = GetSaveFilePaths(name).ToArray();
		if (files.Length == 0)
			return false;
		filePath = files.OrderBy(File.GetLastWriteTime).Last();
		return true;
	}

	public static IEnumerable<string> GetSaveFilePaths(string name) {
		var path = GetSavePath(name);
		if (!Directory.Exists(path))
			return Enumerable.Empty<string>();
		return Directory.GetFiles(path).Where(p => p.EndsWith(".txt"));
	}

	[Command]
	public void Clear() {

		turn = 0;

		foreach (var player in players.ToArray())
			player.Dispose();
		players.Clear();

		localPlayer = null;

		tiles.Clear();

		foreach (var unit in units.Values.ToArray())
			unit.Dispose();
		units.Clear();

		foreach (var building in buildings.Values.ToArray())
			building.Dispose();
		buildings.Clear();

		triggers.Clear();

		foreach (var bridge in bridges)
			bridge.view.bridge = null;
		bridges.Clear();

		RebuildTilemapMesh();
	}

	public void LoadInternal(string name, bool clear = true) {
		var found = TryGetLatestSaveFilePath(name, out var filePath);
		Assert.IsTrue(found, name);
		var text = File.ReadAllText(filePath);
		Debug.Log($"Reading from: {filePath}");
		if (clear)
			Clear();
		GameReader.LoadInto(this, text, true);
		player = players.Count == 0 ? null : players[0];
		RebuildTilemapMesh();
	}
	[Command]
	public void Load(string name) {
		LoadInternal(name, true);
	}
	[Command]
	public void LoadAdditively(string name) {
		LoadInternal(name, false);
	}

	public static T CycleValue<T>(T value, T[] values, int offset = 1) {
		Assert.AreNotEqual(0, values.Length);
		var index = Array.IndexOf(values, value);
		var nextIndex = index == -1 ? 0 : (index + offset).PositiveModulo(values.Length);
		return values[nextIndex];
	}
	public static T CycleValue<T>(T value, List<T> values, int offset = 1) {
		Assert.AreNotEqual(0, values.Count);
		var index = values.IndexOf(value);
		var nextIndex = index == -1 ? 0 : (index + offset).PositiveModulo(values.Count);
		return values[nextIndex];
	}

	[Command]
	public void OpenSaveFile(string name) {
		var found = TryGetLatestSaveFilePath(name, out var filePath);

		Assert.IsTrue(found);
		ProcessStartInfo startInfo = new ProcessStartInfo("/usr/local/bin/subl");
		startInfo.WindowStyle = ProcessWindowStyle.Normal;
		startInfo.Arguments = '"' + filePath + '"';

		Process.Start(startInfo);
	}

	[Command]
	public void PopSaveFile(string name) {
		if (TryGetLatestSaveFilePath(name, out var filePath))
			File.Delete(filePath);
	}
}