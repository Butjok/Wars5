using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class LevelEditor : MonoBehaviour {

    public const string mode = "mode";
    public const string player = "player";

    public Dictionary<Vector2Int, TileType> tiles = new();
    public Dictionary<Vector2Int, GameObject> tileViews = new();
    public Dictionary<Vector2Int, SerializedBuilding> buildings = new();
    public Dictionary<SerializedBuilding, BuildingView> buildingViews = new();
    public Dictionary<Vector2Int, SerializedUnit> units = new();
    public Dictionary<SerializedUnit, UnitView> unitViews = new();
    public TileType tileType = TileType.Plain;
    public TileType[] tileTypes2 = { TileType.Plain, TileType.Road, TileType.Sea, TileType.Mountain, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard };
    public UnitType unitType = UnitType.Infantry;
    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.TransportHelicopter, UnitType.AttackHelicopter, UnitType.FighterJet, UnitType.Bomber, UnitType.Recon, UnitType.LightTank, UnitType.Rockets, };
    public List<SerializedPlayer> players = new();
    public int playerId = -1;
    public Material tileMaterial;
    public TileTypeColorDictionary tileTypeColors = new();
    public UnitTypeUnitViewDictionary unitViewPrefabs = new();
    public Numerator numerator = new();
    public Color nullPlayerColor = Color.white;

    public Transform editModeRoot;
    public Transform playModeRoot;

    public LevelEditorHistory history = new();

    public TMP_Text uiText;
    public LevelEditorTextDisplay textDisplay;

    [Command]
    public bool LogHistory {
        get => history.log;
        set => history.log = value;
    }

    [Command]
    public void Break() { }

    private void Start() {

        textDisplay = new LevelEditorTextDisplay(uiText);

        Load("default");

        editModeRoot.gameObject.SetActive(true);
        playModeRoot.gameObject.SetActive(false);

        StartCoroutine(UnitsMode());

        // var red = new SerializedPlayer {
        //     color = Palette.red,
        //     team = Team.Alpha,
        //     coName = nameof(Co.Natalie),
        //     credits = 16000,
        //     type = PlayerType.Human
        // };
        // red.id = numerator[red];
        //
        // var blue = new SerializedPlayer {
        //     color = Palette.blue,
        //     team = Team.Bravo,
        //     coName = nameof(Co.Vladan),
        //     credits = 16000,
        //     type = PlayerType.Human
        // };
        // blue.id = numerator[blue];
        //
        // players.Add(red);
        // players.Add(blue);
        // playerId = red.id;
        //
        // for (var y = 0; y < 10; y++)
        // for (var x = 0; x < 10; x++)
        //     AddTile(new Vector2Int(x, y), TileType.Plain);
        //
        // // red hq
        // { 
        //     var position = new Vector2Int(0, 0);
        //     RemoveTile(position);
        //     AddTile(position, TileType.Hq);
        //     var hq = new SerializedBuilding {
        //         type = TileType.Hq,
        //         playerId = red.id,
        //         position = position.ToTuple()
        //     };
        //     hq.id = numerator[hq];
        //     AddBuilding(position, hq);
        // }
        // // blue hq
        // {
        //     var position = new Vector2Int(9, 9);
        //     RemoveTile(position);
        //     AddTile(position, TileType.Hq);
        //     var hq = new SerializedBuilding {
        //         type = TileType.Hq,
        //         playerId = blue.id,
        //         position = position.ToTuple()
        //     };
        //     hq.id = numerator[hq];
        //     AddBuilding(position, hq);
        // }
    }

    public IEnumerator TilesMode() {

        textDisplay.Clear();
        textDisplay.Set(mode, nameof(TilesMode));
        textDisplay.Set(nameof(tileType), tileType);
        textDisplay.Set(player, PlayerColor(playerId).Name());

        while (true) {
            yield return null;

            var modeToSwitchTo = HandleModeSelect(TilesMode);
            if (modeToSwitchTo != null) {
                yield return modeToSwitchTo;
                yield break;
            }

            if (HandlePlayerSelect()) { }
            else if (HandleHistory()) { }

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
                tileType = tileTypes2[(Array.IndexOf(tileTypes2, tileType) + offset).PositiveModulo(tileTypes2.Length)];
                textDisplay.Set(nameof(tileType), tileType);
            }

            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                var actionBuilder = history.CreateCompoundActionBuilder($"place {tileType} at {mousePosition}");

                var removeOldTile = tiles.TryGetValue(mousePosition, out var oldTileType) && oldTileType != tileType;
                var addNewTile = removeOldTile || oldTileType == default;

                if (removeOldTile)
                    actionBuilder
                        .EnqueueCommitAction(() => RemoveTile(mousePosition))
                        .PushRevertAction(() => AddTile(mousePosition, oldTileType));

                if (addNewTile) {
                    var savedTileType = tileType;
                    actionBuilder
                        .EnqueueCommitAction(() => AddTile(mousePosition, savedTileType))
                        .PushRevertAction(() => RemoveTile(mousePosition));
                }

                SerializedBuilding building = null;
                if (TileType.Buildings.HasFlag(tileType))
                    building = new SerializedBuilding {
                        type = tileType,
                        playerId = playerId,
                        position = mousePosition.ToTuple()
                    };

                var removeOldBuilding = buildings.TryGetValue(mousePosition, out var oldBuilding) && (building == null || !oldBuilding.HasSameData(building));
                var addNewBuilding = building != null && (removeOldBuilding || oldBuilding == null);

                if (removeOldBuilding)
                    actionBuilder
                        .EnqueueCommitAction(() => RemoveBuilding(mousePosition))
                        .PushRevertAction(() => AddBuilding(mousePosition, oldBuilding));

                if (addNewBuilding) {
                    // assign new id
                    building.id = numerator[building];
                    actionBuilder
                        .EnqueueCommitAction(() => AddBuilding(mousePosition, building))
                        .PushRevertAction(() => RemoveBuilding(mousePosition));
                }

                actionBuilder.Execute();
            }

            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out mousePosition)) {

                var actionBuilder = history.CreateCompoundActionBuilder($"remove tile at {mousePosition}");

                if (tiles.TryGetValue(mousePosition, out var oldTileType))
                    actionBuilder
                        .EnqueueCommitAction(() => RemoveTile(mousePosition))
                        .PushRevertAction(() => AddTile(mousePosition, oldTileType));

                if (buildings.TryGetValue(mousePosition, out var oldBuilding))
                    actionBuilder
                        .EnqueueCommitAction(() => RemoveBuilding(mousePosition))
                        .PushRevertAction(() => AddBuilding(mousePosition, oldBuilding));

                actionBuilder.Execute();
            }

            // picker
            else if (Input.GetKeyDown(KeyCode.L) && Mouse.TryGetPosition(out mousePosition) &&
                     tiles.TryGetValue(mousePosition, out var tileType)) {

                this.tileType = tileType;
                textDisplay.Set(nameof(tileType), tileType);

                if (buildings.TryGetValue(mousePosition, out var building)) {
                    playerId = building.playerId;
                    textDisplay.Set(player, PlayerColor(playerId).Name());
                }
            }
        }
    }

    public void AddTile(Vector2Int position, TileType tileType) {

        Assert.IsFalse(tiles.ContainsKey(position));

        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.SetLayerRecursively(Layers.Terrain);

        go.transform.position = position.ToVector3Int();
        go.transform.rotation = Quaternion.Euler(90, 0, 0);
        var propertyBlock = new MaterialPropertyBlock();

        if (!tileTypeColors.TryGetValue(tileType, out var color))
            color = nullPlayerColor;

        propertyBlock.SetColor("_Tint", color);
        propertyBlock.SetFloat("_Glossiness", color.a);

        var meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = tileMaterial;
        meshRenderer.SetPropertyBlock(propertyBlock);

        tiles.Add(position, tileType);
        tileViews.Add(position, go);
    }

    public void RemoveTile(Vector2Int position) {

        Assert.IsTrue(tiles.ContainsKey(position));

        var found = tileViews.TryGetValue(position, out var view);
        Assert.IsTrue(found);

        Destroy(view.gameObject);
        tileViews.Remove(position);
        tiles.Remove(position);
    }

    public void AddBuilding(Vector2Int position, SerializedBuilding building) {

        Assert.IsFalse(buildings.ContainsKey(position));

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var view = go.AddComponent<BuildingView>();
        view.renderers = new[] { go.GetComponent<MeshRenderer>() };

        view.Position = position;
        view.PlayerColor = PlayerColor(building.playerId);

        buildings.Add(position, building);
        buildingViews.Add(building, view);
    }

    public void RemoveBuilding(Vector2Int position) {

        var found = buildings.TryGetValue(position, out var building);
        Assert.IsTrue(found);

        var foundView = buildingViews.TryGetValue(building, out var view);
        Assert.IsTrue(foundView);

        Destroy(view.gameObject);
        buildingViews.Remove(building);
        buildings.Remove(position);
    }

    public void AddUnit(Vector2Int position, SerializedUnit unit) {

        Assert.IsFalse(units.ContainsKey(position));

        var found = unitViewPrefabs.TryGetValue(unitType, out var viewPrefab);
        Assert.IsTrue(found);
        Assert.IsTrue(viewPrefab);
        var view = Instantiate(viewPrefab, position.ToVector3Int(), Quaternion.identity, editModeRoot);

        view.Position = position;
        view.PlayerColor = PlayerColor(playerId);

        units.Add(position, unit);
        unitViews.Add(unit, view);
    }

    public void RemoveUnit(Vector2Int position) {

        var found = units.TryGetValue(position, out var unit);
        Assert.IsTrue(found);

        var foundView = unitViews.TryGetValue(unit, out var view);
        Assert.IsTrue(foundView);

        Destroy(view.gameObject);
        unitViews.Remove(unit);
        units.Remove(position);
    }

    public IEnumerator UnitsMode() {

        textDisplay.Clear();
        textDisplay.Set(mode, nameof(UnitsMode));
        textDisplay.Set(nameof(unitType), unitType);
        textDisplay.Set(player, PlayerColor(playerId).Name());

        while (true) {
            yield return null;

            var modeToSwitchTo = HandleModeSelect(UnitsMode);
            if (modeToSwitchTo != null) {
                yield return modeToSwitchTo;
                yield break;
            }

            if (HandlePlayerSelect()) { }
            else if (HandleHistory()) { }

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
                unitType = unitTypes[(Array.IndexOf(unitTypes, unitType) + offset).PositiveModulo(unitTypes.Length)];
                textDisplay.Set(nameof(unitType), unitType);
            }

            else if (playerId != -1 && Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                var actionBuilder = history.CreateCompoundActionBuilder($"place {unitType} at {mousePosition}");

                var unit = new SerializedUnit {
                    type = unitType,
                    playerId = playerId,
                    position = mousePosition.ToTuple()
                };

                var removeOldUnit = units.TryGetValue(mousePosition, out var oldUnit) && !oldUnit.HasSameData(unit);
                var addNewUnit = oldUnit == null || removeOldUnit;

                if (removeOldUnit)
                    actionBuilder
                        .EnqueueCommitAction(() => RemoveUnit(mousePosition))
                        .PushRevertAction(() => AddUnit(mousePosition, oldUnit));

                if (addNewUnit) {
                    // assign new id
                    unit.id = numerator[unit];
                    actionBuilder
                        .EnqueueCommitAction(() => AddUnit(mousePosition, unit))
                        .PushRevertAction(() => RemoveUnit(mousePosition));
                }

                actionBuilder.Execute();
            }

            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out mousePosition)) {
                if (units.TryGetValue(mousePosition, out var oldUnit))
                    history.Execute(
                        () => RemoveUnit(mousePosition),
                        () => AddUnit(mousePosition, oldUnit),
                        $"remove unit at {mousePosition}");
            }

            else if (Input.GetKeyDown(KeyCode.L) && Mouse.TryGetPosition(out mousePosition) &&
                     units.TryGetValue(mousePosition, out var unit)) {

                unitType = unit.type;
                playerId = unit.playerId;

                textDisplay.Set(nameof(unitType), unitType);
                textDisplay.Set(player, PlayerColor(playerId).Name());
            }
        }
    }

    public IEnumerator PlayMode(Func<IEnumerator> modeAfterPlayEnd) {

        Assert.AreNotEqual(0, player.Length);
        var positions = tiles.Keys.Concat(buildings.Keys).Concat(units.Keys).ToArray();
        Assert.AreNotEqual(0, positions.Length);
        Assert.IsTrue(editModeRoot);

        textDisplay.Clear();
        textDisplay.Set(mode, nameof(PlayMode));

        var go = new GameObject(nameof(Level));
        var game = go.AddComponent<Level>();

        var playerLookup = new Dictionary<int, Player>();
        foreach (var player in players)
            playerLookup.Add(player.id, new Player(game, player.color, player.team, player.credits, type: player.type));
        game.localPlayer = playerLookup[players[0].id];

        var min = new Vector2Int(positions.Min(p => p.x), positions.Min(p => p.y));
        var max = new Vector2Int(positions.Max(p => p.x), positions.Max(p => p.y));

        game.tiles = new Map2D<TileType>(min, max);
        game.units = new Map2D<Unit>(min, max);
        game.buildings = new Map2D<Building>(min, max);

        foreach (var position in tiles.Keys)
            game.tiles[position] = tiles[position];

        foreach (var position in units.Keys) {
            var unit = units[position];
            var foundPlayer = playerLookup.TryGetValue(unit.playerId, out var player);
            Assert.IsTrue(foundPlayer);
            var foundViewPrefab = unitViewPrefabs.TryGetValue(unit.type, out var prefab);
            Assert.IsTrue(foundViewPrefab);
            Assert.IsTrue(prefab);
            new Unit(player, type: unit.type, position: unit.position?.ToVector2Int(), viewPrefab: prefab);
        }

        foreach (var position in buildings.Keys) {
            var building = buildings[position];
            var player = playerLookup.TryGetValue(building.playerId, out var p) ? p : null;
            new Building(game, position, building.type, player);
        }

        game.levelLogic = new LevelLogic();
        game.StartGame();

        editModeRoot.gameObject.SetActive(false);
        playModeRoot.gameObject.SetActive(true);

        while (true) {
            yield return null;

            if (Input.GetKeyDown(KeyCode.F5)) {

                editModeRoot.gameObject.SetActive(true);
                playModeRoot.gameObject.SetActive((false));
                Destroy(go);

                yield return modeAfterPlayEnd();
                yield break;
            }
        }
    }

    public IEnumerator HandleModeSelect(Func<IEnumerator> modeAfterPlayEnd) {
        if (Input.GetKeyDown(KeyCode.T))
            return TilesMode();
        if (Input.GetKeyDown(KeyCode.U))
            return UnitsMode();
        if (Input.GetKeyDown(KeyCode.F5))
            return PlayMode(modeAfterPlayEnd);
        return null;
    }

    public bool HandlePlayerSelect() {

        if (Input.GetKeyDown(KeyCode.P)) {

            if (players.Count > 0) {
                //var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
                var offset = 1;
                var index = players.FindIndex(p => p.id == playerId);
                if (index == players.Count - 1)
                    playerId = -1;
                else
                    playerId = players[(index + offset).PositiveModulo(players.Count)].id;
            }
            else
                playerId = -1;

            textDisplay.Set(player, PlayerColor(playerId).Name());

            return true;
        }
        return false;
    }
    public bool HandleHistory() {
        if (Input.GetKeyDown(KeyCode.PageDown)) {
            history.TryUndo();
            return true;
        }
        if (Input.GetKeyDown(KeyCode.PageUp)) {
            history.TryRedo();
            return true;
        }
        return false;
    }

    public Color32 PlayerColor(int playerId) {
        return players.SingleOrDefault(p => p.id == playerId)?.color ?? nullPlayerColor;
    }

    [Command]
    public string SavePath(string name) => Path.Combine(Application.persistentDataPath, name);

    [Command]
    public void Save(string name) {

        var level = new LevelConfiguration {
            players = players.ToArray(),
            tiles = tiles.Select(pair => (pair.Key.ToTuple(), pair.Value)).ToArray(),
            buildings = buildings.Values.ToArray(),
            units = units.Values.ToArray()
        };
        var json = level.ToJson();

        File.WriteAllText(SavePath(name), json);
    }

    [Command]
    public void Load(string name) {

        var path = SavePath(name);
        Assert.IsTrue(File.Exists(path), path);
        var level = File.ReadAllText(path).FromJson<LevelConfiguration>();

        players.Clear();
        foreach (var position in tiles.Keys.ToArray())
            RemoveTile(position);
        foreach (var position in buildings.Keys.ToArray())
            RemoveBuilding(position);
        foreach (var position in units.Keys.ToArray())
            RemoveUnit(position);

        players.AddRange(level.players);
        playerId = players.Count == 0 ? -1 : players[0].id;

        tiles.Clear();
        foreach (var tile in level.tiles)
            AddTile(tile.position.ToVector2Int(), tile.tileType);

        buildings.Clear();
        foreach (var building in level.buildings)
            AddBuilding(building.position.ToVector2Int(), building);

        units.Clear();
        foreach (var unit in level.units)
            if (unit.position?.ToVector2Int() is { } position)
                AddUnit(position, unit);
    }
}

[Serializable]
public class TileTypeBuildingViewDictionary : SerializableDictionary<TileType, BuildingView> { }