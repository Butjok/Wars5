using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameParser {

    public static readonly Dictionary<string, TileType> tileTypes = new() {
        ["."] = TileType.Plain,
        ["_"] = TileType.Road,
        ["~"] = TileType.Sea,
        ["^"] = TileType.Mountain,
    };

    public static readonly Dictionary<string, TileType> buildingTypes = new() {
        ["C"] = TileType.City,
        ["H"] = TileType.Hq,
        ["F"] = TileType.Factory,
        ["A"] = TileType.Airport,
        ["S"] = TileType.Shipyard
    };

    // append building types to tile types
    static GameParser() {
        foreach (var (key, value) in buildingTypes)
            tileTypes.Add(key, value);
    }

    private enum State { None, Players, Tiles, Units, Game }

    public static void Parse(Main main, string input) {
        Parse(main, input, Vector2Int.zero, Vector2Int.right, Vector2Int.down);
    }

    public static void Parse(Main main, string input,
        Vector2Int startPosition, Vector2Int nextTileDelta, Vector2Int nextLineDelta) {

        var state = State.None;

        var players = new Dictionary<Color32, Player>();
        var playerLoop = new List<Player>();

        Player FindPlayer(string colorNamePrefix) {
            return players.Values.SingleOrDefault(player => player.color.GetName().ToString().StartsWith(colorNamePrefix));
        }

        Team playerTeam;
        Co playerCo;
        PlayerType playerType;
        AiDifficulty playerDifficulty;
        int playerCredits;
        PlayerView playerViewPrefab;
        bool playerLocal;

        void ResetPlayerValues() {
            playerTeam = Team.None;
            playerCo = Co.Natalie;
            playerType = PlayerType.Human;
            playerDifficulty = AiDifficulty.Normal;
            playerCredits = 0;
            playerViewPrefab = PlayerView.DefaultPrefab;
            playerLocal = false;
        }
        ResetPlayerValues();

        var scanPosition = startPosition;
        var tiles = new Dictionary<Vector2Int, TileType>();

        Vector2Int? unitPosition;
        Vector2Int unitRotation;
        UnitType unitType;
        bool unitMoved;
        int unitHp ;
        int unitFuel;
        UnitView unitViewPrefab;

        void ResetUnitValues() {
            unitPosition = null;
            unitRotation = Vector2Int.up;
            unitType = UnitType.Infantry;
            unitMoved = false;
            unitHp = 10;
            unitFuel = 99;
            unitViewPrefab = UnitView.DefaultPrefab;
        }
        ResetUnitValues();

        var delayedActions = new List<Action>();

        PostfixInterpreter.Execute(input, (token, stack) => {

            if (token == "set-state") {
                state = Enum.Parse<State>(stack.Pop<string>());
                return true;
            }

            switch (state) {
                
                case State.Game: {
                    switch (token) {
                        case "set-turn": {
                            main.turn = stack.Pop<int>();
                            return true;
                        }
                        default:
                            return false;
                    }
                }
                
                case State.Tiles: {
                    if (tileTypes.TryGetValue(token, out var tileType)) {
                        tiles.Add(scanPosition, tileType);
                        if (buildingTypes.TryGetValue(token, out var buildingType)) {
                            var player = FindPlayer(stack.Pop<string>());
                            var position = scanPosition;
                            delayedActions.Add(() => new Building(main, position, buildingType, player));
                        }
                        scanPosition += nextTileDelta;
                        return true;
                    }
                    if (token == "nl") {
                        scanPosition.x = startPosition.x;
                        scanPosition += nextLineDelta;
                        return true;
                    }
                    if (token == "set-start-position") {
                        Assert.AreEqual(0,tiles.Count);
                        startPosition = stack.Pop<Vector2Int>();
                        scanPosition = startPosition;
                        return true;
                    }
                    return false;
                }

                case State.Players: {
                    switch (token) {
                        case "add-player": {
                            var colorName = stack.Pop<ColorName>();
                            var color = colorName.ToColor32();
                            var player = new Player(main, color, playerTeam, playerCredits, playerCo, playerViewPrefab, playerType, playerDifficulty);
                            players.Add(color, player);
                            playerLoop.Add(player);
                            if (playerLocal) {
                                Assert.IsNull(main.localPlayer);
                                main.localPlayer = player;
                            }
                            ResetPlayerValues();
                            return true;
                        }
                        case "set-team": {
                            playerTeam = stack.Pop<Team>();
                            return true;
                        }
                        case "set-credits": {
                            playerCredits = stack.Pop<int>();
                            return true;
                        }
                        case "set-co": {
                            playerCo = stack.Pop<string>().LoadAs<Co>();
                            return true;
                        }
                        case "set-prefab": {
                            playerViewPrefab = stack.Pop<string>().LoadAs<PlayerView>();
                            return true;
                        }
                        case "set-type": {
                            playerType = stack.Pop<PlayerType>();
                            return true;
                        }
                        case "set-difficulty": {
                            playerDifficulty = stack.Pop<AiDifficulty>();
                            return true;
                        }
                        case "set-local": {
                            playerLocal = stack.Pop<bool>();
                            return true;
                        }
                        default:
                            return false;
                    }
                }

                case State.Units: {
                    switch (token) {
                        case "add-unit": {
                            var player = FindPlayer(stack.Pop<string>());
                            Assert.IsNotNull(player);
                            var moved = unitMoved;
                            var type = unitType;
                            var position = unitPosition;
                            var rotation = unitRotation;
                            var hp = unitHp;
                            var fuel = unitFuel;
                            var viewPrefab = unitViewPrefab;
                            delayedActions.Add(() => new Unit(player, moved, type, position, rotation, hp, fuel, viewPrefab));
                            ResetUnitValues();
                            return true;
                        }
                        case "set-position": {
                            unitPosition = stack.Pop<Vector2Int>();
                            return true;
                        }
                        case "set-moved": {
                            unitMoved = stack.Pop<bool>();
                            return true;
                        }
                        case "set-type": {
                            unitType = stack.Pop<UnitType>();
                            return true;
                        }
                        case "set-look-direction": {
                            unitRotation = stack.Pop<Vector2Int>();
                            return true;
                        }
                        case "set-hp": {
                            unitHp = stack.Pop<int>();
                            return true;
                        }
                        case "set-fuel": {
                            unitFuel = stack.Pop<int>();
                            return true;
                        }
                        case "set-prefab": {
                            unitViewPrefab = stack.Pop<string>().LoadAs<UnitView>();
                            return true;
                        }
                        default:
                            return false;
                    }
                }
            }

            return false;
        });

        Assert.IsNotNull(main.localPlayer);

        if (tiles.Count > 0) {
            
            var positions = tiles.Keys;
            var min = new Vector2Int(positions.Min(i => i.x), positions.Min(i => i.y));
            var max = new Vector2Int(positions.Max(i => i.x), positions.Max(i => i.y));

            main.tiles = new Map2D<TileType>(min, max);
            main.buildings = new Map2D<Building>(min, max);
            main.units = new Map2D<Unit>(min, max);
            
            foreach (var (position, tileType) in tiles)
                main.tiles[position] = tileType;
        }

        foreach (var action in delayedActions)
            action();
    }
}