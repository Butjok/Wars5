using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

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

    public Transform unitsRoot;

    private void Start() {

        var red = new SerializedPlayer {
            color = Palette.red,
            team = Team.Alpha,
            coName = nameof(Co.Natalie),
            credits = 16000,
            type = PlayerType.Human
        };
        red.id = numerator[red];

        var blue = new SerializedPlayer {
            color = Palette.blue,
            team = Team.Bravo,
            coName = nameof(Co.Vladan),
            credits = 16000,
            type = PlayerType.Human
        };
        blue.id = numerator[blue];

        players.Add(red);
        players.Add(blue);
        playerId = red.id;

        StartCoroutine(TilesMode());
    }

    public Dictionary<string, object> text = new();
    public TMP_Text uiText;
    public void UpdateText() {
        uiText.text = string.Join("\n", text
            .OrderBy(pair => pair.Key)
            .Select(pair => $"{pair.Key}: {pair.Value}"));
    }
    public void ClearText() {
        text.Clear();
        UpdateText();
    }
    public void SetText(string key, object value) {
        text[key] = value;
        UpdateText();
    }

    public IEnumerator PlayersMode() {

        ClearText();
        SetText(mode, nameof(PlayersMode));

        while (true) {
            yield return null;

            var modeToSwitchTo = HandleModeSelect(PlayersMode);
            if (modeToSwitchTo != null) {
                yield return modeToSwitchTo;
                yield break;
            }
        }
    }

    public IEnumerator TilesMode() {

        ClearText();
        SetText(mode, nameof(TilesMode));
        SetText(nameof(tileType), tileType);
        SetText(player, PlayerColor.Name());

        while (true) {
            yield return null;

            var modeToSwitchTo = HandleModeSelect(TilesMode);
            if (modeToSwitchTo != null) {
                yield return modeToSwitchTo;
                yield break;
            }

            if (HandlePlayerSelect()) { }

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
                tileType = tileTypes2[(Array.IndexOf(tileTypes2, tileType) + offset).PositiveModulo(tileTypes2.Length)];
                SetText(nameof(tileType), tileType);
            }

            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int mousePosition)) {
                {
                    RemoveTile(mousePosition);

                    var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.SetLayerRecursively(Layers.Terrain);

                    go.transform.position = mousePosition.ToVector3Int();
                    go.transform.rotation = Quaternion.Euler(90, 0, 0);
                    var propertyBlock = new MaterialPropertyBlock();

                    Color color = default;
                    if (TileType.PlayerOwned.HasFlag(tileType))
                        propertyBlock.SetColor("_Tint", PlayerColor);
                    else {
                        var found = tileTypeColors.TryGetValue(tileType, out color);
                        Assert.IsTrue(found);
                        propertyBlock.SetColor("_Tint", color);
                    }
                    propertyBlock.SetFloat("_Glossiness", color.a);

                    var meshRenderer = go.GetComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = tileMaterial;
                    meshRenderer.SetPropertyBlock(propertyBlock);

                    tiles.Add(mousePosition, tileType);
                    tileViews.Add(mousePosition, go);
                }

                if (TileType.PlayerOwned.HasFlag(tileType)) {

                    var building = new SerializedBuilding {
                        type = tileType,
                        playerId = playerId,
                        position = mousePosition
                    };
                    building.id = numerator[building];

                    // var found = buildingViewPrefabs.TryGetValue(tileType, out var viewPrefab);
                    // Assert.IsTrue(found);
                    // Assert.IsTrue(viewPrefab);
                    // var view = Instantiate(viewPrefab);

                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var view = go.AddComponent<BuildingView>();
                    view.renderers = new[] { go.GetComponent<MeshRenderer>() };

                    view.Position = mousePosition;
                    view.PlayerColor = PlayerColor;

                    buildings.Add(mousePosition, building);
                    buildingViews.Add(building, view);
                }
            }

            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out mousePosition))
                RemoveTile(mousePosition);
        }
    }

    public IEnumerator UnitsMode() {

        ClearText();
        SetText(mode, nameof(UnitsMode));
        SetText(nameof(unitType), unitType);
        SetText(player, PlayerColor.Name());

        while (true) {
            yield return null;

            var modeToSwitchTo = HandleModeSelect(UnitsMode);
            if (modeToSwitchTo != null) {
                yield return modeToSwitchTo;
                yield break;
            }

            if (HandlePlayerSelect()) { }

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
                unitType = unitTypes[(Array.IndexOf(unitTypes, unitType) + offset).PositiveModulo(unitTypes.Length)];
                SetText(nameof(unitType), unitType);
            }

            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                RemoveUnit(mousePosition);

                var unit = new SerializedUnit {
                    type = unitType,
                    playerId = playerId,
                    position = mousePosition
                };
                unit.id = numerator[unit];

                var found = unitViewPrefabs.TryGetValue(unitType, out var viewPrefab);
                Assert.IsTrue(found);
                Assert.IsTrue(viewPrefab);
                var view = Instantiate(viewPrefab, unitsRoot);

                unit.viewPrefabName = viewPrefab.name;

                view.Position = mousePosition;
                view.PlayerColor = PlayerColor;

                units.Add(mousePosition, unit);
                unitViews.Add(unit, view);
            }

            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out mousePosition))
                RemoveUnit(mousePosition);
        }
    }

    public IEnumerator PlayMode(Func<IEnumerator> modeAfterPlayEnd) {

        Assert.AreNotEqual(0, player.Length);
        var positions = tiles.Keys.Concat(units.Keys).ToArray();
        Assert.AreNotEqual(0, positions.Length);
        Assert.IsTrue(unitsRoot);

        ClearText();
        SetText(mode, nameof(PlayMode));

        var go = new GameObject(nameof(Game));
        var game = go.AddComponent<Game>();

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
            var prefab = Resources.Load<UnitView>(unit.viewPrefabName);
            Assert.IsTrue(prefab);
            new Unit(player, type: unit.type, position: unit.position, viewPrefab: prefab);
        }

        game.levelLogic = new LevelLogic();
        game.StartCoroutine(SelectionState.New(game, true));

        unitsRoot.gameObject.SetActive(false);

        while (true) {
            yield return null;

            if (Input.GetKeyDown(KeyCode.F3)) {

                unitsRoot.gameObject.SetActive(true);
                Destroy(go);

                yield return modeAfterPlayEnd();
                yield break;
            }
        }
    }

    public void RemoveTile(Vector2Int position) {
        if (tiles.TryGetValue(position, out _)) {
            var found = tileViews.TryGetValue(position, out var view);
            Assert.IsTrue(found);
            Destroy(view.gameObject);
            tileViews.Remove(position);
            tiles.Remove(position);
        }
        if (buildings.TryGetValue(position, out var building)) {
            var found = buildingViews.TryGetValue(building, out var view);
            Assert.IsTrue(found);
            Destroy(view.gameObject);
            buildingViews.Remove(building);
            buildings.Remove(position);
        }
    }

    public void RemoveUnit(Vector2Int position) {
        if (units.TryGetValue(position, out var unit)) {
            var found = unitViews.TryGetValue(unit, out var view);
            Assert.IsTrue(found);
            Destroy(view.gameObject);
            unitViews.Remove(unit);
            units.Remove(position);
        }
    }

    public IEnumerator HandleModeSelect(Func<IEnumerator> modeAfterPlayEnd) {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            return PlayersMode();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            return TilesMode();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            return UnitsMode();
        if (Input.GetKeyDown(KeyCode.F3))
            return PlayMode(modeAfterPlayEnd);
        return null;
    }

    public bool HandlePlayerSelect() {
        if (Input.GetKeyDown(KeyCode.P) && players.Count > 0) {
            var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
            var index = players.FindIndex(p => p.id == playerId);
            playerId = players[(index + offset).PositiveModulo(players.Count)].id;
            SetText(player, PlayerColor.Name());
            return true;
        }
        return false;
    }

    public Color32 PlayerColor => players.SingleOrDefault(p => p.id == playerId)?.color ?? nullPlayerColor;
}

[Serializable]
public class TileTypeBuildingViewDictionary : SerializableDictionary<TileType, BuildingView> { }