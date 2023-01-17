using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class Main2 : Main {

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public LevelEditor2 levelEditor;

    public UnitTypeUnitViewDictionary unitPrefabs = new();
    public TileTypeBuildingViewDictionary buildingPrefabs = new();

    private void Start() {
        levelEditor = new LevelEditor2(this, meshFilter, meshCollider);
        StartCoroutine(levelEditor.Run());
    }

    [Command]
    public void LoadColors() {
        levelEditor.LoadColors();
    }
    [Command]
    public void Save(string name) {
        levelEditor.Save(name);
    }
    [Command]
    public void Load(string name) {
        levelEditor.Load(name);
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
    public void OpenSaveFile(string name) {
        levelEditor.OpenSaveFile(name);
    }
    [Command]
    public void PopSaveFile(string name) {
        levelEditor.PopSaveFile(name);
    }

    protected override void OnApplicationQuit() {
        levelEditor.Save("autosave");
        base.OnApplicationQuit();
    }
}

[Serializable]
public class UnitTypeUnitViewDictionary : SerializableDictionary<UnitType, UnitView> { }

[Serializable]
public class TileTypeBuildingViewDictionary : SerializableDictionary<TileType, BuildingView> { }

public class LevelEditor2 {

    public const string prefix = "level-editor.";

    public const string selectTilesMode = prefix + "select-tiles-mode";
    public const string selectUnitsMode = prefix + "select-units-mode";

    public const string cycleTileType = prefix + "cycle-tile-type";
    public const string placeTile = prefix + "place-tile";
    public const string removeTile = prefix + "remove-tile";

    public const string cycleUnitType = prefix + "cycle-unit";
    public const string placeUnit = prefix + "place-unit";
    public const string removeUnit = prefix + "remove-unit";
    public const string cycleLookDirection = prefix + "cycle-look-direction";

    public const string pickTile = prefix + "pick-tile";
    public const string pickUnit = prefix + "pick-unit";

    public const string cyclePlayer = prefix + "cycle-players";
    public const string play = prefix + "play";

    public const string mode = nameof(mode);
    public const string autosave = prefix + "autosave";

    public Stack<(Action perform, Action revert)> undos = new();
    public Stack<(Action perform, Action revert)> redos = new();

    public Main2 main;
    public LevelEditorTextDisplay textDisplay;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    public LevelEditor2(Main2 main, MeshFilter meshFilter, MeshCollider meshCollider) {
        this.main = main;
        this.meshFilter = meshFilter;
        this.meshCollider = meshCollider;
    }

    public Vector2Int lookDirection = Vector2Int.right;
    public Vector2Int[] lookDirections = Rules.offsets;

    public IEnumerator Run() {

        main.Clear();

        // var red = new Player(main, Palette.red, Team.Alpha, credits: 16000, unitLookDirection: Vector2Int.right);
        // var blue = new Player(main, Palette.blue, Team.Bravo, credits: 16000, unitLookDirection: Vector2Int.left);
        // main.localPlayer = red;
        // player = red;
        //
        // var min = new Vector2Int(-5, -5);
        // var max = new Vector2Int(5, 5);
        //
        // for (var y = min.y; y <= max.y; y++)
        // for (var x = min.x; x <= max.x; x++)
        //     main.tiles.Add(new Vector2Int(x, y), TileType.Plain);
        //
        // new Building(main, min, TileType.Hq, red, viewPrefab: "WbFactory".LoadAs<BuildingView>());
        // new Building(main, max, TileType.Hq, blue, viewPrefab: "WbFactory".LoadAs<BuildingView>());
        //
        // new Unit(red, UnitType.Infantry, min);
        // new Unit(blue, UnitType.Infantry, max);
        //
        // LoadColors();
        // RebuildTilemapMesh();
        //
        // if (main.players.Count > 0)
        //     lookDirection = main.players[0].unitLookDirection;

        Load("autosave");

        main.screenText["tile-type"] = () => tileType;
        main.screenText["player"] = () => main.players.IndexOf(player);
        main.screenText["look-direction"] = () => lookDirection;
        main.screenText["unit-type"] = () => unitType;

        yield return TilesMode();
    }

    public TileType tileType = TileType.Plain;
    public TileType[] tileTypes = { TileType.Plain, TileType.Road, TileType.Forest, TileType.Mountain, TileType.River, TileType.Sea, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard };

    public Player player;

    public IEnumerator TilesMode() {

        main.ClearScreenTextFilters();
        main.AddScreenTextPrefixFilter("tiles-mode.");
        main.AddScreenTextFilter("tile-type", "player", "look-direction");

        if (CursorView.TryFind(out var cursorView))
            cursorView.Visible = true;

        while (true) {
            yield return null;

            if (Input.GetKeyDown(KeyCode.F8))
                main.commands.Enqueue(selectUnitsMode);
            else if (Input.GetKeyDown(KeyCode.Tab)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cycleTileType);
            }
            else if (Input.GetKeyDown(KeyCode.F2)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cyclePlayer);
            }
            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int addPosition)) {
                main.stack.Push(player);
                main.stack.Push(tileType);
                main.stack.Push(lookDirection);
                main.stack.Push(addPosition);
                main.commands.Enqueue(placeTile);
            }
            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int removePosition)) {
                main.stack.Push(removePosition);
                main.commands.Enqueue(removeTile);
            }
            else if (Input.GetKeyDown(KeyCode.F5))
                main.commands.Enqueue(play);

            else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
                main.stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
                main.commands.Enqueue(cycleLookDirection);
            }
            else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
                main.stack.Push(pickPosition);
                main.commands.Enqueue(pickTile);
            }

            while (main.commands.TryDequeue(out var command))
                foreach (var token in command.Tokenize())
                    switch (token) {

                        case selectUnitsMode:
                            yield return UnitsMode();
                            yield break;

                        case cycleTileType:
                            tileType = CycleValue(tileType, tileTypes, main.stack.Pop<int>());
                            break;

                        case cyclePlayer: {
                            var playersWithNull = new List<Player>(main.players);
                            playersWithNull.Add(null);
                            player = CycleValue(player, playersWithNull, main.stack.Pop<int>());
                            lookDirection = player?.unitLookDirection ?? Vector2Int.up;
                            break;
                        }

                        case placeTile: {

                            var position = main.stack.Pop<Vector2Int>();
                            var lookDirection = main.stack.Pop<Vector2Int>();
                            var tileType = main.stack.Pop<TileType>();
                            var player = main.stack.Pop<Player>();

                            if (main.tiles.ContainsKey(position))
                                TryRemoveTile(position, false);

                            main.tiles.Add(position, tileType);
                            if (TileType.Buildings.HasFlag(tileType))
                                new Building(main, position, tileType, player, viewPrefab: main.buildingPrefabs[tileType],
                                    lookDirection: lookDirection);

                            RebuildTilemapMesh();

                            break;
                        }

                        case removeTile:
                            TryRemoveTile(main.stack.Pop<Vector2Int>(), true);
                            RebuildTilemapMesh();
                            break;

                        case cycleLookDirection:
                            lookDirection = CycleValue(lookDirection, lookDirections, main.stack.Pop<int>());
                            break;

                        case play:
                            yield return Play();
                            yield return TilesMode();
                            yield break;

                        case pickTile: {
                            var position = main.stack.Pop<Vector2Int>();
                            if (main.tiles.TryGetValue(position, out var pickedTileType))
                                tileType = pickedTileType;
                            if (main.buildings.TryGetValue(position, out var building)) {
                                player = building.player.v;
                            }
                            break;
                        }

                        default:
                            main.stack.ExecuteToken(token);
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
        foreach (var position in main.tiles.Keys) {
            var tileType = main.tiles[position];
            var color = main.buildings.TryGetValue(position, out var building) && building.player.v != null
                ? building.player.v.color
                : this.colors.TryGetValue(tileType, out var htmlColor) && ColorUtility.TryParseHtmlString(htmlColor, out var c)
                    ? c
                    : Palette.white;
            color.a = (int)main.tiles[position];
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
        if (!main.tiles.ContainsKey(position))
            return false;
        main.tiles.Remove(position);
        if (main.buildings.TryGetValue(position, out var building))
            building.Dispose();
        if (removeUnit && main.units.TryGetValue(position, out var unit))
            unit.Dispose();
        return true;
    }

    public UnitType unitType = UnitType.Infantry;
    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.Recon, UnitType.LightTank, UnitType.MediumTank, UnitType.Rockets, };

    public IEnumerator UnitsMode() {

        if (player == null) {
            Assert.AreNotEqual(0, main.players.Count);
            player = main.players[0];
            lookDirection = player.unitLookDirection;
        }

        main.ClearScreenTextFilters();
        main.AddScreenTextPrefixFilter("units-mode.");
        main.AddScreenTextFilter("unit-type", "look-direction", "player");

        if (CursorView.TryFind(out var cursorView))
            cursorView.Visible = true;

        while (true) {
            yield return null;

            if (Input.GetKeyDown(KeyCode.F8))
                main.commands.Enqueue(selectTilesMode);

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cycleUnitType);
            }
            else if (Input.GetKeyDown(KeyCode.F2)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cyclePlayer);
            }
            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int position) && main.tiles.ContainsKey(position)) {
                main.stack.Push(player);
                main.stack.Push(unitType);
                main.stack.Push(position);
                main.stack.Push(lookDirection);
                main.commands.Enqueue(placeUnit);
            }
            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int position2)) {
                main.stack.Push(position2);
                main.commands.Enqueue(removeUnit);
            }
            else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
                main.stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
                main.commands.Enqueue(cycleLookDirection);
            }
            else if (Input.GetKeyDown(KeyCode.F5))
                main.commands.Enqueue(play);

            else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
                main.stack.Push(pickPosition);
                main.commands.Enqueue(pickUnit);
            }

            while (main.commands.TryDequeue(out var command))
                foreach (var token in command.Tokenize())
                    switch (token) {

                        case selectTilesMode:
                            yield return TilesMode();
                            yield break;

                        case cyclePlayer:
                            player = CycleValue(player, main.players, main.stack.Pop<int>());
                            lookDirection = player.unitLookDirection;
                            break;

                        case cycleUnitType:
                            unitType = CycleValue(unitType, unitTypes, main.stack.Pop<int>());
                            break;

                        case cycleLookDirection:
                            lookDirection = CycleValue(lookDirection, lookDirections, main.stack.Pop<int>());
                            break;

                        case play:
                            yield return Play();
                            yield return UnitsMode();
                            yield break;

                        case placeUnit: {

                            var lookDirection = main.stack.Pop<Vector2Int>();
                            var position = main.stack.Pop<Vector2Int>();
                            var unitType = main.stack.Pop<UnitType>();
                            var player = main.stack.Pop<Player>();

                            if (main.units.ContainsKey(position))
                                TryRemoveUnit(position);

                            var viewPrefab = main.unitPrefabs.TryGetValue(unitType, out var p) ? p : UnitView.DefaultPrefab;
                            new Unit(player, unitType, position, lookDirection, viewPrefab: viewPrefab);

                            break;
                        }

                        case removeUnit:
                            TryRemoveUnit(main.stack.Pop<Vector2Int>());
                            break;

                        case pickUnit: {
                            var position = main.stack.Pop<Vector2Int>();
                            if (main.units.TryGetValue(position, out var unit)) {
                                unitType = unit.type;
                                player = unit.player;
                                lookDirection = player.unitLookDirection;
                            }
                            break;
                        }

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }

    public bool TryRemoveUnit(Vector2Int position) {
        if (!main.units.TryGetValue(position, out var unit))
            return false;
        unit.Dispose();
        return true;
    }

    public IEnumerator Play() {

        main.ClearScreenTextFilters();
        main.AddScreenTextPrefixFilter("play-mode.");

        if (CursorView.TryFind(out var cursorView))
            cursorView.Visible = false;

        using var tw = new StringWriter();
        GameWriter.Write(tw, main);
        var save = tw.ToString();
        Debug.Log(save);
        var playerIndex = main.players.IndexOf(player);
        main.levelLogic = new LevelLogic();
        var play = SelectionState.Run(main, true);
        main.StartCoroutine(play);
        while (true) {
            yield return null;
            if (Input.GetKeyDown(KeyCode.F5)) {
                main.StopCoroutine(play);
                break;
            }
        }

        main.Clear();
        GameReader.LoadInto(main, save, true);
        player = playerIndex == -1 ? null : main.players[playerIndex];
    }

    public static string SaveRootDirectoryPath => Path.Combine(Application.dataPath, "Saves");
    public static string GetSavePath(string name) => Path.Combine(SaveRootDirectoryPath, name);

    public void Save(string name) {
        using var tw = new StringWriter();
        GameWriter.Write(tw, main);
        var text = tw.ToString();
        if (!Directory.Exists(SaveRootDirectoryPath))
            Directory.CreateDirectory(SaveRootDirectoryPath);
        var path = GetSavePath(name);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var saveName = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE")) + ".txt";
        var filePath = Path.Combine(path, saveName);
        File.WriteAllText(filePath, text);
        Debug.Log($"Saved to: {filePath}");
    }

    public static bool TryGetLatestSaveFilePath(string name, out string filePath) {
        filePath = default;
        var path = GetSavePath(name);
        if (!Directory.Exists(path))
            return false;
        var files = Directory.GetFiles(path).Where(path=>path.EndsWith(".txt")).ToArray();
        if (files.Length == 0)
            return false;
        filePath = files.OrderBy(File.GetLastWriteTime).Last();
        return true;
    }

    public void Load(string name) {
        var found = TryGetLatestSaveFilePath(name, out var filePath);
        Assert.IsTrue(found, name);
        var text = File.ReadAllText(filePath);
        Debug.Log($"Reading from: {filePath}");
        main.Clear();
        GameReader.LoadInto(main, text, true);
        player = main.players.Count == 0 ? null : main.players[0];
        RebuildTilemapMesh();
    }

    public static T CycleValue<T>(T value, T[] values, int offset = 1) {
        var index = Array.IndexOf(values, value);
        Assert.AreNotEqual(-1, index);
        return values[(index + offset).PositiveModulo(values.Length)];
    }
    public static T CycleValue<T>(T value, List<T> values, int offset = 1) {
        var index = values.IndexOf(value);
        Assert.AreNotEqual(-1, index);
        return values[(index + offset).PositiveModulo(values.Count)];
    }

    public void OpenSaveFile(string name) {
        var found = TryGetLatestSaveFilePath(name, out var filePath);
        
        Assert.IsTrue(found);
        ProcessStartInfo startInfo = new ProcessStartInfo("/usr/local/bin/subl");
        startInfo.WindowStyle = ProcessWindowStyle.Normal;
        startInfo.Arguments = '"' + filePath + '"';

        Process.Start(startInfo);
    }

    public void PopSaveFile(string name) {
        if( TryGetLatestSaveFilePath(name, out var filePath))
            File.Delete(filePath);
    }
}