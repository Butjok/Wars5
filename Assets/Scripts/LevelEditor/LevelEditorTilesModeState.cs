using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Vector2Int = UnityEngine.Vector2Int;

public class LevelEditorTilesModeState : StateMachineState {

    public enum Command {
        CycleTileType,
        PlaceTile,
        RemoveTile,
        PickTile,
        CyclePlayer,
        ToggleMode
    }

    public enum Mode {
        Add,
        Remove
    }

    private static readonly List<Vector3> vertices = new();
    private static readonly List<int> triangles = new();
    private static readonly List<Color> colors = new();

    private static Mesh quadMesh;
    private static Vector3[] quadVertices;
    private static int[] quadTriangles;

    private static Mesh forestMesh;
    private static Vector3[] forestVertices;
    private static int[] forestTriangles;

    private static Mesh mountainMesh;
    private static Vector3[] mountainVertices;
    private static int[] mountainTriangles;

    public TileType[] tileTypes = {
        TileType.Plain,
        TileType.Beach,
        TileType.Road,
        TileType.Bridge,
        TileType.BridgeSea,
        TileType.Forest,
        TileType.Mountain,
        TileType.TunnelEntrance,
        TileType.River,
        TileType.Sea,
        TileType.City,
        TileType.Hq,
        TileType.Factory,
        TileType.Airport,
        TileType.Shipyard,
        TileType.MissileSilo,
        TileType.MissileStorage,
        TileType.PipeSection
    };
    public TileType tileType = TileType.Plain;
    public Player player;
    public Mode mode;
    public GameObject tileMeshGameObject;
    public Material tileMeshMaterial;
    public TileMapCreator tileMapCreator;
    public RoadCreator roadCreator;
    public ForestCreator forestCreator;
    public PropPlacement propPlacement;
    public HeightMapBaker heightMapBaker;
    public float height;

    [Command] public static Color plainColor = Color.green;
    [Command] public static Color roadColor = Color.gray;
    [Command] public static Color seaColor = Color.blue;
    [Command] public static Color mountainColor = new(0.5f, 0.37f, 0.22f);
    [Command] public static Color forestColor = new(0.09f, 0.51f, 0.2f);
    [Command] public static Color riverColor = Color.cyan;
    [Command] public static Color beachColor = Color.yellow;
    [Command] public static Color unownedBuildingColor = Color.white;

    public bool showTiles = false;
    [Command] static public bool showHeights = true;

    public static Color GetColor(TileType tileType) {
        return tileType switch {
            TileType.Plain => plainColor,
            TileType.Road or TileType.Bridge or TileType.BridgeSea => roadColor,
            TileType.Sea => seaColor,
            TileType.Mountain => mountainColor,
            TileType.Forest => forestColor,
            TileType.River => riverColor,
            TileType.Beach => beachColor,
            _ => Color.magenta
        };
    }

