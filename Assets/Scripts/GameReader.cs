using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameReader {

    public static void ReadInto(Main main, string input, bool spawnBuildingViews = false,
        bool selectExistingPlayersInsteadOfCreatingNewOnes = false, bool loadCameraRig = true) {

        ReadInto(main, input, Vector2Int.one, spawnBuildingViews, selectExistingPlayersInsteadOfCreatingNewOnes);
    }

    public static void ReadInto(Main main, string input, Vector2Int transform, bool spawnBuildingViews = false,
        bool selectExistingPlayersInsteadOfCreatingNewOnes = false, bool loadCameraRig = true) {

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
        string playerName;

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
            playerName = null;
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
            try {
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

                            playerIndex++;
                            Assert.IsTrue(playerIndex >= 0 && playerIndex < main.players.Count);
                            main.stack.Push(main.players[playerIndex]);

                        }
                        else {
                            if (playerColor is not { } color)
                                throw new AssertionException("color is null", null);

                            var player = new Player(main, color, playerTeam, playerCredits, playerCo, playerViewPrefab, playerType, playerDifficulty, playerUnitLookDirection, name:playerName);

                            if (playerLocal) {
                                Assert.IsNull(main.localPlayer);
                                main.localPlayer = player;
                            }

                            player.abilityActivationTurn = playerAbilityActivationTurn;
                            player.AbilityMeter = playerPowerMeter;

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
                        var coName = main.stack.Pop<string>();
                        var found = Co.TryGet(coName, out playerCo);
                        Assert.IsTrue(found, coName);
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
                        playerUnitLookDirection = main.stack.Pop<Vector2Int>() * transform;
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
                        break;
                    }
                    case "player.set-name": {
                        playerName = main.stack.Pop<string>();
                        break;
                    }

                    case "tile.add": {
                        var position = main.stack.Pop<Vector2Int>();
                        var type = main.stack.Pop<TileType>();
                        Assert.IsTrue(!main.tiles.ContainsKey(position), position.ToString());
                        main.tiles.Add(position, type);
                        ResetTileValues();
                        break;
                    }
                    case "tile.set-position": {
                        tilePosition = main.stack.Pop<Vector2Int>() * (transform);
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
                        var viewPrefab = !spawnBuildingViews ? null : ((Main2)main).buildingPrefabs.TryGetValue(buildingType, out var p) ? p : "WbFactory".LoadAs<BuildingView>();

                        var building = new Building(main, position, buildingType, player, buildingCp, viewPrefab, buildingLookDirection);

                        building.missileSiloLastLaunchTurn = buildingMissileSiloLastLaunchTurn;
                        building.missileSiloLaunchCooldown = buildingMissileSiloLaunchCooldown;
                        building.missileSiloAmmo = buildingMissileSiloLaunchAmmo;
                        building.missileSiloRange = buildingMissileSiloRange;
                        building.missileBlastRange = buildingMissileSiloBlastRange;
                        building.missileUnitDamage = buildingMissileSiloMissileUnitDamage;
                        building.missileBridgeDamage = buildingMissileSiloMissileBridgeDamage;

                        main.stack.Push(building);

                        ResetBuildingValues();
                        break;
                    }
                    case "building.set-type": {
                        buildingType = main.stack.Pop<TileType>();
                        break;
                    }
                    case "building.set-position": {
                        buildingPosition = main.stack.Pop<Vector2Int>() * (transform);
                        break;
                    }
                    case "building.set-cp": {
                        buildingCp = main.stack.Pop<int>();
                        break;
                    }
                    case "building.set-look-direction": {
                        buildingLookDirection = main.stack.Pop<Vector2Int>() * transform;
                        break;
                    }
                    case "building.missile-silo.set-last-launch-turn": {
                        buildingMissileSiloLastLaunchTurn = main.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-launch-cooldown": {
                        buildingMissileSiloLaunchCooldown = main.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-ammo": {
                        buildingMissileSiloLaunchAmmo = main.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-range": {
                        buildingMissileSiloRange = main.stack.Pop<Vector2Int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-blast-range": {
                        buildingMissileSiloBlastRange = main.stack.Pop<Vector2Int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-unit-damage": {
                        buildingMissileSiloMissileUnitDamage = main.stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-bridge-damage": {
                        buildingMissileSiloMissileBridgeDamage = main.stack.Pop<int>();
                        break;
                    }

                    case "unit.add": {
                        if (unitType is not { } type)
                            throw new AssertionException("unitType == null", "");
                        var player = main.stack.Pop<Player>();
                        main.stack.Push(new Unit(player, type, unitPosition, unitLookDirection, unitHp, unitFuel, unitMoved, unitViewPrefab));
                        ResetUnitValues();
                        break;
                    }
                    case "unit.get-player": {
                        main.stack.Push(main.stack.Pop<Unit>().Player);
                        break;
                    }
                    case "unit.set-position": {
                        unitPosition = main.stack.Pop<Vector2Int>() * transform;
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
                        unitLookDirection = main.stack.Pop<Vector2Int>() * transform;
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
                        carrier.AddCargo(unit);
                        unit.Carrier = carrier;
                        break;
                    }

                    case "trigger.select": {
                        trigger = main.stack.Pop<TriggerName>();
                        break;
                    }

                    case "trigger.add-position": {
                        var position = main.stack.Pop<Vector2Int>() * transform;
                        if (trigger is not { } value)
                            throw new AssertionException("trigger is null", position.ToString());
                        Assert.IsTrue(main.triggers.ContainsKey(value), value.ToString());
                        main.triggers[value].Add(position);
                        break;
                    }

                    case "bridge.add": {
                        Assert.AreNotEqual(0, bridgePositions.Count);
                        Assert.IsTrue(bridgeView);
                        main.stack.Push(new Bridge(main, bridgePositions, bridgeView, bridgeHp));
                        ResetBridgeValues();
                        break;
                    }

                    case "bridge.set-view": {
                        bridgeView = (BridgeView)main.stack.Pop<object>();
                        break;
                    }

                    case "bridge.add-position": {
                        var position = main.stack.Pop<Vector2Int>() * transform;
                        Assert.IsFalse(main.bridges.Any(bridge => bridge.tiles.ContainsKey(position)));
                        bridgePositions.Add(position);
                        break;
                    }

                    case "bridge.set-hp": {
                        bridgeHp = main.stack.Pop<int>();
                        break;
                    }

                    case "camera-rig.set-position": {
                        var position = main.stack.Pop<Vector3>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.transform.position = position;
                        break;
                    }
                    case "camera-rig.set-rotation": {
                        var angle = main.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.transform.rotation = Quaternion.Euler(0, angle, 0);
                        break;
                    }
                    case "camera-rig.set-distance": {
                        var distance = main.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.targetDistance = cameraRig.distance = distance;
                        break;
                    }
                    case "camera-rig.set-pitch-angle": {
                        var pitchAngle = main.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.tagetPitchAngle = cameraRig.pitchAngle = pitchAngle;
                        break;
                    }
                    case "camera-rig.set-fov": {
                        var fov = main.stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.Fov = fov;
                        break;
                    }

                    default:
                        main.stack.ExecuteToken(token);
                        break;
                }
            }
            
            catch (Exception) {

                const int radius = 10;
                var outline = new string('-', 32);
                
                var (line, column) = token.CalculateLocation();
                var lines = input.Split('\n');
                var low = Mathf.Max(0, line - radius);
                var high = Mathf.Min(lines.Length - 1, line + radius);

                var text = $"{nameof(GameReader)}: {line+1}:{column+1}: {lines[line]}\n{outline}\n";
                for (var i = low; i <= high; i++) {
                    text += $"{i,3} ";
                    text += string.Format(i == line ? "<b>{0}</b>\n" : "{0}\n", lines[i]);
                }
                text += $"{outline}\n\n\n\n";

                Debug.LogError(text);
                throw;
            }
        }
    }
}