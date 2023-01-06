using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameLoader {

    public static readonly Dictionary<string, TileType> parseMnemonic = new() {
        ["."] = TileType.Plain,
        ["_"] = TileType.Road,
        ["~"] = TileType.Sea,
        ["^"] = TileType.Mountain,
        ["c"] = TileType.City,
        ["h"] = TileType.Hq,
        ["f"] = TileType.Factory,
        ["a"] = TileType.Airport,
        ["s"] = TileType.Shipyard
    };

    public static void Load(Main main, string input) {
        Load(main, input, Vector2Int.zero, Vector2Int.right, Vector2Int.down);
    }

    public static void Load(Main main, string input,
        Vector2Int startPosition, Vector2Int nextTileDelta, Vector2Int nextLineOffset) {

        var playerLookup = new Dictionary<string, Player>();

        Team playerTeam;
        Co playerCo;
        PlayerType playerType;
        AiDifficulty playerDifficulty;
        int playerCredits;
        PlayerView playerViewPrefab;
        bool playerLocal;
        Color playerColor;
        string playerLookupId;

        void ResetPlayerValues() {
            playerTeam = Team.None;
            playerCo = Co.Natalie;
            playerType = PlayerType.Human;
            playerDifficulty = AiDifficulty.Normal;
            playerCredits = 0;
            playerViewPrefab = PlayerView.DefaultPrefab;
            playerLocal = false;
            playerColor = Palette.none;
            playerLookupId = null;
        }
        ResetPlayerValues();

        var scanPosition = startPosition;
        var hasPlayer = true;

        Vector2Int? unitPosition;
        Vector2Int unitRotation;
        UnitType unitType;
        bool unitMoved;
        int unitHp;
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

        main.Clear();

        var stack = new Stack();
        foreach (var token in input.Tokenize()) {
            switch (token) {

                case "game.set-turn": {
                    main.turn = stack.Pop<int>();
                    break;
                }
                case "game.load-scene": {
                    var name = stack.Pop<string>();
                    break;
                }

                case "player.create": {

                    Assert.IsTrue(playerLookupId == null || !playerLookup.ContainsKey(playerLookupId), playerLookupId);

                    var player = new Player(main, playerColor, playerTeam, playerCredits, playerCo, playerViewPrefab, playerType, playerDifficulty);

                    if (playerLookupId != null)
                        playerLookup.Add(playerLookupId, player);

                    if (playerLocal) {
                        Assert.IsNull(main.localPlayer);
                        main.localPlayer = player;
                    }

                    stack.Push(player);
                    ResetPlayerValues();
                    break;
                }

                case "player.set-lookup-id": {
                    playerLookupId = stack.Pop<string>();
                    break;
                }
                case "player.set-color": {
                    var b = stack.Pop<dynamic>();
                    var g = stack.Pop<dynamic>();
                    var r = stack.Pop<dynamic>();
                    playerColor = new Color(r, g, b);
                    break;
                }
                case "player.set-team": {
                    playerTeam = stack.Pop<Team>();
                    break;
                }
                case "player.set-credits": {
                    playerCredits = stack.Pop<int>();
                    break;
                }
                case "player.set-co": {
                    playerCo = stack.Pop<string>().LoadAs<Co>();
                    break;
                }
                case "player.set-prefab": {
                    playerViewPrefab = stack.Pop<string>().LoadAs<PlayerView>();
                    break;
                }
                case "player.set-ai": {
                    playerType = PlayerType.Ai;
                    playerDifficulty = stack.Pop<AiDifficulty>();
                    break;
                }
                case "player.mark-as-local": {
                    playerLocal = true;
                    break;
                }

                case "unit.create": {
                    Assert.AreNotEqual(default, unitType);
                    var player = stack.Pop<Player>();
                    var unit = new Unit(player, unitType, unitPosition, unitRotation, unitHp, unitFuel, unitMoved, unitViewPrefab);
                    stack.Push(unit);
                    ResetUnitValues();
                    break;
                }
                case "unit.get-player": {
                    stack.Push(stack.Pop<Unit>().player);
                    break;
                }
                case "unit.set-position": {
                    unitPosition = stack.Pop<Vector2Int>();
                    break;
                }
                case "unit.set-moved": {
                    unitMoved = true;
                    break;
                }
                case "unit.set-type": {
                    unitType = stack.Pop<UnitType>();
                    break;
                }
                case "unit.set-look-direction": {
                    unitRotation = stack.Pop<Vector2Int>();
                    break;
                }
                case "unit.set-hp": {
                    unitHp = stack.Pop<int>();
                    break;
                }
                case "unit.set-fuel": {
                    unitFuel = stack.Pop<int>();
                    break;
                }
                case "unit.set-prefab": {
                    unitViewPrefab = stack.Pop<string>().LoadAs<UnitView>();
                    break;
                }
                case "unit.put-into": {
                    var unit = stack.Pop<Unit>();
                    var carrier = stack.Pop<Unit>();
                    carrier.cargo.Add(unit);
                    unit.carrier.v = carrier;
                    break;
                }

                case "nl": {
                    scanPosition.x = startPosition.x;
                    scanPosition += nextLineOffset;
                    break;
                }
                case "tilemap.set-next-line-offset": {
                    nextLineOffset = stack.Pop<Vector2Int>();
                    break;
                }
                case "tilemap.set-start-position": {
                    Assert.AreEqual(0, main.tiles.Count);
                    startPosition = stack.Pop<Vector2Int>();
                    scanPosition = startPosition;
                    break;
                }
                case "n": {
                    hasPlayer = false;
                    break;
                }

                default: {
                    if (parseMnemonic.TryGetValue(token, out var tileType)) {
                        main.tiles.Add(scanPosition, tileType);
                        if (TileType.Buildings.HasFlag(tileType)) {
                            var cp = stack.Pop<int>();
                            Player player = null;
                            if (hasPlayer) {
                                var id = stack.Pop<string>();
                                var found = playerLookup.TryGetValue(id, out player);
                                Assert.IsTrue(found, id);
                            }
                            new Building(main, scanPosition, tileType, player, cp);
                        }
                        scanPosition += nextTileDelta;
                        hasPlayer = true;
                    }
                    else
                        stack.ExecuteToken(token);
                    break;
                }
            }
        }

        Assert.IsNotNull(main.localPlayer);

        var cameraRig = CameraRig.Instance;
        if (cameraRig) {
            var clampToHull = cameraRig.GetComponent<ClampToHull>();
            if (clampToHull)
                clampToHull.Recalculate(main.tiles.Keys);
        }
    }
}