    public void RebuildTilemapMesh() {
        var levelEditorState = stateMachine.Find<LevelEditorSessionState>();
        var level = levelEditorState.level;
        var tiles = level.tiles;
        var buildings = level.buildings;
        var tileMeshFilter = levelEditorState.tileMeshFilter;
        var tileMeshCollider = levelEditorState.tileMeshCollider;

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
                : Color.white;
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

        if (tileMeshFilter.sharedMesh)
            Object.Destroy(tileMeshFilter.sharedMesh);
        if (tileMeshCollider.sharedMesh)
            Object.Destroy(tileMeshCollider.sharedMesh);

        tileMeshFilter.sharedMesh = mesh;
        tileMeshCollider.sharedMesh = mesh;

        // TerrainCreator
        if (tileMeshMaterial) {
            var oldTexture = tileMeshMaterial.GetTexture("_TileMap");
            if (oldTexture)
                Object.Destroy(oldTexture);
            if (level.tiles.Count > 0) {
                var minX = level.tiles.Keys.Select(v => v.x).Min();
                var maxX = level.tiles.Keys.Select(v => v.x).Max();
                var minY = level.tiles.Keys.Select(v => v.y).Min();
                var maxY = level.tiles.Keys.Select(v => v.y).Max();
                var min = new Vector2Int(minX, minY);
                var max = new Vector2Int(maxX, maxY);
                var size = max - min + Vector2Int.one;
                var worldToTileMap = Matrix4x4.TRS((min - new Vector2(.5f, .5f)).ToVector3(), Quaternion.identity, new Vector3(size.x, 1, size.y)).inverse;
                tileMeshMaterial.SetMatrix("_WorldToTileMap", worldToTileMap);
                var texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false) {
                    filterMode = FilterMode.Point
                };
                var game = stateMachine.TryFind<GameSessionState>().game;
                for (var y = min.y; y <= max.y; y++)
                for (var x = min.x; x <= max.x; x++) {
                    var position = new Vector2Int(x, y);
                    texture.SetPixel(x - minX, y - minY,
                        level.TryGetTile(position, out var tileType)
                            ? game.GetColor(tileType, level.TryGetBuilding(position, out var building) ? building : null)
                            : Color.clear);
                }

                texture.Apply();
                tileMeshMaterial.SetTexture("_TileMap", texture);
            }
            else
                tileMeshMaterial.SetTexture("_TileMap", null);
        }
    }

    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.TryFind<GameSessionState>().game;
            var editorState = stateMachine.TryFind<LevelEditorSessionState>();
            var gui = editorState.gui;
            var level = editorState.level;
            var tiles = level.tiles;
            var buildings = level.buildings;
            var units = level.units;
            var camera = level.view.cameraRig.camera;

            var terrainCreator = Object.FindObjectOfType<TerrainCreator>();
            if (terrainCreator) {
                var mesh = terrainCreator.mesh;
            }

            tileMapCreator = Object.FindObjectOfType<TileMapCreator>(true);
            roadCreator = Object.FindObjectOfType<RoadCreator>(true);
            forestCreator = Object.FindObjectOfType<ForestCreator>(true);
            propPlacement = Object.FindObjectOfType<PropPlacement>(true);
            heightMapBaker = Object.FindObjectOfType<HeightMapBaker>(true);

            if (tileMapCreator) {
                //tileMapCreator.TryLoad(tileMapCreator.loadOnAwakeFileName);
                tileMapCreator.tiles.Clear();
                foreach (var (position, tileType) in tiles)
                    tileMapCreator.tiles.Add(position, tileType);
                tileMapCreator.RebuildPieces();
            }

            if (roadCreator) {
                roadCreator.tiles = tiles.Where(p => p.Value is TileType.Road or TileType.Bridge or TileType.BridgeSea or TileType.TunnelEntrance || (TileType.Buildings & p.Value) != 0).ToDictionary(p => p.Key, p => p.Value);
                roadCreator.Rebuild();
            }

            if (forestCreator) {
                forestCreator.trees.Clear();
                foreach (var position in tiles.Keys.Where(p => tiles[p] == TileType.Forest))
                    forestCreator.PlaceTreesAt(position);
            }

            void UpdatePipeSectionViews() {
                foreach (var pipeSection in level.pipeSections.Values)
                    pipeSection.UpdateView();
            }

            void TryRemoveTile(Vector2Int position, bool removeUnit) {
                if (!tiles.TryGetValue(position, out var tileType))
                    return;

                if (level.bridges2.TryGetValue(position, out var bridge))
                    bridge.Destroy();

                tiles.Remove(position);

                if (tileMapCreator) {
                    tileMapCreator.tiles.Remove(position);
                    tileMapCreator.RebuildPieces();

                    if (tileMapCreator.terrainMapper)
                        tileMapCreator.terrainMapper.ClearBushes();
                }

                if (roadCreator) {
                    if (tileType is TileType.Road or TileType.Bridge or TileType.BridgeSea or TileType.TunnelEntrance || (TileType.Buildings & tileType) != 0)
                        roadCreator.tiles.Remove(position);
                    roadCreator.Rebuild();
                }

                if (forestCreator) {
                    if (tileType == TileType.Forest)
                        forestCreator.RemoveTreesAt(position);
                    forestCreator.RespawnTrees();
                }

                if (buildings.TryGetValue(position, out var building))
                    building.Despawn();
                if (removeUnit && units.TryGetValue(position, out var unit))
                    unit.Despawn();
                if (level.tunnelEntrances.TryGetValue(position, out var tunnelEntrance)) {
                    if (tunnelEntrance.connected != null && tunnelEntrance.connected.connected == tunnelEntrance)
                        tunnelEntrance.connected.connected = null;

                    level.tunnelEntrances.Remove(position);
                    tunnelEntrance.Despawn();
                }
                if (level.TryGetPipeSection(position, out var pipeSection)) {
                    level.pipeSections.Remove(position);
                    pipeSection.Despawn();
                    UpdatePipeSectionViews();
                }

                foreach (var b in buildings.Values)
                    b.view.Position = b.position;
                foreach (var te in level.tunnelEntrances.Values)
                    te.view.Position = te.position;

                foreach (var player in level.players)
                    if (player.rootZone != null)
                        foreach (var zone in Zone.GetConnected(player.rootZone)) {
                            zone.tiles.Remove(position);
                            if (zone.tiles.Count == 0) {
                                foreach (var neighbor in zone.neighbors)
                                    neighbor.neighbors.Remove(zone);
                                if (player.rootZone == zone)
                                    player.rootZone = null;
                            }
                        }
            }

            void TryPlaceTile(Vector2Int position, TileType tileType, Player player) {
                if (tiles.TryGetValue(position, out var oldTileType)) {
                    if (oldTileType == tileType && ((TileType.Buildings & tileType) == 0 || buildings.TryGetValue(position, out var building) && building.Player == player))
                        return;
                    TryRemoveTile(position, false);
                    if (forestCreator && oldTileType == TileType.Forest)
                        forestCreator.RemoveTreesAt(position, false);
                }

                tiles.Add(position, tileType);

                if (tileMapCreator) {
                    if (tileMapCreator.tiles.ContainsKey(position))
                        tileMapCreator.tiles.Remove(position);
                    tileMapCreator.tiles.Add(position, tileType is TileType.Road or TileType.Forest or TileType.TunnelEntrance or TileType.PipeSection || ((tileType & TileType.Buildings) != 0) ? TileType.Plain : tileType);
                    tileMapCreator.RebuildPieces();

                    if (tileMapCreator.terrainMapper)
                        tileMapCreator.terrainMapper.ClearBushes();
                }

                if (roadCreator) {
                    if (tileType is TileType.Road or TileType.Bridge or TileType.BridgeSea or TileType.TunnelEntrance || (TileType.Buildings & tileType) != 0)
                        roadCreator.tiles.Add(position, tileType);
                    roadCreator.Rebuild();
                }

                if (forestCreator) {
                    if (tileType == TileType.Forest)
                        forestCreator.PlaceTreesAt(position, false);
                    forestCreator.RespawnTrees();
                }

                if (TileType.Buildings.HasFlag(tileType)) {
                    var building = new Building {
                        level = level,
                        position = position,
                        type = tileType,
                        Player = player,
                        ViewPrefab = BuildingView.GetPrefab(tileType),
                        lookDirection = player?.unitLookDirection ?? Vector2Int.up,
                        Cp = Rules.MaxCp(tileType)
                    };
                    if (building.type == TileType.MissileSilo)
                        building.missileSilo = new Building.MissileSiloStats { building = building };
                    else if (building.type == TileType.MissileStorage)
                        building.missileStorage = new Building.MissileStorageStats { building = building };
                    buildings.Add(position, building);
                    building.Spawn();
                }

                if (tileType == TileType.TunnelEntrance) {
                    var tunnel = new TunnelEntrance {
                        level = level,
                        position = position
                    };
                    level.tunnelEntrances.Add(position, tunnel);
                    tunnel.Spawn();
                }

                if (tileType == TileType.PipeSection) {
                    var pipeSection = new PipeSection {
                        level = level,
                        position = position
                    };
                    level.pipeSections.Add(position, pipeSection);
                    pipeSection.Spawn();
                    UpdatePipeSectionViews();
                }

                foreach (var b in buildings.Values)
                    b.view.Position = b.position;
                foreach (var tunnelEntrance in level.tunnelEntrances.Values)
                    tunnelEntrance.view.Position = tunnelEntrance.position;
            }

            gui.layerStack.Push(() => {
                GUILayout.Label("Level editor > Tiles " + ((TileType.Buildings & tileType) == 0 ? $"[{tileType}]" : $"[{(player?.ColorName.ToString() ?? "---")} {tileType}]"));
                GUILayout.Label("Height: " + height);
                GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
                GUILayout.Label("[F2] Cycle player");
                GUILayout.Label("[Tab] Cycle tile type");

                var camera = level.view.cameraRig.camera;
                if ((camera.TryPhysicsRaycast(out Vector3 point) || camera.TryRaycastPlane(out point)) &&
                    level.TryGetTile(point.ToVector2Int(), out var mouseTileType)) {
                    var text = buildings.TryGetValue(point.ToVector2Int(), out var building) ? building.ToString() : mouseTileType.ToString();
                    GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
                    GUILayout.Label(text);
                }
            });

            while (true) {
                yield return StateChange.none;

                editorState.DrawBridges();

                if (TryEnqueueModeSelectionCommand()) { }
                else if (Input.GetKeyDown(KeyCode.Tab))
                    game.EnqueueCommand(Command.CycleTileType, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                else if (Input.GetKeyDown(KeyCode.F2))
                    game.EnqueueCommand(Command.CyclePlayer, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                else if (Input.GetKeyDown(KeyCode.F3))
                    game.EnqueueCommand(Command.ToggleMode);
                else if (Input.GetMouseButton(Mouse.left) && camera.TryGetMousePosition(out Vector2Int mousePosition))
                    game.EnqueueCommand(Command.PlaceTile, (mousePosition, tileType, player));
                else if (Input.GetMouseButton(Mouse.right) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.RemoveTile, mousePosition);
                else if (Input.GetKeyDown(KeyCode.LeftAlt) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.PickTile, mousePosition);
                else if (Input.GetKeyDown(KeyCode.H))
                    showTiles = !showTiles;
                else if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    height += .1f;
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                    height -= .1f;
                else if (Input.GetKeyDown(KeyCode.Keypad0) && camera.TryGetMousePosition(out mousePosition))
                    tileMapCreator.heights[mousePosition] = height;
                else if (Input.GetKeyDown(KeyCode.Delete) && camera.TryGetMousePosition(out mousePosition))
                    tileMapCreator.heights.Remove(mousePosition);
                else if (Input.GetKeyDown(KeyCode.Print) || Input.GetMouseButtonDown(Mouse.extra0) || Input.GetMouseButtonDown(Mouse.extra1)) {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                        if (tileMapCreator) {
                            tileMapCreator.Save();
                            if (tileMapCreator.terrainMapper)
                                tileMapCreator.terrainMapper.SaveBushes();
                        }

                        if (roadCreator)
                            roadCreator.Save();
                        if (forestCreator)
                            forestCreator.Save();
                        if (propPlacement)
                            propPlacement.Save();

                        LevelEditorFileSystem.Save(level.name.ToString(), level);

                        gui.AddNotification("Saved");
                    }
                    else {
                        if (roadCreator)
                            roadCreator.Rebuild();
                        if (tileMapCreator && tileMapCreator.terrainMapper)
                            tileMapCreator.terrainMapper.PlaceBushes();
                        if (forestCreator) {
                            foreach (var position in forestCreator.trees.Keys.ToList())
                                forestCreator.RemoveTreesAt(position);
                            foreach (var position in tiles.Keys.Where(p => tiles[p] == TileType.Forest))
                                forestCreator.PlaceTreesAt(position);
                        }
                        if (heightMapBaker)
                            heightMapBaker.Bake();
                    }
                }
                else if (Input.GetKeyDown(KeyCode.M) && camera.TryGetMousePosition(out mousePosition)) {
                    if (level.mineFields.TryGetValue(mousePosition, out var mineField)) {
                        mineField.Despawn();
                        level.mineFields.Remove(mousePosition);
                    }
                    else {
                        mineField = new MineField {
                            level = level,
                            position = mousePosition,
                            Player = player
                        };
                        level.mineFields[mousePosition] = mineField;
                        mineField.Spawn();
                    }
                }
                else if (Input.GetKeyDown(KeyCode.B) && camera.TryGetMousePosition(out mousePosition) &&
                         (!level.tiles.TryGetValue(mousePosition, out var tileType) ||
                          tileType is TileType.Sea or TileType.River or TileType.Bridge or TileType.BridgeSea)) {
                    // no bridge at the position
                    if (!level.bridges2.TryGetValue(mousePosition, out var bridge)) {
                        // try find bridge nearby
                        var nearbyBridges = level.PositionsInRange(mousePosition, Vector2Int.one).Select(p => level.TryGetBridge2(p, out var b) ? b : null).Where(b => b != null).Distinct().ToList();
                        if (nearbyBridges.Count <= 1) {
                            if (nearbyBridges.Count == 1)
                                bridge = nearbyBridges[0];
                            var newPositions = bridge == null
                                ? new List<Vector2Int> { mousePosition }
                                : new List<Vector2Int>(bridge.Positions) { mousePosition };
                            var positionsAreValid = true;
                            if (newPositions.Count > 1) {
                                var direction = newPositions[1] - newPositions[0];
                                if (newPositions[^1] - newPositions[^ 2] != direction)
                                    positionsAreValid = false;
                            }
                            if (positionsAreValid) {
                                var mustSpawn = false;
                                if (bridge == null) {
                                    bridge = new Bridge2 { level = level };
                                    mustSpawn = true;
                                }
                                bridge.SetPositions(newPositions);
                                if (mustSpawn)
                                    bridge.Spawn();
                                level.bridges2.Add(mousePosition, bridge);
                                ToggleBridgeTile(mousePosition);
                            }
                            else
                                Debug.LogWarning("Bridge positions must be in a straight line.");
                        }
                    }
                    else {
                        // rotate bridge
                        if (Input.GetKey(KeyCode.LeftShift)) {
                            var forward = bridge.Forward ?? Vector2Int.up;
                            bridge.Forward = forward.Rotate(1);
                        }
                        // remove bridge tile at position
                        else {
                            if (bridge.Positions.Count == 1) {
                                level.bridges2.Remove(mousePosition);
                                ToggleBridgeTile(mousePosition);
                                bridge.Despawn();
                            }
                            else {
                                if (mousePosition == bridge.Positions[0] || mousePosition == bridge.Positions[^1]) {
                                    var newPositions = new List<Vector2Int>(bridge.Positions);
                                    newPositions.Remove(mousePosition);
                                    bridge.SetPositions(newPositions);
                                    level.bridges2.Remove(mousePosition);
                                    ToggleBridgeTile(mousePosition);
                                }
                                else
                                    Debug.LogWarning("Can only remove bridge tile at the ends of the bridge.");
                            }
                        }
                    }
                }

                void ToggleBridgeTile(Vector2Int position) {
                    if (!level.tiles.TryGetValue(position, out var tileType))
                        tileType = TileType.Sea;
                    level.tiles[position] = tileType switch {
                        TileType.River => TileType.Bridge,
                        TileType.Sea => TileType.BridgeSea,
                        TileType.Bridge => TileType.River,
                        TileType.BridgeSea => TileType.Sea,
                        _ => level.tiles[position]
                    };
                }

                while (game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (LevelEditorSessionState.SelectModeCommand, _):
                            yield return HandleModeSelectionCommand(command);
                            break;

                        case (Command.CycleTileType, int offset):
                            tileType = tileType.Cycle(tileTypes, offset);
                            break;

                        case (Command.CyclePlayer, int offset):
                            player = player.Cycle(level.players.Concat(new[] { (Player)null }), offset);
                            break;

                        case (Command.PlaceTile, Vector2Int position):
                            TryPlaceTile(position, tileType, null);
                            break;

                        case (Command.PlaceTile, (Vector2Int position, TileType tileType, null)):
                            TryPlaceTile(position, tileType, null);
                            break;

                        case (Command.PlaceTile, (Vector2Int position, TileType tileType, Player player)): {
                            TryPlaceTile(position, tileType, player);
                            break;
                        }

                        case (Command.RemoveTile, Vector2Int position):
                            TryRemoveTile(position, true);
                            break;

                        case (Command.PickTile, Vector2Int position):
                            if (tiles.TryGetValue(position, out var pickedTileType))
                                tileType = pickedTileType;
                            if (buildings.TryGetValue(position, out var building))
                                player = building.Player;
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }

                if (showTiles)
                    foreach (var (position, tileType) in level.tiles)
                        Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, level.TryGetBuilding(position, out var building) ? building.Player?.Color ?? unownedBuildingColor : GetColor(tileType));

                if (showHeights)
                    foreach (var (position, height) in tileMapCreator.heights) {
                        Draw.ingame.WirePlane(position.ToVector3(), Vector3.up, Vector2.one, Color.yellow);
                        Draw.ingame.Label3D(position.ToVector3(), Quaternion.LookRotation(Vector3.down), height.ToString(), 0.25f, LabelAlignment.Center, Color.yellow);
                    }

                if (showBridges) {
                    var index = 0;
                    foreach (var bridge in level.bridges2.Values.Distinct()) {
                        var minX = bridge.Positions.Select(v => v.x).Min();
                        var maxX = bridge.Positions.Select(v => v.x).Max();
                        var minY = bridge.Positions.Select(v => v.y).Min();
                        var maxY = bridge.Positions.Select(v => v.y).Max();
                        var center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
                        var size = new Vector2(maxX - minX + 1, maxY - minY + 1);
                        Draw.ingame.SolidPlane(center.ToVector3(), Vector3.up, size, Color.grey * new Color(1, 1, 1, .5f));
                        Draw.ingame.Label2D(center.ToVector3(), $"Bridge #{index}", Color.white);
                        index++;
                    }
                }
                {
                    if (camera.TryGetMousePosition(out Vector2Int mousePosition) &&
                        level.TryGetTunnelEntrance(mousePosition, out var tunnelEntrance) &&
                        tunnelEntrance.connected != null) {
                        Draw.ingame.Line(tunnelEntrance.position.Raycasted(), tunnelEntrance.connected.position.Raycasted(), Color.green);
                    }
                }
            }
        }
    }

    [Command]
    public static bool showBridges = false;

    public override void Exit() {
        if (tileMeshMaterial) {
            var texture = tileMeshMaterial.GetTexture("_TileMap");
            if (texture)
                Object.Destroy(texture);
        }

        if (tileMeshGameObject)
            Object.Destroy(tileMeshGameObject);

        var levelEditorState = stateMachine.TryFind<LevelEditorSessionState>();
        levelEditorState.gui.layerStack.Pop();
    }

    public LevelEditorTilesModeState(StateMachine stateMachine) : base(stateMachine) { }
}