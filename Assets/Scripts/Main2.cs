using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using static Rules;

public class Main2 : Main {

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public MeshFilter triggersMeshFilter;

    public UnitTypeUnitViewDictionary unitPrefabs = new();
    public TileTypeBuildingViewDictionary buildingPrefabs = new();
    public int autosaveLifespanInDays = 30;

    private void Start() {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        PushState("LevelEditor", Run());
    }

    [Command]
    public float pathFindingDrawDuration = 10;

    [Command]
    public void FindPath(Vector2Int start, Vector2Int goal) {
        if (units.TryGetValue(start, out var unit)) {
            traverser.Traverse(tiles.Keys, start, GetMoveCostFunction(unit, false), goal: goal);
            using (Draw.ingame.WithDuration(pathFindingDrawDuration)) {
                Draw.ingame.CircleXZ((Vector3)goal.ToVector3Int(), .4f, Color.cyan);
                List<Vector2Int> path = null;
                if (traverser.TryReconstructPath(goal, ref path)) {
                    for (var i = 1; i < path.Count; i++)
                        Draw.ingame.Arrow((Vector3)path[i - 1].ToVector3Int(), (Vector3)path[i].ToVector3Int(), Color.black);
                }
            }
        }
    }

    [Command]
    public bool TrySetPlayerColor(int index, Color color) {
        if (index < 0 || index >= players.Count)
            return false;
        var player = players[index];
        player.Color = color;
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
    [Command]
    public bool TrySetPlayerCredits(int index, int amount) {
        if (index < 0 || index >= players.Count)
            return false;
        players[index].Credits = amount;
        return true;
    }
    [Command]
    public bool TrySetPlayerCreditsMax(int index, int amount) {
        if (index < 0 || index >= players.Count)
            return false;
        players[index].maxCredits = amount;
        return true;
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

    [Command]
    public void UpdatePlayBorderStyle() {

        Assert.IsTrue(guiSkin);

        playBorderTexture = new Texture2D(1, 1);
        playBorderTexture.SetPixel(0, 0, playBorderColor);
        playBorderTexture.Apply();

        playBorderStyle = new GUIStyle(guiSkin.label);
        playBorderStyle.normal.background = playBorderTexture;
        playBorderStyle.alignment = TextAnchor.MiddleRight;
        playBorderStyle.normal.textColor = Color.black;

        playBorderStyle.onNormal = playBorderStyle.normal;
    }

    public string playModeText = "[PLAYING]";

    public GUISkin guiSkin;

    public string stackLeakFormat = "<b><color=yellow>{0}</color></b>";
    
    protected void OnGUI() {

        GUI.skin = guiSkin;

        if (showPlayBorder) {
            var width = Screen.width;
            var height = playBorderHeight;

            if (showPlayBorderOnTop) {
                var rect = new Rect(0, 0, width, height);
                GUI.Label(rect, playModeText, playBorderStyle);
            }
            if (showPlayBorderOnBottom) {
                var rect = new Rect(0, Screen.height - height, width, height);
                GUI.Label(rect, GUIContent.none, playBorderStyle);
            }
        }

        var topLine = "";
        topLine += "[";
        topLine += stack.Count < 10 ? stack.Count.ToString() : string.Format(stackLeakFormat,stack.Count);
        topLine += "]: ";
        topLine += string.Join(" / ", stateNames.Zip(states, (n, s) => (n, s)).Select(ns => {
            var stateName = ns.n;
            var state = ns.s;
            var text = /*stateName.EndsWith("State") ? stateName.Substring(0, stateName.Length - "State".Length) :*/ stateName;
            //if (readyForInputStates.Contains(state))
            //    text = '[' + text + ']';
            return text;
        }).Reverse());
        GUILayout.Label(topLine);

        if (showPlayInfo) {
            var filled = CurrentPlayer.AbilityMeter;
            var max = MaxAbilityMeter(CurrentPlayer);
            if (AbilityInUse(CurrentPlayer))
                filled = max;
            var abilityStripe = new string('*', filled) + new string(' ', max - filled);
            if (AbilityInUse(CurrentPlayer))
                abilityStripe += " [ACTIVE]";
            if (filled == max)
                abilityStripe = $"<color=#{ColorUtility.ToHtmlStringRGB(fullAbilityStripeColor)}>{abilityStripe}</color>";
            var credits = CurrentPlayer.Credits;
            var playerHtmlColor = ColorUtility.ToHtmlStringRGB(CurrentPlayer.Color);
            GUILayout.Label($"{turn} / <color=#{playerHtmlColor}>{CurrentPlayer.co.name}</color> / {credits} / {abilityStripe}");
        }
        else
            foreach (var (name, value) in screenText.Where(kv => screenTextFilters.Any(filter => filter(kv.Key))).OrderBy(kv => kv.Key))
                GUILayout.Label($"{name}: {value()}");

        if (inspectedUnit != null) {

            CameraRig.TryFind(out var cameraRig);

            GUILayout.Space(25);

            GUILayout.Label($"type:\t{inspectedUnit.type}");
            GUILayout.Label($"player:\t{inspectedUnit.Player}");
            GUILayout.Label($"position:\t{inspectedUnit.Position}");
            GUILayout.Label($"moved:\t{inspectedUnit.Moved}");
            GUILayout.Label($"hp:\t{inspectedUnit.Hp} / {MaxHp(inspectedUnit)}");
            GUILayout.Label($"moveDistance:\t{MoveCapacity(inspectedUnit)}");

            GUILayout.Label($"fuel:\t{inspectedUnit.Fuel} / {MaxFuel(inspectedUnit)}");
            var weaponNames = GetWeaponNames(inspectedUnit).ToArray();
            if (weaponNames.Length>0) {
                GUILayout.Label($"ammo:");
                foreach (var weaponName in weaponNames) {
                    var amount = inspectedUnit.GetAmmo(weaponName);
                    var text = amount > 10000  && MaxAmmo(inspectedUnit, weaponName) == int.MaxValue ? "∞" : $"{amount} / {MaxAmmo(inspectedUnit, weaponName)}";
                    GUILayout.Label($"- {weaponName}:\t{text}");
                }
            }

            if (inspectedUnit.Carrier is { Disposed: false }) {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"carrier:\t{inspectedUnit.Carrier}");
                if (GUILayout.Button(jumpButtonText)) {
                    if (cameraRig)
                        cameraRig.Jump(inspectedUnit.Carrier.view.transform.position);
                }
                GUILayout.EndHorizontal();
            }
            if (inspectedUnit.Cargo.Count > 0) {
                GUILayout.Label($"cargo ({inspectedUnit.Cargo.Sum(c => Weight(c))} / {CarryCapacity(inspectedUnit)}):");
                foreach (var cargo in inspectedUnit.Cargo)
                    GUILayout.Label($"- {cargo} ({Weight(cargo)})");
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label($"view:\t{inspectedUnit.view}");
            if (GUILayout.Button(jumpButtonText)) {
                if (cameraRig)
                    cameraRig.Jump(inspectedUnit.view.transform.position);
            }
            GUILayout.EndHorizontal();
        }
    }

    public string jumpButtonText = "➲";

    public float inspectionCircleRadius = .4f;
    public float inspectionCircleLineWidth = 2;
    [ColorUsage(false, true)] public Color inspectionCircleColor = Color.yellow;
    public Color inspectionTextColor = Color.yellow;
    public Color attackPositionColor = Color.red;

    public Color fullAbilityStripeColor = Color.yellow;

    [Command]
    public bool TrySetPlayerAbilityMeter(int index, int value) {
        if (index < 0 || index >= players.Count)
            return false;
        players[index].AbilityMeter = value;
        return true;
    }
    [Command]
    public bool TrySetPlayerAbilityActivationTurn(int index, int? value) {
        if (index < 0 || index >= players.Count)
            return false;
        players[index].abilityActivationTurn = value;
        return true;
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
                if (File.Exists(filePath + ".meta"))
                    File.Delete(filePath + ".meta");
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

        base.OnApplicationQuit();

        Save("autosave");
        DeleteOldAutosaves();

        Clear();
        if (Player.undisposed.Count > 0)
            Debug.LogError($"undisposed players: {Player.undisposed.Count}");
        if (Building.undisposed.Count > 0)
            Debug.LogError($"undisposed buildings: {Building.undisposed.Count}");
        if (Unit.undisposed.Count > 0)
            Debug.LogError($"undisposed units: {Unit.undisposed.Count}");
        if (UnitAction.undisposed.Count > 0)
            Debug.LogError($"undisposed unit actions: {UnitAction.undisposed.Count}");
    }

    protected override void Update() {
        base.Update();

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
                    Draw.ingame.Label2D(center.ToVector3(), $"Bridge{index}: {bridge.Hp}", 14, LabelAlignment.Center, Color.black);
                }
            }

        if (Input.GetKeyDown(KeyCode.Return) && Mouse.TryGetPosition(out Vector2Int mousePosition))
            TryGetUnit(mousePosition, out inspectedUnit);


        if (inspectedUnit != null) {
            if (inspectedUnit.Disposed)
                inspectedUnit = null;
            else if (inspectedUnit.Position is { } position) {
                Vector3 position3d = position.ToVector3Int();

                using (Draw.ingame.WithLineWidth(inspectionCircleLineWidth)) {
                    Draw.ingame.CircleXZ(position3d, inspectionCircleRadius, inspectionCircleColor);
                }

                var attackPositions = Enumerable.Empty<Vector2Int>();
                if (TryGetAttackRange(inspectedUnit, out var attackRange)) {
                    if (IsArtillery(inspectedUnit))
                        attackPositions = PositionsInRange(position, attackRange);
                    else {
                        //traverser.Traverse(tiles.Keys, position, Rules.MoveCost(), Rules.MoveDistance(inspectedUnit));
                    }
                }

                foreach (var attackPosition in attackPositions)
                    Draw.ingame.SolidPlane((Vector3)attackPosition.ToVector3Int() + Vector3.up * offset, Vector3.up, Vector2.one, attackPositionColor);
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
    public const string inspectUnit = prefix + "inspect-unit";

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
    public bool TrySetBridgeHp(int hp) {
        if (!Mouse.TryGetPosition(out Vector2Int position) || !TryFindBridge(position, out var bridge))
            return false;
        bridge.SetHp(hp);
        return true;
    }
    [Command]
    public bool TryRemoveBridge() {
        if (!Mouse.TryGetPosition(out Vector2Int position) || !TryFindBridge(position, out var bridge))
            return false;
        bridges.Remove(bridge);
        return true;
    }

    [Command]
    public void RemoveTrigger(TriggerName triggerName) {
        Assert.IsTrue(triggers.ContainsKey(triggerName));
        triggers[triggerName].Clear();
    }

    [Command]
    public bool TrySetUnitHp(int hp) {
        if (!Mouse.TryGetPosition(out Vector2Int position) || !TryGetUnit(position, out var unit))
            return false;
        unit.SetHp(hp);
        return true;
    }

    [Command]
    public bool TrySetUnitFuel(int fuel) {
        if (!Mouse.TryGetPosition(out Vector2Int position) || !TryGetUnit(position, out var unit))
            return false;
        unit.Fuel = fuel;
        return true;
    }

    [Command]
    public void ResetToDefaultLevel() {
        
        Clear();
        
        var red = new Player(this, Palette.red, Team.Alpha, credits: 16000, unitLookDirection: Vector2Int.right);
        var blue = new Player(this, Palette.blue, Team.Bravo, credits: 16000, unitLookDirection: Vector2Int.left);
        localPlayer = red;
        player = red;
        
        RebuildTilemapMesh();
    }

    public IEnumerator<StateChange> Run() {

        if (!TryGetLatestSaveFilePath("autosave", out var path))
            ResetToDefaultLevel();
        else
            Load("autosave");

        screenText["tile-type"] = () => tileType;
        screenText["player"] = () => players.IndexOf(player);
        screenText["unit-type"] = () => unitType;
        screenText["trigger-name"] = () => triggerName;

        yield return StateChange.ReplaceWith(nameof(TilesMode), TilesMode());
    }

    public TileType tileType = TileType.Plain;
    public TileType[] tileTypes = { TileType.Plain, TileType.Road, TileType.Forest, TileType.Mountain, TileType.River, TileType.Sea, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard, TileType.MissileSilo };

    public Player player;

    public IEnumerator<StateChange> TilesMode() {

        ClearScreenTextFilters();
        AddScreenTextPrefixFilter("tiles-mode.");
        AddScreenTextFilter("tile-type", "player", "look-direction");

        if (CursorView.TryFind(out var cursorView))
            cursorView.show = true;

        while (true) {
            yield return StateChange.none;

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
                stack.Push(addPosition);
                commands.Enqueue(placeTile);
            }
            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int removePosition)) {
                stack.Push(removePosition);
                commands.Enqueue(removeTile);
            }
            else if (Input.GetKeyDown(KeyCode.F5))
                commands.Enqueue(play);

            // else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
            //     stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
            //     commands.Enqueue(cycleLookDirection);
            // }
            else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
                stack.Push(pickPosition);
                commands.Enqueue(pickTile);
            }

            while (commands.TryDequeue(out var command))
                foreach (var token in Tokenizer.Tokenize(command))
                    switch (token) {

                        case selectUnitsMode:
                            yield return StateChange.ReplaceWith(nameof(UnitsMode), UnitsMode());
                            break;

                        case cycleTileType:
                            tileType = tileType.Cycle(tileTypes, stack.Pop<int>());
                            break;

                        case cyclePlayer: {
                            player = player.Cycle(players.Concat(new[] { (Player)null }), stack.Pop<int>());
                            break;
                        }

                        case placeTile: {

                            var position = stack.Pop<Vector2Int>();
                            var tileType = stack.Pop<TileType>();
                            var player = stack.Pop<Player>();

                            if (tiles.ContainsKey(position))
                                TryRemoveTile(position, false);

                            tiles.Add(position, tileType);
                            if (TileType.Buildings.HasFlag(tileType)) {

                                var viewPrefab = BuildingView.DefaultPrefab;
                                if (buildingPrefabs.TryGetValue(tileType, out var v) && v)
                                    viewPrefab = v;
                                Assert.IsTrue(viewPrefab);

                                new Building(this, position, tileType, player, viewPrefab: viewPrefab, lookDirection: player?.unitLookDirection ?? Vector2Int.up);
                            }

                            RebuildTilemapMesh();

                            break;
                        }

                        case removeTile:
                            TryRemoveTile(stack.Pop<Vector2Int>(), true);
                            RebuildTilemapMesh();
                            break;

                        case play:
                            yield return StateChange.Push(nameof(Play), Play());
                            yield return StateChange.ReplaceWith(nameof(TilesMode), TilesMode());
                            break;

                        case pickTile: {
                            var position = stack.Pop<Vector2Int>();
                            if (tiles.TryGetValue(position, out var pickedTileType))
                                tileType = pickedTileType;
                            if (buildings.TryGetValue(position, out var building)) {
                                player = building.Player;
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

    [Command]
    public string InspectPlayer(int index) {
        if (index < 0 || index > players.Count)
            return null;

        using var sw = new StringWriter();
        GameWriter.WritePlayer(sw, players[index]);
        return sw.ToString();
    }

    [Command]
    public string InspectUnit() {
        if (!Mouse.TryGetPosition(out Vector2Int position)|| !TryGetUnit(position, out var unit))
            return null;

        using var sw = new StringWriter();
        GameWriter.WriteUnit(sw, unit);
        return sw.ToString();
    }

    [Command]
    public string InspectBuilding() {
        if (!Mouse.TryGetPosition(out Vector2Int position)||!TryGetBuilding(position, out var building))
            return null;

        using var sw = new StringWriter();
        GameWriter.WriteBuilding(sw, building);
        return sw.ToString();
    }

    [Command]
    public string InspectBridge() {
        if (!Mouse.TryGetPosition(out Vector2Int position)||!TryGetBridge(position, out var bridge))
            return null;

        using var sw = new StringWriter();
        GameWriter.WriteBridge(sw, bridge);
        return sw.ToString();
    }

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
            var color = buildings.TryGetValue(position, out var building) && building.Player != null
                ? building.Player.Color
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

    public IEnumerator<StateChange> UnitsMode() {

        if (player == null) {
            Assert.AreNotEqual(0, players.Count);
            player = players[0];
        }

        ClearScreenTextFilters();
        AddScreenTextPrefixFilter("units-mode.");
        AddScreenTextFilter("unit-type", "look-direction", "player");

        if (CursorView.TryFind(out var cursorView))
            cursorView.show = true;

        while (true) {
            yield return StateChange.none;

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
                commands.Enqueue(placeUnit);
            }
            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out position)) {
                stack.Push(position);
                commands.Enqueue(removeUnit);
            }
            // else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
            //     stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
            //     commands.Enqueue(cycleLookDirection);
            // }
            else if (Input.GetKeyDown(KeyCode.F5))
                commands.Enqueue(play);

            else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out position)) {
                stack.Push(position);
                commands.Enqueue(pickUnit);
            }
            /*else if (Input.GetKeyDown(KeyCode.Return) && Mouse.TryGetPosition(out position)) {
                stack.Push(position);
                commands.Enqueue(inspectUnit);
            }*/

            while (commands.TryDequeue(out var command))
                foreach (var token in Tokenizer.Tokenize(command))
                    switch (token) {

                        case selectTriggersMode:
                            yield return StateChange.ReplaceWith(nameof(TriggersMode), TriggersMode());
                            break;

                        case cyclePlayer:
                            player = player.Cycle(players, stack.Pop<int>());
                            break;

                        case cycleUnitType:
                            unitType = unitType.Cycle(unitTypes, stack.Pop<int>());
                            break;

                        case play:
                            yield return StateChange.Push(nameof(Play), Play());
                            yield return StateChange.ReplaceWith(nameof(UnitsMode), UnitsMode());
                            break;

                        case placeUnit: {

                            var position = stack.Pop<Vector2Int>();
                            var unitType = stack.Pop<UnitType>();
                            var player = stack.Pop<Player>();

                            if (units.ContainsKey(position))
                                TryRemoveUnit(position);

                            var viewPrefab = UnitView.DefaultPrefab;
                            if (player.co.unitTypesInfoOverride.TryGetValue(unitType, out var record) && record.viewPrefab)
                                viewPrefab = record.viewPrefab;
                            else if (UnitTypesInfo.TryGet(unitType, out record) && record.viewPrefab)
                                viewPrefab = record.viewPrefab;

                            new Unit(player, unitType, position, player.unitLookDirection, viewPrefab: viewPrefab);
                            break;
                        }

                        case removeUnit:
                            TryRemoveUnit(stack.Pop<Vector2Int>());
                            break;

                        case pickUnit: {
                            var position = stack.Pop<Vector2Int>();
                            if (units.TryGetValue(position, out var unit)) {
                                unitType = unit.type;
                                player = unit.Player;
                            }
                            break;
                        }

                        case inspectUnit: {
                            var position = stack.Pop<Vector2Int>();
                            units.TryGetValue(position, out inspectedUnit);
                            break;
                        }

                        default:
                            stack.ExecuteToken(token);
                            break;
                    }
        }
    }

    public Unit inspectedUnit;

    public bool TryRemoveUnit(Vector2Int position) {
        if (!units.TryGetValue(position, out var unit))
            return false;
        unit.Dispose();
        return true;
    }

    public IEnumerator<StateChange> TriggersMode() {

        ClearScreenTextFilters();
        AddScreenTextPrefixFilter("triggers-mode.");
        AddScreenTextFilter("trigger-name");

        if (CursorView.TryFind(out var cursorView))
            cursorView.show = true;

        while (true) {
            yield return StateChange.none;

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
                foreach (var token in Tokenizer.Tokenize(command))
                    switch (token) {

                        case selectTilesMode:
                            yield return StateChange.ReplaceWith(nameof(TilesMode), TilesMode());
                            break;

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
                            yield return StateChange.Push(nameof(Play), Play());
                            yield return StateChange.ReplaceWith(nameof(TriggersMode), TriggersMode());
                            break;

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

                Draw.ingame.SolidPlane((Vector3)position.ToVector3Int() + Vector3.up * offset, Vector3.up, Vector2.one, color);
                Draw.ingame.Label2D((Vector3)position.ToVector3Int(), string.Join(",", triggerNames), Color.white);
            }
        }
    }

    public float offset = .01f;
    public TriggerNameColorDictionary triggerColors = new() {
        [TriggerName.A] = Color.red,
        [TriggerName.B] = Color.green,
        [TriggerName.C] = Color.blue,
        [TriggerName.D] = Color.cyan,
        [TriggerName.E] = Color.yellow,
        [TriggerName.F] = Color.magenta
    };

    public bool showPlayInfo;

    public IEnumerator<StateChange> Play() {

        autoplay = true;

        ClearScreenTextFilters();
        AddScreenTextPrefixFilter("play-mode.");

        if (CursorView.TryFind(out var cursorView))
            cursorView.show = false;

        using var tw = new StringWriter();
        GameWriter.Write(tw, this);
        var save = tw.ToString();
        // Debug.Log(save);
        var playerIndex = players.IndexOf(player);
        levelLogic = new LevelLogic();

        UpdatePlayBorderStyle();
        showPlayInfo = true;
        showPlayBorder = true;

        //if (aiPlayerCommander)
//            aiPlayerCommander.StartPlaying();

        yield return StateChange.Push(nameof(SelectionState), SelectionState.Run(this, true));

  //      if (aiPlayerCommander)
    //        aiPlayerCommander.StopPlaying();

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
        if (TryGetLatestSaveFilePath(name, out var filePath)) {
            File.Delete(filePath);
            if (File.Exists(filePath + ".meta"))
                File.Delete(filePath + ".meta");
        }
    }
}

[Serializable]
public class TriggerNameColorDictionary : SerializableDictionary<TriggerName, Color> { }