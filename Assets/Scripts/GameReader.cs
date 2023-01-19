using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameReader {
    
    public static void LoadInto(Main main, string input, bool spawnBuildingViews = false) {

        var playerLookup = new Dictionary<string, Player>();

        Team playerTeam;
        Co playerCo;
        PlayerType playerType;
        AiDifficulty playerDifficulty;
        int playerCredits;
        PlayerView playerViewPrefab;
        bool playerLocal;
        Vector2Int? playerUnitLookDirection;
        Color playerColor;
        string playerLookupId;
        int? playerAbilityActivationTurn;
        int playerPowerMeter;

        void ResetPlayerValues() {
            playerTeam = Team.None;
            playerCo = Co.Natalie;
            playerType = PlayerType.Human;
            playerDifficulty = AiDifficulty.Normal;
            playerCredits = 0;
            playerViewPrefab = PlayerView.DefaultPrefab;
            playerLocal = false;
            playerColor = Palette.white;
            playerLookupId = null;
            playerUnitLookDirection = null;
            playerAbilityActivationTurn = null;
            playerPowerMeter = 0;
        }
        ResetPlayerValues();

        Vector2Int? tilePosition;
        TileType tileType;
        void ResetTileValues() {
            tilePosition = null;
            tileType = 0;
        }
        ResetTileValues();

        Vector2Int? buildingPosition;
        TileType buildingType;
        int buildingCp;
        Vector2Int? buildingLookDirection;
        void ResetBuildingValues() {
            buildingPosition = null;
            buildingType = 0;
            buildingCp = 20;
            buildingLookDirection = null;
        }
        ResetBuildingValues();

        Vector2Int? unitPosition;
        Vector2Int? unitLookDirection;
        UnitType unitType;
        bool unitMoved;
        int unitHp;
        int unitFuel;
        UnitView unitViewPrefab;

        void ResetUnitValues() {
            unitPosition = null;
            unitLookDirection = null;
            unitType = UnitType.Infantry;
            unitMoved = false;
            unitHp = 10;
            unitFuel = 99;
            unitViewPrefab = UnitView.DefaultPrefab;
        }
        ResetUnitValues();

        int bridgeHp ;
        BridgeView bridgeView;
        HashSet<Vector2Int> bridgePositions =new HashSet<Vector2Int>();
        void ResetBridgeValues() {
            bridgeHp = 20;
            bridgeView = null;
            bridgePositions.Clear();
        }
        ResetBridgeValues();

        Trigger? trigger = null;

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
                case "game.set-mission-name": {
                    var name = stack.Pop<MissionName>();
                    main.missionName = name;
                    break;
                }

                case "player.add": {

                    Assert.IsTrue(playerLookupId == null || !playerLookup.ContainsKey(playerLookupId), playerLookupId);

                    var player = new Player(main, playerColor, playerTeam, playerCredits, playerCo, playerViewPrefab, playerType, playerDifficulty, playerUnitLookDirection);

                    if (playerLookupId != null)
                        playerLookup.Add(playerLookupId, player);

                    if (playerLocal) {
                        Assert.IsNull(main.localPlayer);
                        main.localPlayer = player;
                    }

                    player.abilityActivationTurn = playerAbilityActivationTurn;
                    player.powerMeter = playerPowerMeter;

                    stack.Push(player);
                    ResetPlayerValues();
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
                case "player.set-unit-look-direction": {
                    playerUnitLookDirection = stack.Pop<Vector2Int>();
                    break;
                }
                case "player.set-ability-activation-turn": {
                    playerAbilityActivationTurn = stack.Pop<int>();
                    break;
                }
                case "player.set-power-meter": {
                    playerPowerMeter = stack.Pop<int>();
                    break;
                }

                case "player.select-by-index": {
                    var index = stack.Pop<int>();
                    Assert.IsTrue(index >= 0 && index < main.players.Count, index.ToString());
                    stack.Push(main.players[index]);
                    break;
                }

                case "tile.add": {
                    Assert.AreNotEqual((TileType)0, tileType);
                    if (tilePosition is not { } position)
                        throw new AssertionException("tilePosition is null", tileType.ToString());
                    Assert.IsTrue(!main.tiles.ContainsKey(position), position.ToString());
                    main.tiles.Add(position, tileType);
                    ResetTileValues();
                    break;
                }
                case "tile.set-position": {
                    tilePosition = stack.Pop<Vector2Int>();
                    break;
                }
                case "tile.set-type": {
                    tileType = stack.Pop<TileType>();
                    break;
                }

                case "building.add": {
                    Assert.AreNotEqual((TileType)0, buildingType);
                    if (buildingPosition is not { } position)
                        throw new AssertionException("buildingPosition is null", buildingType.ToString());
                    Assert.IsTrue(!main.buildings.ContainsKey(position), position.ToString());
                    var player = stack.Pop<Player>();
                    var viewPrefab = spawnBuildingViews ? "WbFactory".LoadAs<BuildingView>() : null;
                    stack.Push(new Building(main, position, buildingType, player, buildingCp, viewPrefab, buildingLookDirection));
                    ResetBuildingValues();
                    break;
                }
                case "building.set-type": {
                    buildingType = stack.Pop<TileType>();
                    break;
                }
                case "building.set-position": {
                    buildingPosition = stack.Pop<Vector2Int>();
                    break;
                }
                case "building.set-cp": {
                    buildingCp = stack.Pop<int>();
                    break;
                }
                case "building.set-look-direction": {
                    buildingLookDirection = stack.Pop<Vector2Int>();
                    break;
                }

                case "unit.add": {
                    Assert.AreNotEqual(default, unitType);
                    var player = stack.Pop<Player>();
                    stack.Push(new Unit(player, unitType, unitPosition, unitLookDirection, unitHp, unitFuel, unitMoved, unitViewPrefab));
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
                    unitMoved = stack.Pop<bool>();
                    break;
                }
                case "unit.set-type": {
                    unitType = stack.Pop<UnitType>();
                    break;
                }
                case "unit.set-look-direction": {
                    unitLookDirection = stack.Pop<Vector2Int>();
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
                case "unit.set-view-prefab": {
                    unitViewPrefab = stack.Pop<UnitView>();
                    break;
                }
                case "unit.put-into": {
                    var unit = stack.Pop<Unit>();
                    var carrier = stack.Pop<Unit>();
                    carrier.cargo.Add(unit);
                    unit.carrier.v = carrier;
                    break;
                }

                case "trigger.select": {
                    trigger = stack.Pop<Trigger>();
                    break;
                }
                
                case "trigger.add-position": {
                    var position = stack.Pop<Vector2Int>();
                    if (trigger is not { } value)
                        throw new AssertionException("trigger is null", position.ToString());
                    main.triggers.Add(position, value);
                    break;
                }

                case "bridge.add": {
                    Assert.AreNotEqual(0,bridgePositions.Count);
                    Assert.IsTrue(bridgeView);
                    var bridge = new Bridge(main,bridgePositions,bridgeView, bridgeHp);
                    main.bridges.Add(bridge);
                    stack.Push(bridge);
                    ResetBridgeValues();
                    break;
                }

                case "bridge.set-view": {
                    bridgeView = stack.Pop<BridgeView>();
                    break;
                }

                case "bridge.add-position": {
                    var position = stack.Pop<Vector2Int>();
                    Assert.IsFalse(main.bridges.Any(bridge => bridge.tiles.ContainsKey(position)));
                    bridgePositions.Add(position);
                    break;
                }

                case "bridge.set-hp": {
                    bridgeHp = stack.Pop<int>();
                    break;
                }

                default:
                    stack.ExecuteToken(token);
                    break;
            }
        }
    }
}