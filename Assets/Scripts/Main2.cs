using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

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
    public bool TrySetPlayerColor(int index, Color color) {

        if (index < 0 || index >= players.Count)
            return false;

        var player = players[index];
        player.color = color;

        foreach (var unit in units.Values)
            RecursivelySetUnitColor(player, unit, color);

        foreach (var building in FindBuildingsOf(player))
            building.view.PlayerColor = color;

        return true;
    }
    [Command]
    public bool TrySetPlayerUnitLookDirection(int index, Vector2Int lookDirection) {
        Assert.AreEqual(1, lookDirection.ManhattanLength());
        if (index < 0 || index >= players.Count)
            return false;
        players[index].unitLookDirection = lookDirection;
        return true;
    }

    /// <summary>
    /// Recursive goes through the units' cargos and update all units' view color for a specified player.
    /// </summary>
    public void RecursivelySetUnitColor(Player player, Unit unit, Color color) {
        if (unit.player == player && unit.view)
            unit.view.PlayerColor = color;
        foreach (var cargo in unit.cargo)
            RecursivelySetUnitColor(player, cargo, color);
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

    public override void Awake() {
        base.Awake();
    }

    [Command]
    public void UpdatePlayBorderStyle() {

        playBorderTexture = new Texture2D(1, 1);
        playBorderTexture.SetPixel(0, 0, playBorderColor);
        playBorderTexture.Apply();

        playBorderStyle = new GUIStyle {
            normal = new GUIStyleState {
                background = playBorderTexture
            }
        };
        playBorderStyle.onNormal = playBorderStyle.normal;
    }

    protected override void OnGUI() {
        base.OnGUI();

        foreach (var (name, value) in screenText.Where(kv => screenTextFilters.Any(filter => filter(kv.Key))).OrderBy(kv => kv.Key))
            GUILayout.Label($"{name}: {value()}");

        if (showPlayBorder) {
            var width = Screen.width;
            var height = playBorderHeight;

            if (showPlayBorderOnTop) {
                var rect = new Rect(0, 0, width, height);
                GUI.Label(rect, GUIContent.none, playBorderStyle);
            }
            if (showPlayBorderOnBottom) {
                var rect = new Rect(0, Screen.height - height, width, height);
                GUI.Label(rect, GUIContent.none, playBorderStyle);
            }
        }

        if (showPlayInfo) {
            GUILayout.Label($"turn: {turn}");
        }
    }

    public Texture2D playBorderTexture;
    public bool showPlayBorder;
    public bool showPlayBorderOnTop = true;
    public bool showPlayBorderOnBottom = true;
    public float playBorderHeight = 5;
    public Color playBorderColor = Color.green;
    public GUIStyle playBorderStyle;

    public int DeleteAutosaves(Predicate<string> predicate) {
        var count = 0;
        foreach (var filePath in GetSaveFilePaths("autosave").ToArray()) {
            if (predicate(filePath)) {
                File.Delete(filePath);
                count++;
            }
        }
        return count;
    }

    [Command]
    public int DeleteOldAutosaves() {
        return DeleteAutosaves(path => {
            var lastAccessTime = File.GetLastAccessTime(path);
            return DateTime.Now.Subtract(lastAccessTime).Days > autosaveLifespanInDays;
        });
    }
    [Command]
    public int DeleteAllAutosaves() {
        return DeleteAutosaves(_ => true);
    }

    protected override void OnApplicationQuit() {
        Save("autosave");
        DeleteOldAutosaves();
        base.OnApplicationQuit();
    }

    private void Update() {
        if (showBridges)
            foreach (var bridge in bridges) {
                var index = bridges.IndexOf(bridge);
                if (bridge.tiles.Count > 0) {

                    var center = Vector2.zero;
                    var count = 0;
                    foreach (var position in bridge.tiles.Keys) {
                        center += position;
                        count++;

                        Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, Color.white);
                    }

                    center /= count;
                    Draw.ingame.Label2D(center.ToVector3(), $"Bridge{index}", 14, LabelAlignment.Center, Color.black);
                }
            }
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
    public TriggerName[] triggerNames = { TriggerName.A, TriggerName.B, TriggerName.C, TriggerName.D, TriggerName.E, TriggerName.F };
    public TriggerName triggerName = TriggerName.A;

    public bool showBridges;
    [Command]
    public void ToggleBridges() {
        showBridges = !showBridges;
    }

    public bool TryFindBridge(Vector2Int position, out Bridge bridge) {
        bridge = bridges.SingleOrDefault(b => b.tiles.Keys.Contains(position));
        return bridge != null;
    }

    [Command]
    public bool TrySetBridgeHp(Vector2Int position, int hp) {
        if (TryFindBridge(position, out var bridge)) {
            bridge.Hp = hp;
            return true;
        }
        return false;
    }
    [Command]
    public bool TryRemoveBridge(Vector2Int position) {
        if (TryFindBridge(position, out var bridge)) {
            bridges.Remove(bridge);
            return true;
        }
        return false;
    }

    [Command]
    public void RemoveTrigger(TriggerName triggerName) {
        Assert.IsTrue(triggers.ContainsKey(triggerName));
        triggers[triggerName].Clear();
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
        screenText["trigger-name"] = () => triggerName;

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
                            tileType = tileType.Cycle(tileTypes, stack.Pop<int>());
                            break;

                        case cyclePlayer: {
                            player = player.Cycle(players.Concat(new[] { (Player)null }), stack.Pop<int>());
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
                            lookDirection = lookDirection.Cycle(lookDirections, stack.Pop<int>());
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

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Color> colors = new List<Color>();

    private Mesh quadMesh;
    private Vector3[] quadVertices;
    private int[] quadTriangles;

    private Mesh forestMesh;
    private Vector3[] forestVertices;
    private int[] forestTriangles;

    private Mesh mountainMesh;
    private Vector3[] mountainVertices;
    private int[] mountainTriangles;

    public void RebuildTilemapMesh() {

        vertices.Clear();
        triangles.Clear();
        colors.Clear();

        void LoadMesh(string name, out Mesh mesh, out Vector3[] vertices, out int[] triangles) {
            mesh = name.LoadAs<Mesh>();
            vertices = mesh.vertices;
            triangles = mesh.triangles;
        }

        if (!quadMesh)
            LoadMesh("quad", out quadMesh, out quadVertices, out quadTriangles);
        if (!forestMesh)
            LoadMesh("forest-placeholder", out forestMesh, out forestVertices, out forestTriangles);
        if (!mountainMesh)
            LoadMesh("mountain-placeholder", out mountainMesh, out mountainVertices, out mountainTriangles);

        foreach (var position in tiles.Keys) {

            var tileType = tiles[position];
            var color = buildings.TryGetValue(position, out var building) && building.player.v != null
                ? building.player.v.color
                : Palette.white;
            color.a = (int)tiles[position];

            var source = tileType switch {
                TileType.Forest => (forestVertices, forestTriangles, Enumerable.Repeat(color, forestVertices.Length)),
                TileType.Mountain => (mountainVertices, mountainTriangles, Enumerable.Repeat(color, mountainVertices.Length)),
                _ => (quadVertices, quadTriangles, Enumerable.Repeat(color, quadVertices.Length))
            };

            Random.InitState(position.GetHashCode());
            MeshUtils.AppendMesh(
                (vertices, triangles, colors),
                source,
                Matrix4x4.TRS(position.ToVector3Int(), Quaternion.Euler(0, Random.Range(0, 4) * 90, 0), Vector3.one));
        }
        
        var mesh = new Mesh {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            colors = colors.ToArray()
        };
        
        mesh.Optimize();
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if (meshFilter.sharedMesh)
            Destroy(meshFilter.sharedMesh);
        if (meshCollider.sharedMesh)
            Destroy(meshCollider.sharedMesh);
        
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
                            player = player.Cycle(players, stack.Pop<int>());
                            lookDirection = player.unitLookDirection;
                            break;

                        case cycleUnitType:
                            unitType = unitType.Cycle(unitTypes, stack.Pop<int>());
                            break;

                        case cycleLookDirection:
                            lookDirection = lookDirection.Cycle(lookDirections, stack.Pop<int>());
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

    public IEnumerator TriggersMode() {

        ClearScreenTextFilters();
        AddScreenTextPrefixFilter("triggers-mode.");
        AddScreenTextFilter("trigger-name");

        if (CursorView.TryFind(out var cursorView))
            cursorView.Visible = true;

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
                            yield return TilesMode();
                            yield break;

                        case cycleTrigger:
                            triggerName = triggerName.Cycle(triggerNames, stack.Pop<int>());
                            Assert.IsTrue(triggers.ContainsKey(triggerName));
                            break;

                        case placeTrigger: {
                            var position = stack.Pop<Vector2Int>();
                            triggers[triggerName].Add(position);
                            break;
                        }

                        case removeTrigger: {
                            var position = stack.Pop<Vector2Int>();
                            foreach (var (_, set) in triggers)
                                set.Remove(position);
                            break;
                        }

                        case play:
                            yield return Play();
                            yield return TriggersMode();
                            yield break;

                        case pickTrigger: {
                            var position = stack.Pop<Vector2Int>();
                            var candidates = triggers.Where(kv => kv.Value.Contains(position)).Select(kv => kv.Key).ToArray();
                            if (candidates.Length > 0)
                                triggerName = triggerName.Cycle(candidates, 1);
                            break;
                        }

                        default:
                            stack.ExecuteToken(token);
                            break;
                    }

            var positions = triggers.SelectMany(t => t.Value).Distinct();
            foreach (var position in positions) {

                var triggerNames = triggers.Keys.Where(t => triggers[t].Contains(position)).ToArray();
                var color = Color.black;
                foreach (var triggerName in triggerNames) {
                    if (triggerColors.TryGetValue(triggerName, out var triggerColor))
                        color += triggerColor;
                }

                Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, color);
                Draw.ingame.Label2D((Vector3)position.ToVector3Int(), string.Join(",", triggerNames), Color.white);
            }
        }
    }

    public TriggerNameColorDictionary triggerColors = new() {
        [TriggerName.A] = Color.red,
        [TriggerName.B] = Color.green,
        [TriggerName.C] = Color.blue,
        [TriggerName.D] = Color.cyan,
        [TriggerName.E] = Color.yellow,
        [TriggerName.F] = Color.magenta
    };

    public bool showPlayInfo;

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

        UpdatePlayBorderStyle();
        showPlayInfo = true;
        showPlayBorder = true;

        while (true) {
            yield return null;
            if (Input.GetKeyDown(KeyCode.F5)) {
                StopCoroutine(play);
                break;
            }
        }

        showPlayInfo = false;
        showPlayBorder = false;

        LoadFromText(save);
        player = playerIndex == -1 ? null : players[playerIndex];
    }

    public static string SaveRootDirectoryPath => Path.Combine(Application.dataPath, "Saves");
    public static string GetSavePath(string name) => Path.Combine(SaveRootDirectoryPath, name);

    public void SaveInternal(string name) {
        using var tw = new StringWriter();
        GameWriter.Write(tw, this);
        var text = tw.ToString();
        if (!Directory.Exists(SaveRootDirectoryPath))
            Directory.CreateDirectory(SaveRootDirectoryPath);
        var path = GetSavePath(name);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var saveName = name + "-" + DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE")).Replace(":", ".").Replace(" ", "-") + ".txt";
        var filePath = Path.Combine(path, saveName);
        File.WriteAllText(filePath, text);
        Debug.Log($"Saved to: {filePath}");
    }

    [Command]
    public void Save(string name) {
        SaveInternal(name);
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

        foreach (var set in triggers.Values)
            set.Clear();

        foreach (var bridge in bridges)
            bridge.view.bridge = null;
        bridges.Clear();

        RebuildTilemapMesh();
    }

    public class ReadingOptions {
        public bool clearGame = true;
        public string saveName;
        public string input = "";
        public bool spawnBuildingViews = true;
        public bool checkLocalPlayerIsSet = true;
        public bool selectExistingPlayersInsteadOfCreatingNewOnes = false;
        public Vector2Int transform = Vector2Int.one;
        public bool loadCameraRig = true;
    }

    public void LoadInternal(ReadingOptions options) {

        if (options.clearGame)
            Clear();

        string input;
        if (options.saveName != null) {
            var found = TryGetLatestSaveFilePath(options.saveName, out var filePath);
            Assert.IsTrue(found, options.saveName);
            Debug.Log($"Reading from: {filePath}");
            input = File.ReadAllText(filePath);
        }
        else
            input = options.input;

        GameReader.ReadInto(this, input, options.transform, options.spawnBuildingViews, options.selectExistingPlayersInsteadOfCreatingNewOnes,
            options.loadCameraRig);

        player = players.Count == 0 ? null : players[0];

        RebuildTilemapMesh();

        if (options.checkLocalPlayerIsSet && players.Count > 0)
            Assert.IsNotNull(localPlayer, "local player is not set");
    }

    [Command]
    public void LoadTransformed(string name, Vector2Int transform) {
        LoadInternal(new ReadingOptions { saveName = name, transform = transform });
    }

    [Command]
    public void Load(string name) {
        LoadInternal(new ReadingOptions { saveName = name });
    }
    [Command]
    public void LoadAdditively(string name) {
        LoadInternal(new ReadingOptions {
            saveName = name,
            clearGame = false,
            selectExistingPlayersInsteadOfCreatingNewOnes = true,
            loadCameraRig = false
        });
    }
    [Command]
    public void LoadFromText(string text) {
        LoadInternal(new ReadingOptions { input = text });
    }
    [Command]
    public void LoadFromTextAdditively(string text) {
        LoadInternal(new ReadingOptions {
            input = text,
            clearGame = false,
            selectExistingPlayersInsteadOfCreatingNewOnes = true,
            loadCameraRig = false
        });
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

[Serializable]
public class TriggerNameColorDictionary : SerializableDictionary<TriggerName, Color> { }