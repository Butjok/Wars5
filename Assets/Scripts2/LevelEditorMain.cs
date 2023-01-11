using System;
using System.Collections;
using System.Collections.Generic;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelEditorMain : Main {

    public TMP_Text textDisplay;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public LevelEditor2 levelEditor;

    private void Start() {
        levelEditor = new LevelEditor2(this, textDisplay, meshFilter, meshCollider);
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
}

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

    public const string cyclePlayer = prefix + "cycle-players";
    public const string play = prefix + "play";

    public const string mode = nameof(mode);
    public const string autosave = prefix + "autosave";

    public Main main;
    public LevelEditorTextDisplay textDisplay;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    public LevelEditor2(Main main, TMP_Text text, MeshFilter meshFilter, MeshCollider meshCollider) {
        this.main = main;
        this.meshFilter = meshFilter;
        this.meshCollider = meshCollider;
        textDisplay = new LevelEditorTextDisplay(text);
        textDisplay.Clear();
    }

    public Vector2Int lookDirection = Vector2Int.up;
    public Vector2Int[] lookDirections = Rules.offsets;

    public IEnumerator Run() {

        main.Clear();

        var red = new Player(main, Color.red, Team.Alpha, credits:16000);
        var blue = new Player(main, Color.blue, Team.Bravo, credits:16000);
        main.localPlayer = red;
        player = red;

        var min = new Vector2Int(-5, -5);
        var max = new Vector2Int(5, 5);

        for (var y = min.y; y <= max.y; y++)
        for (var x = min.x; x <= max.x; x++)
            main.tiles.Add(new Vector2Int(x, y), TileType.Plain);

        new Building(main, min, TileType.Hq, red);
        new Building(main, max, TileType.Hq, blue);

        new Unit(red, UnitType.Infantry, min);
        new Unit(blue, UnitType.Infantry, max);

        LoadColors();
        RebuildTilemapMesh();

        yield return TilesMode();
    }

    public TileType tileType = TileType.Plain;
    public TileType[] tileTypes = { TileType.Plain, TileType.Road, TileType.Sea, TileType.Mountain, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard };

    public Player player;

    public IEnumerator TilesMode() {

        textDisplay.Clear();
        textDisplay.Set(mode, nameof(TilesMode));
        textDisplay.Set(nameof(tileType), tileType);
        textDisplay.Set(nameof(player), player);

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
            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int position)) {
                main.stack.Push(player);
                main.stack.Push(tileType);
                main.stack.Push(position);
                main.commands.Enqueue(placeTile);
            }
            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int position2)) {
                main.stack.Push(position2);
                main.commands.Enqueue(removeTile);
            }
            else if (Input.GetKeyDown(KeyCode.F5))
                main.commands.Enqueue(play);

            while (main.commands.TryDequeue(out var command))
                foreach (var token in command.Tokenize())
                    switch (token) {

                        case selectUnitsMode:
                            yield return UnitsMode();
                            yield break;

                        case cycleTileType:
                            tileType = CycleValue(tileType, tileTypes, main.stack.Pop<int>());
                            textDisplay.Set(nameof(tileType), tileType);
                            break;

                        case cyclePlayer:
                            player = CycleValue(player, main.players, main.stack.Pop<int>());
                            textDisplay.Set(nameof(player), player);
                            break;

                        case placeTile: {

                            var position = main.stack.Pop<Vector2Int>();
                            var tileType = main.stack.Pop<TileType>();
                            var player = main.stack.Pop<Player>();

                            if (main.tiles.ContainsKey(position))
                                TryRemoveTile(position);

                            main.tiles.Add(position, tileType);
                            if (TileType.Buildings.HasFlag(tileType))
                                new Building(main, position, tileType, player);

                            RebuildTilemapMesh();

                            break;
                        }

                        case removeTile:
                            TryRemoveTile(main.stack.Pop<Vector2Int>());
                            RebuildTilemapMesh();
                            break;

                        case play:
                            yield return Play();
                            yield return TilesMode();
                            yield break;

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
                    : new Color(1,1,1,0);
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

    public bool TryRemoveTile(Vector2Int position) {
        if (!main.tiles.ContainsKey(position))
            return false;
        main.tiles.Remove(position);
        if (main.buildings.TryGetValue(position, out var building))
            building.Dispose();
        return true;
    }

    public UnitType unitType = UnitType.Infantry;
    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.TransportHelicopter, UnitType.AttackHelicopter, UnitType.FighterJet, UnitType.Bomber, UnitType.Recon, UnitType.LightTank, UnitType.Rockets, };

    public IEnumerator UnitsMode() {

        textDisplay.Clear();
        textDisplay.Set(mode, nameof(UnitsMode));
        textDisplay.Set(nameof(unitType), unitType);
        textDisplay.Set(nameof(lookDirection), lookDirection);
        textDisplay.Set(nameof(player), player);

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
            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int position)) {
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

            while (main.commands.TryDequeue(out var command))
                foreach (var token in command.Tokenize())
                    switch (token) {

                        case selectTilesMode:
                            yield return TilesMode();
                            yield break;

                        case cyclePlayer:
                            player = CycleValue(player, main.players, main.stack.Pop<int>());
                            textDisplay.Set(nameof(player), player);
                            break;

                        case cycleUnitType:
                            unitType = CycleValue(unitType, unitTypes, main.stack.Pop<int>());
                            textDisplay.Set(nameof(unitType), unitType);
                            break;

                        case cycleLookDirection:
                            lookDirection = CycleValue(lookDirection, lookDirections, main.stack.Pop<int>());
                            textDisplay.Set(nameof(lookDirection), lookDirection);
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

                            new Unit(player, unitType, position, lookDirection);

                            break;
                        }

                        case removeUnit:
                            TryRemoveUnit(main.stack.Pop<Vector2Int>());
                            break;

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
        var save = GameSaver.SaveToString(main);
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
        GameLoader.Load(main, save);
        player = main.players[playerIndex];
    }

    public void Save(string name) {
        PlayerPrefs.SetString(name, GameSaver.SaveToString(main));
    }
    public void Load(string name) {
        var commands = PlayerPrefs.GetString(name);
        Assert.IsNotNull(commands);
        GameLoader.Load(main, commands);
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
}