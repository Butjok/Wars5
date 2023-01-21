using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameReader {
    
    public static void ReadInto(Main main, string input, bool spawnBuildingViews = false,
        bool selectExistingPlayersInsteadOfCreatingNewOnes = false) {

        Team playerTeam;
        Co playerCo;
        PlayerType playerType;
        AiDifficulty playerDifficulty;
        int playerCredits;
        PlayerView playerViewPrefab;
        bool playerLocal;
        Vector2Int? playerUnitLookDirection;
        Color? playerColor;
        int? playerAbilityActivationTurn;
        int playerPowerMeter;
        int playerIndex;

        void ResetPlayerValues() {
            playerTeam = Team.None;
            playerCo = Co.Natalie;
            playerType = PlayerType.Human;
            playerDifficulty = AiDifficulty.Normal;
            playerCredits = 0;
            playerViewPrefab = PlayerView.DefaultPrefab;
            playerLocal = false;
            playerColor = null;
            playerUnitLookDirection = null;
            playerAbilityActivationTurn = null;
            playerPowerMeter = 0;
            playerIndex = -1;
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

        foreach (var token in input.Tokenize()) {
            switch (token) {

                case "game.set-turn": {
                    main.turn = main.stack.Pop<int>();
                    break;
                }
                case "game.load-scene": {
                    var name = main.stack.Pop<string>();
                    break;
                }
                case "game.set-mission-name": {
                    var name = main.stack.Pop<MissionName>();
                    main.missionName = name;
                    break;
                }

                case "player.add": {

                    if (selectExistingPlayersInsteadOfCreatingNewOnes) {
                        
                        Assert.AreNotEqual(-1,playerIndex);
                        Assert.IsTrue(playerIndex >= 0 && playerIndex < main.players.Count);
                        main.stack.Push(main.players[playerIndex]);
                        
                    }
                    else {
                        if (playerColor is not { } color)
                            throw new AssertionException("color is null", null);

                        var player = new Player(main, color, playerTeam, playerCredits, playerCo, playerViewPrefab, playerType, playerDifficulty, playerUnitLookDirection);

                        if (playerLocal) {
                            Assert.IsNull(main.localPlayer);
                            main.localPlayer = player;
                        }

                        player.abilityActivationTurn = playerAbilityActivationTurn;
                        player.powerMeter = playerPowerMeter;

                        main.stack.Push(player);
                    }
                    
                    ResetPlayerValues();
                    break;
                }

                case "player.set-color": {
                    var b = main.stack.Pop<dynamic>();
                    var g = main.stack.Pop<dynamic>();
                    var r = main.stack.Pop<dynamic>();
                    playerColor = new Color(r, g, b);
                    break;
                }
                case "player.set-team": {
                    playerTeam = main.stack.Pop<Team>();
                    break;
                }
                case "player.set-credits": {
                    playerCredits = main.stack.Pop<int>();
                    break;
                }
                case "player.set-co": {
                    playerCo = main.stack.Pop<string>().LoadAs<Co>();
                    break;
                }
                case "player.set-prefab": {
                    playerViewPrefab = main.stack.Pop<string>().LoadAs<PlayerView>();
                    break;
                }
                case "player.set-ai": {
                    playerType = PlayerType.Ai;
                    playerDifficulty = main.stack.Pop<AiDifficulty>();
                    break;
                }
                case "player.mark-as-local": {
                    playerLocal = true;
                    break;
                }
                case "player.set-unit-look-direction": {
                    playerUnitLookDirection = main.stack.Pop<Vector2Int>();
                    break;
                }
                case "player.set-ability-activation-turn": {
                    playerAbilityActivationTurn = main.stack.Pop<int>();
                    break;
                }
                case "player.set-power-meter": {
                    playerPowerMeter = main.stack.Pop<int>();
                    break;
                }
                case "player.on-additive-load-get-by-index": {
                    playerIndex = main.stack.Pop<int>();
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
                    tilePosition = main.stack.Pop<Vector2Int>();
                    break;
                }
                case "tile.set-type": {
                    tileType = main.stack.Pop<TileType>();
                    break;
                }

                case "building.add": {
                    Assert.AreNotEqual((TileType)0, buildingType);
                    if (buildingPosition is not { } position)
                        throw new AssertionException("buildingPosition is null", buildingType.ToString());
                    Assert.IsTrue(!main.buildings.ContainsKey(position), position.ToString());
                    var player = main.stack.Pop<Player>();
                    var viewPrefab = spawnBuildingViews ? "WbFactory".LoadAs<BuildingView>() : null;
                    main.stack.Push(new Building(main, position, buildingType, player, buildingCp, viewPrefab, buildingLookDirection));
                    ResetBuildingValues();
                    break;
                }
                case "building.set-type": {
                    buildingType = main.stack.Pop<TileType>();
                    break;
                }
                case "building.set-position": {
                    buildingPosition = main.stack.Pop<Vector2Int>();
                    break;
                }
                case "building.set-cp": {
                    buildingCp = main.stack.Pop<int>();
                    break;
                }
                case "building.set-look-direction": {
                    buildingLookDirection = main.stack.Pop<Vector2Int>();
                    break;
                }

                case "unit.add": {
                    Assert.AreNotEqual(default, unitType);
                    var player = main.stack.Pop<Player>();
                    main.stack.Push(new Unit(player, unitType, unitPosition, unitLookDirection, unitHp, unitFuel, unitMoved, unitViewPrefab));
                    ResetUnitValues();
                    break;
                }
                case "unit.get-player": {
                    main.stack.Push(main.stack.Pop<Unit>().player);
                    break;
                }
                case "unit.set-position": {
                    unitPosition = main.stack.Pop<Vector2Int>();
                    break;
                }
                case "unit.set-moved": {
                    unitMoved = main.stack.Pop<bool>();
                    break;
                }
                case "unit.set-type": {
                    unitType = main.stack.Pop<UnitType>();
                    break;
                }
                case "unit.set-look-direction": {
                    unitLookDirection = main.stack.Pop<Vector2Int>();
                    break;
                }
                case "unit.set-hp": {
                    unitHp = main.stack.Pop<int>();
                    break;
                }
                case "unit.set-fuel": {
                    unitFuel = main.stack.Pop<int>();
                    break;
                }
                case "unit.set-view-prefab": {
                    unitViewPrefab = main.stack.Pop<UnitView>();
                    break;
                }
                case "unit.put-into": {
                    var unit = main.stack.Pop<Unit>();
                    var carrier = main.stack.Pop<Unit>();
                    carrier.cargo.Add(unit);
                    unit.carrier.v = carrier;
                    break;
                }

                case "trigger.select": {
                    trigger = main.stack.Pop<Trigger>();
                    break;
                }
                
                case "trigger.add-position": {
                    var position = main.stack.Pop<Vector2Int>();
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
                    main.stack.Push(bridge);
                    ResetBridgeValues();
                    break;
                }

                case "bridge.set-view": {
                    bridgeView = main.stack.Pop<BridgeView>();
                    break;
                }

                case "bridge.add-position": {
                    var position = main.stack.Pop<Vector2Int>();
                    Assert.IsFalse(main.bridges.Any(bridge => bridge.tiles.ContainsKey(position)));
                    bridgePositions.Add(position);
                    break;
                }

                case "bridge.set-hp": {
                    bridgeHp = main.stack.Pop<int>();
                    break;
                }

                default:
                    main.stack.ExecuteToken(token);
                    break;
            }
        }
    }
}