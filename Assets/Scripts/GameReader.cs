using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static BattleConstants;

public static class GameReader {

    public static readonly WarsStack stack = new();
    
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
        int playerAbilityMeter;
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
            playerAbilityMeter = 0;
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

        stack.Clear();
        foreach (var token in Tokenizer.Tokenize(input)) {
            // try {
                switch (token) {

                    case "game.set-turn": {
                        level.turn = stack.Pop<int>();
                        break;
                    }
                    case "game.load-scene": {
                        var name = stack.Pop<string>();
                        break;
                    }
                    case "game.set-mission-name": {
                        var name = stack.Pop<MissionName>();
                        level.missionName = name;
                        break;
                    }

                    case "player.add": {

                        if (selectExistingPlayersInsteadOfCreatingNewOnes) {

                            playerIndex++;
                            Assert.IsTrue(playerIndex >= 0 && playerIndex < level.players.Count);
                            stack.Push(level.players[playerIndex]);

                        }
                        else {
                            if (playerColorName is not { } colorName)
                                throw new AssertionException("colorName is null", null);

                            var player = new Player(level, colorName, playerTeam, playerCredits, playerCoName, playerViewPrefab, playerType, playerDifficulty, playerUnitLookDirection, uiPosition:playerUiPosition,
                                abilityActivationTurn:playerAbilityActivationTurn, side:playerSide, abilityMeter:playerAbilityMeter);

                            if (playerLocal) {
                                Assert.IsNull(level.localPlayer);
                                level.localPlayer = player;
                            }

                            stack.Push(player);
                        }

                        ResetPlayerValues();
                        break;
                    }

                    case "player.set-color-name": {
                        playerColorName = stack.Pop<ColorName>();
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
                    case "player.set-co-name": {
                        playerCoName = stack.Pop<PersonName>();
                        Assert.IsTrue(People.IsCo(playerCoName));
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
                        playerUnitLookDirection = stack.Pop<Vector2Int>() * transform;
                        break;
                    }
                    case "player.set-ability-activation-turn": {
                        playerAbilityActivationTurn = stack.Pop<int>();
                        break;
                    }
                    case "player.set-power-meter": {
                        playerAbilityMeter = stack.Pop<int>();
                        break;
                    }
                    case "player.on-additive-load-get-by-index": {
                        break;
                    }
                    case "player.set-side": {
                        playerSide = stack.Pop<int>();
                        break;
                    }
                    case "player.set-ui-position": {
                        playerUiPosition = stack.Pop<Vector2Int>();
                        break;
                    }

                    case "tile.add": {
                        var position = stack.Pop<Vector2Int>() * transform;
                        var type = stack.Pop<TileType>();
                        Assert.IsTrue(!level.tiles.ContainsKey(position), position.ToString());
                        level.tiles.Add(position, type);
                        ResetTileValues();
                        break;
                    }
                    case "tile.set-position": {
                        tilePosition = stack.Pop<Vector2Int>() * (transform);
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
                        Assert.IsTrue(!level.buildings.ContainsKey(position), position.ToString());

                        var player = stack.Pop<Player>();
                        var viewPrefab = !spawnBuildingViews ? null : BuildingView.GetPrefab(buildingType);

                        var building = new Building(level, position, buildingType, player, buildingCp, viewPrefab, buildingLookDirection);

                        building.missileSiloLastLaunchTurn = buildingMissileSiloLastLaunchTurn;
                        building.missileSiloLaunchCooldown = buildingMissileSiloLaunchCooldown;
                        building.missileSiloAmmo = buildingMissileSiloLaunchAmmo;
                        building.missileSiloRange = buildingMissileSiloRange;
                        building.missileBlastRange = buildingMissileSiloBlastRange;
                        building.missileUnitDamage = buildingMissileSiloMissileUnitDamage;
                        building.missileBridgeDamage = buildingMissileSiloMissileBridgeDamage;

                        stack.Push(building);

                        ResetBuildingValues();
                        break;
                    }
                    case "building.set-type": {
                        buildingType = stack.Pop<TileType>();
                        break;
                    }
                    case "building.set-position": {
                        buildingPosition = stack.Pop<Vector2Int>() * (transform);
                        break;
                    }
                    case "building.set-cp": {
                        buildingCp = stack.Pop<int>();
                        break;
                    }
                    case "building.set-look-direction": {
                        buildingLookDirection = stack.Pop<Vector2Int>() * transform;
                        break;
                    }
                    case "building.missile-silo.set-last-launch-turn": {
                        buildingMissileSiloLastLaunchTurn = stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-launch-cooldown": {
                        buildingMissileSiloLaunchCooldown = stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-ammo": {
                        buildingMissileSiloLaunchAmmo = stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.set-range": {
                        buildingMissileSiloRange = stack.Pop<Vector2Int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-blast-range": {
                        buildingMissileSiloBlastRange = stack.Pop<Vector2Int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-unit-damage": {
                        buildingMissileSiloMissileUnitDamage = stack.Pop<int>();
                        break;
                    }
                    case "building.missile-silo.missile.set-bridge-damage": {
                        buildingMissileSiloMissileBridgeDamage = stack.Pop<int>();
                        break;
                    }

                    case "unit.add": {
                        if (unitType is not { } type)
                            throw new AssertionException("unitType == null", "");
                        var player = stack.Pop<Player>();
                        stack.Push(new Unit(player, type, unitPosition, unitLookDirection, unitHp, unitFuel, unitMoved, unitViewPrefab));
                        ResetUnitValues();
                        break;
                    }
                    case "unit.get-player": {
                        stack.Push(stack.Pop<Unit>().Player);
                        break;
                    }
                    case "unit.set-position": {
                        unitPosition = stack.Pop<Vector2Int>() * transform;
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
                        //unitLookDirection = main.stack.Pop<Vector2Int>() * transform;
                        stack.Pop<Vector2Int>();
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
                        carrier.AddCargo(unit);
                        unit.Carrier = carrier;
                        break;
                    }

                    case "trigger.select": {
                        trigger = stack.Pop<TriggerName>();
                        break;
                    }

                    case "trigger.add-position": {
                        var position = stack.Pop<Vector2Int>() * transform;
                        if (trigger is not { } value)
                            throw new AssertionException("trigger is null", position.ToString());
                        Assert.IsTrue(level.triggers.ContainsKey(value), value.ToString());
                        level.triggers[value].Add(position);
                        break;
                    }

                    case "bridge.add": {
                        Assert.AreNotEqual(0, bridgePositions.Count);
                        Assert.IsTrue(bridgeView);
                        stack.Push(new Bridge(level, bridgePositions, bridgeView, bridgeHp));
                        ResetBridgeValues();
                        break;
                    }

                    case "bridge.set-view": {
                        bridgeView = (BridgeView)stack.Pop<object>();
                        break;
                    }

                    case "bridge.add-position": {
                        var position = stack.Pop<Vector2Int>() * transform;
                        Assert.IsFalse(level.bridges.Any(bridge => bridge.tiles.ContainsKey(position)));
                        bridgePositions.Add(position);
                        break;
                    }

                    case "bridge.set-hp": {
                        bridgeHp = stack.Pop<int>();
                        break;
                    }

                    case "camera-rig.set-position": {
                        var position = stack.Pop<Vector3>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.transform.position = position;
                        break;
                    }
                    case "camera-rig.set-rotation": {
                        var angle = stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.transform.rotation = Quaternion.Euler(0, angle, 0);
                        break;
                    }
                    case "camera-rig.set-distance": {
                        var distance = stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.targetDistance = cameraRig.distance = distance;
                        break;
                    }
                    case "camera-rig.set-pitch-angle": {
                        var pitchAngle = stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.tagetPitchAngle = cameraRig.pitchAngle = pitchAngle;
                        break;
                    }
                    case "camera-rig.set-fov": {
                        var fov = stack.Pop<dynamic>();
                        if (loadCameraRig && cameraRig)
                            cameraRig.Fov = fov;
                        break;
                    }

                    default:
                        stack.ExecuteToken(token);
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