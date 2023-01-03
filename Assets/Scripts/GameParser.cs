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
            return players.Values.SingleOrDefault(player => player.color.Name().StartsWith(colorNamePrefix));
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

            if (token is "players" or "tiles" or "units" or "game") {
                state = Enum.Parse<State>(token, true);
                return true;
            }

            Assert.IsTrue(state != State.None);

            switch (state) {
                
                case State.Game: {
                    switch (token) {
                        case "turn": {
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
                    if (token == "\\") {
                        scanPosition.x = startPosition.x;
                        scanPosition += nextLineDelta;
                        return true;
                    }
                    if (token == "startPosition") {
                        Assert.AreEqual(0,tiles.Count);
                        startPosition = stack.Pop<Vector2Int>();
                        scanPosition = startPosition;
                        return true;
                    }
                    return false;
                }

                case State.Players: {
                    switch (token) {
                        case "player": {
                            var colorName = stack.Pop<string>();
                            var found = Palette.TryGetColor(colorName, out var color);
                            Assert.IsTrue(found);
                            var player = new Player(main, color, playerTeam, playerCredits, playerCo, playerViewPrefab, playerType, playerDifficulty);
                            players.Add(color, player);
                            playerLoop.Add(player);
                            if (playerLocal)
                                main.localPlayer = player;
                            ResetPlayerValues();
                            return true;
                        }
                        case "team": {
                            playerTeam = stack.Pop<Team>();
                            return true;
                        }
                        case "credits": {
                            playerCredits = stack.Pop<int>();
                            return true;
                        }
                        case "co": {
                            playerCo = stack.Pop<string>().LoadAs<Co>();
                            return true;
                        }
                        case "prefab": {
                            playerViewPrefab = stack.Pop<string>().LoadAs<PlayerView>();
                            return true;
                        }
                        case "type": {
                            playerType = stack.Pop<PlayerType>();
                            return true;
                        }
                        case "difficulty": {
                            playerDifficulty = stack.Pop<AiDifficulty>();
                            return true;
                        }
                        case "local": {
                            Assert.IsNull(main.localPlayer);
                            playerLocal = true;
                            return true;
                        }
                        default:
                            return false;
                    }
                }

                case State.Units: {
                    switch (token) {
                        case "unit": {
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
                        case "position": {
                            unitPosition = stack.Pop<Vector2Int>();
                            return true;
                        }
                        case "moved": {
                            unitMoved = stack.Pop<bool>();
                            return true;
                        }
                        case "type": {
                            unitType = stack.Pop<UnitType>();
                            return true;
                        }
                        case "rotation": {
                            unitRotation = stack.Pop<Vector2Int>();
                            return true;
                        }
                        case "hp": {
                            unitHp = stack.Pop<int>();
                            return true;
                        }
                        case "fuel": {
                            unitFuel = stack.Pop<int>();
                            return true;
                        }
                        case "prefab": {
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