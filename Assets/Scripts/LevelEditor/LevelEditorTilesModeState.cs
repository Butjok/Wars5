using System;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Vector2Int = UnityEngine.Vector2Int;

public class LevelEditorTilesModeState : StateMachineState {

    public enum Command {
        CycleTileType, PlaceTile, RemoveTile, PickTile, CyclePlayer,
        ToggleMode
    }
    public enum Mode { Add, Remove }

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

    public TileType[] tileTypes = { TileType.Plain, TileType.Road, TileType.Forest, TileType.Mountain, TileType.River, TileType.Sea, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard, TileType.MissileSilo };
    public TileType tileType = TileType.Plain;
    public Player player;
    public Vector2Int lookDirection = Vector2Int.up;
    public Mode mode;
    public GameObject tileMeshGameObject;
    public Material tileMeshMaterial;

    public LevelEditorTilesModeState(StateMachine stateMachine) : base(stateMachine) {
        var terrainCreator = Object.FindObjectOfType<TerrainCreator>();
        if (terrainCreator) {
            tileMeshGameObject = new GameObject("LevelEditorTileMesh");
            var meshFilter = tileMeshGameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = terrainCreator.mesh;
            var meshRenderer = tileMeshGameObject.AddComponent<MeshRenderer>();
            tileMeshMaterial = "Unlit_TileMap".LoadAs<Material>();
            meshRenderer.sharedMaterial = tileMeshMaterial;
        }
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

            RebuildTilemapMesh();

            void TryRemoveTile(Vector2Int position, bool removeUnit) {
                if (!tiles.ContainsKey(position))
                    return;
                tiles.Remove(position);
                if (buildings.TryGetValue(position, out var building))
                    building.Dispose();
                if (removeUnit && units.TryGetValue(position, out var unit))
                    unit.Dispose();
                RebuildTilemapMesh();
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
                if (tiles.ContainsKey(position))
                    TryRemoveTile(position, false);
                tiles.Add(position, tileType);
                if (TileType.Buildings.HasFlag(tileType))
                    new Building(level, position, tileType, player, viewPrefab: BuildingView.GetPrefab(tileType), lookDirection: player?.unitLookDirection ?? Vector2Int.up);
                RebuildTilemapMesh();
            }

            gui
                .Push()
                .Add("TileType", () => tileType)
                .Add("Player", () => player)
                .Add("LookDirection", () => lookDirection)
                // .Add("Mode", () => mode)
                ;

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
                else if (Input.GetKeyDown(KeyCode.F5))
                    game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.Play);
                else if (Input.GetKeyDown(KeyCode.LeftAlt) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.PickTile, mousePosition);

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

                //foreach (var (position, tileType) in level.tiles)
                //  Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, GetColor(tileType, level.TryGetBuilding(position, out var building) ? building : null));
            }
        }
    }

    public override void Exit() {

        if (tileMeshMaterial) {
            var texture = tileMeshMaterial.GetTexture("_TileMap");
            if (texture)
                Object.Destroy(texture);
        }
        if (tileMeshGameObject)
            Object.Destroy(tileMeshGameObject);

        var levelEditorState = stateMachine.TryFind<LevelEditorSessionState>();
        levelEditorState.gui.Pop();
    }
}