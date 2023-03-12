using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static BattleConstants;

public static class GameReader {
    
    static GameReader() {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    }

    public static void ReadInto(Level level, string input, bool spawnBuildingViews = false,
        bool selectExistingPlayersInsteadOfCreatingNewOnes = false, bool loadCameraRig = true) {

        ReadInto(level, input, Vector2Int.one, spawnBuildingViews, selectExistingPlayersInsteadOfCreatingNewOnes);
    }

    public static void ReadInto(Level level, string input, Vector2Int transform, bool spawnBuildingViews = false,
        bool selectExistingPlayersInsteadOfCreatingNewOnes = false, bool loadCameraRig = true) {

        Team playerTeam;
        PersonName playerCoName;
        PlayerType playerType;
        AiDifficulty playerDifficulty;
        int playerCredits;
        PlayerView playerViewPrefab;
        bool playerLocal;
        Vector2Int? playerUnitLookDirection;
        ColorName? playerColorName;
        int? playerAbilityActivationTurn;
        int playerPowerMeter;
        string playerName;
        int playerSide;
        Vector2Int? playerUiPosition;

        void ResetPlayerValues() {
            playerTeam = Team.None;
            playerCoName = PersonName.Natalie;
            playerType = PlayerType.Human;
            playerDifficulty = AiDifficulty.Normal;
            playerCredits = 0;
            playerViewPrefab = PlayerView.DefaultPrefab;
            playerLocal = false;
            playerColorName=null;
            playerUnitLookDirection = null;
            playerAbilityActivationTurn = null;
            playerPowerMeter = 0;
            playerName = null;
            playerSide = left;
            playerUiPosition = null;
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
        int buildingMissileSiloLastLaunchTurn;
        int buildingMissileSiloLaunchCooldown;
        int buildingMissileSiloLaunchAmmo;
        Vector2Int buildingMissileSiloRange;
        Vector2Int buildingMissileSiloBlastRange;
        int buildingMissileSiloMissileUnitDamage;
        int buildingMissileSiloMissileBridgeDamage;
        void ResetBuildingValues() {
            buildingPosition = null;
            buildingType = 0;
            buildingCp = 20;
            buildingLookDirection = null;
            buildingMissileSiloLastLaunchTurn = -99;
            buildingMissileSiloLaunchCooldown = 1;
            buildingMissileSiloLaunchAmmo = 999;
            buildingMissileSiloRange = new Vector2Int(0, 999);
            buildingMissileSiloBlastRange = new Vector2Int(0, 3);
            buildingMissileSiloMissileUnitDamage = 5;
            buildingMissileSiloMissileBridgeDamage = 10;
        }
        ResetBuildingValues();

        Vector2Int? unitPosition;
        Vector2Int? unitLookDirection;
        UnitType? unitType;
        bool unitMoved;
        int unitHp;
        int unitFuel;
        UnitView unitViewPrefab;

        void ResetUnitValues() {
            unitPosition = null;
            unitLookDirection = null;
            unitType = null;
            unitMoved = false;
            unitHp = 10;
            unitFuel = 99;
            unitViewPrefab = UnitView.DefaultPrefab;
        }
        ResetUnitValues();

        int bridgeHp;
        BridgeView bridgeView;
        HashSet<Vector2Int> bridgePositions = new HashSet<Vector2Int>();
        void ResetBridgeValues() {
            bridgeHp = 20;
            bridgeView = null;
            bridgePositions.Clear();
        }
        ResetBridgeValues();

        TriggerName? trigger = null;

        CameraRig.TryFind(out var cameraRig);

        var playerIndex = -1;

        foreach (var token in Tokenizer.Tokenize(input)) {
            // try {
                switch (token) {

                    case "game.set-turn": {
                        level.turn = level.stack.Pop<int>();
                        break;
                    }
                    case "game.load-scene": {
                        var name = level.stack.Pop<string>();
                        break;
                    }
                    case "game.set-mission-name": {
                        var name = level.stack.Pop<MissionName>();
                        level.missionName = name;
                        break;
                    }

                    case "player.add": {

                        if (selectExistingPlayersInsteadOfCreatingNewOnes) {

                            playerIndex++;
                            Assert.IsTrue(playerIndex >= 0 && playerIndex < level.players.Count);
                            level.stack.Push(level.players[playerIndex]);

                        }
                        else {
                            if (playerColorName is not { } colorName)
                                throw new AssertionException("colorName is null", null);

                            var player = new Player(level, colorName, playerTeam, playerCredits, playerCoName, playerViewPrefab, playerType, playerDifficulty, playerUnitLookDirection, name:playerName, uiPosition:playerUiPosition);

                            if (playerLocal) {
                                Assert.IsNull(level.localPlayer);
                                level.localPlayer = player;
                            }

                            player.abilityActivationTurn = playerAbilityActivationTurn;
                            player.AbilityMeter = playerPowerMeter;

                            player.side = playerSide;

                            level.stack.Push(player);
                        }

                        ResetPlayerValues();
                        break;
                    }

                    case "player.set-color-name": {
                        playerColorName = level.stack.Pop<ColorName>();
                        break;
                    }
                    case "player.set-team": {
                        playerTeam = level.stack.Pop<Team>();
                        break;
                    }
                    case "player.set-credits": {
                        playerCredits = level.stack.Pop<int>();
                        break;
                    }
                    case "player.set-co-name": {
                        playerCoName = level.stack.Pop<PersonName>();
                        Assert.IsTrue(People.IsCo(playerCoName));
                        break;
                    }
                    case "player.set-prefab": {
                        playerViewPrefab = level.stack.Pop<string>().LoadAs<PlayerView>();
                        break;
                    }
                    case "player.set-ai": {
                        playerType = PlayerType.Ai;
                        playerDifficulty = level.stack.Pop<AiDifficulty>();
                        break;
                    }
                    case "player.mark-as-local": {
                        playerLocal = true;
                        break;
                    }
                    case "player.set-unit-look-direction": {
                        playerUnitLookDirection = level.stack.Pop<Vector2Int>() * transform;
                        break;
                    }
                    case "player.set-ability-activation-turn": {
                        playerAbilityActivationTurn = level.stack.Pop<int>();
                        break;
                    }
                    case "player.set-power-meter": {
                        playerPowerMeter = level.stack.Pop<int>();
                        break;
                    }
                    case "player.on-additive-load-get-by-index": {
                        break;
                    }
                    case "player.set-name": {
                        playerName = level.stack.Pop<string>();
                        break;
                    }
                    case "player.set-side": {
                        playerSide = level.stack.Pop<int>();
                        break;
                    }
                    case "player.set-ui-position": {
                        playerUiPosition = level.stack.Pop<Vector2Int>();
                        break;
                    }

                    case "tile.add": {
                        var position = level.stack.Pop<Vector2Int>() * transform;
                        var type = level.stack.Pop<TileType>();
                        Assert.IsTrue(!level.tiles.ContainsKey(position), position.ToString());
                        level.tiles.Add(position, type);
                        ResetTileValues();
                        break;
                    }
                    case "tile.set-position": {
                        tilePosition = level.stack.Pop<Vector2Int>() * (transform);
                        break;
                    }
                    case "tile.set-type": {
                        tileType = level.stack.Pop<TileType>();
                        break;
                    }

                    case "building.add": {

                        Assert.AreNotEqual((TileType)0, buildingType);
                        if (buildingPosition is not { } position)
                            throw new AssertionException("buildingPosition is null", buildingType.ToString());
                        Assert.IsTrue(!level.buildings.ContainsKey(position), position.ToString());

                        var player = level.stack.Pop<Player>();
                        var viewPrefab = !spawnBuildingViews ? null : BuildingView.GetPrefab(buildingType);

                        var building = new Building(level, position, buildingType, player, buildingCp, viewPrefab, buildingLookDirection);

                        building.missileSiloLastLaunchTurn = buildingMissileSiloLastLaunchTurn;
                        building.missileSiloLaunchCooldown = buildingMissileSiloLaunchCooldown;
                        building.missileSiloAmmo = buildingMissileSiloLaunchAmmo;
                        building.missileSiloRange = buildingMissileSiloRange;
                        building.missileBlastRange = buildingMissileSiloBlastRange;
                        building.missileUnitDamage = buildingMissileSiloMissileUnitDamage;
                        building.missileBridgeDamage = buildingMissileSiloMissileBridgeDamage;

                        level.stack.Push(building);

                        ResetBuildingValues();
                        break;
                    }
                    case "building.set-type": {
                        buildingType = level.stack.Pop<TileType>();
                        break;
                    }
                    case "building.set-position": {
                        buildingPosition = level.stack.Pop<Vector2Int>() * (transform);
                        break;
                    }
                    case "building.set-cp": {
                        buildingCp = level.stack.Pop<int>();
                        break;
                    }
                    case "building.set-look-direction": {
                        buildingLookDirection = level.stack.Pop<Vector2Int>() * transform;
                        break;
                    }
                    case "building.missile-silo.set-last-launch-turn": {
                        buildingMissileSiloLastLaunchTurn = level.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-launch-cooldown": {
                        buildingMissileSiloLaunchCooldown = level.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-ammo": {
                        buildingMissileSiloLaunchAmmo = level.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-range": {
                        buildingMissileSiloRange = level.stack.Pop<Vector2Int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-blast-range": {
                        buildingMissileSiloBlastRange = level.stack.Pop<Vector2Int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-unit-damage": {
                        buildingMissileSiloMissileUnitDamage = level.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-bridge-damage": {
                        buildingMissileSiloMissileBridgeDamage = level.stack.Pop<int>();
                        break;
                    }

                    case "unit.add": {
                        if (unitType is not { } type)
                            throw new AssertionException("unitType == null", "");
                        var player = level.stack.Pop<Player>();
                        level.stack.Push(new Unit(player, type, unitPosition, unitLookDirection, unitHp, unitFuel, unitMoved, unitViewPrefab));
                        ResetUnitValues();
                        break;
                    }
                    case "unit.get-player": {
                        level.stack.Push(level.stack.Pop<Unit>().Player);
                        break;
                    }
                    case "unit.set-position": {
                        unitPosition = level.stack.Pop<Vector2Int>() * transform;
                        break;
                    }
                    case "unit.set-moved": {
                        unitMoved = level.stack.Pop<bool>();
                        break;
                    }
                    case "unit.set-type": {
                        unitType = level.stack.Pop<UnitType>();
                        break;
                    }
                    case "unit.set-look-direction": {
                        //unitLookDirection = main.stack.Pop<Vector2Int>() * transform;
                        level.stack.Pop<Vector2Int>();
                        break;
                    }
                    case "unit.set-hp": {
                        unitHp = level.stack.Pop<int>();
                        break;
                    }
                    case "unit.set-fuel": {
                        unitFuel = level.stack.Pop<int>();
                        break;
                    }
                    case "unit.set-view-prefab": {
                        unitViewPrefab = level.stack.Pop<UnitView>();
                        break;
                    }
                    case "unit.put-into": {
                        var unit = level.stack.Pop<Unit>();
                        var carrier = level.stack.Pop<Unit>();
                        carrier.AddCargo(unit);
                        unit.Carrier = carrier;
                        break;
                    }

                    case "trigger.select": {
                        trigger = level.stack.Pop<TriggerName>();
                        break;
                    }

                    case "trigger.add-position": {
                        var position = level.stack.Pop<Vector2Int>() * transform;
                        if (trigger is not { } value)
                            throw new AssertionException("trigger is null", position.ToString());
                        Assert.IsTrue(level.triggers.ContainsKey(value), value.ToString());
                        level.triggers[value].Add(position);
                        break;
                    }

                    case "bridge.add": {
                        Assert.AreNotEqual(0, bridgePositions.Count);
                        Assert.IsTrue(bridgeView);
                        level.stack.Push(new Bridge(level, bridgePositions, bridgeView, bridgeHp));
                        ResetBridgeValues();
                        break;
                    }

                    case "bridge.set-view": {
                        bridgeView = (BridgeView)level.stack.Pop<object>();
                        break;
                    }

                    case "bridge.add-position": {
                        var position = level.stack.Pop<Vector2Int>() * transform;
                        Assert.IsFalse(level.bridges.Any(bridge => bridge.tiles.ContainsKey(position)));
                        bridgePositions.Add(position);
                        break;
                    }

                    case "bridge.set-hp": {
                        bridgeHp = level.stack.Pop<int>();
                        break;
                    }

                    case "camera-rig.set-position": {
                        var position = level.stack.Pop<Vector3>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.transform.position = position;
                        break;
                    }
                    case "camera-rig.set-rotation": {
                        var angle = level.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.transform.rotation = Quaternion.Euler(0, angle, 0);
                        break;
                    }
                    case "camera-rig.set-distance": {
                        var distance = level.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.targetDistance = cameraRig.distance = distance;
                        break;
                    }
                    case "camera-rig.set-pitch-angle": {
                        var pitchAngle = level.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.tagetPitchAngle = cameraRig.pitchAngle = pitchAngle;
                        break;
                    }
                    case "camera-rig.set-fov": {
                        var fov = level.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.Fov = fov;
                        break;
                    }

                    default:
                        level.stack.ExecuteToken(token);
                        break;
                }
            // }
            //
            // catch (Exception e) {
            //
            //     const int radius = 10;
            //     var outline = new string('-', 32);
            //     
            //     var (line, column) = token.CalculateLocation();
            //     var lines = input.Split('\n');
            //     var low = Mathf.Max(0, line - radius);
            //     var high = Mathf.Min(lines.Length - 1, line + radius);
            //
            //     var text = $"{nameof(GameReader)}: {line+1}:{column+1}: {lines[line]}\n{outline}\n";
            //     for (var i = low; i <= high; i++) {
            //         text += $"{i,3} ";
            //         text += string.Format(i == line ? "<b>{0}</b>\n" : "{0}\n", lines[i]);
            //     }
            //     text += $"{outline}\n\n\n\n";
            //
            //     Debug.LogError(text);
            //     Debug.LogError(e.ToString());
            //     throw;
            // }
        }
    }
}