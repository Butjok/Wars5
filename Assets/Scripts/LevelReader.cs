using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static BattleConstants;

public static class LevelReader {

    public static readonly Stack stack = new();

    static LevelReader() {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    }

    public static void ReadInto(Level level, string input) {
        ReadInto(level, input, Vector2Int.one);
    }

    public static void ReadInto(Level level, string input, Vector2Int transform) {

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
            playerColorName = null;
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

        var cameraRig = level.view.cameraRig;
        var zones = new Dictionary<string, Zone>();

        var unitTargetAssignmentActions = new List<Action>();

        stack.Clear();
        foreach (var token in Tokenizer.Tokenize(input)) {
            // try {
            switch (token) {

                case "game.set-turn": {
                    level.turn = (int)stack.Pop();
                    break;
                }
                case "game.load-scene": {
                    var name = (string)stack.Pop();
                    break;
                }
                case "game.set-mission-name": {
                    var name = (MissionName)stack.Pop();
                    level.missionName = name;
                    break;
                }

                case "player.add": {

                    if (playerColorName is not { } colorName)
                        throw new AssertionException("colorName is null", null);

                    var player = new Player(level, colorName, playerTeam, playerCredits, playerCoName, playerViewPrefab, playerType, playerDifficulty, playerUnitLookDirection, uiPosition: playerUiPosition,
                        abilityActivationTurn: playerAbilityActivationTurn, side: playerSide, abilityMeter: playerAbilityMeter);

                    if (playerLocal) {
                        Assert.IsNull(level.localPlayer);
                        level.localPlayer = player;
                    }

                    stack.Push(player);

                    ResetPlayerValues();
                    break;
                }

                case "player.set-color-name": {
                    playerColorName = (ColorName)stack.Pop();
                    break;
                }
                case "player.set-team": {
                    playerTeam = (Team)stack.Pop();
                    break;
                }
                case "player.set-credits": {
                    playerCredits = (int)stack.Pop();
                    break;
                }
                case "player.set-co-name": {
                    playerCoName = (PersonName)stack.Pop();
                    Assert.IsTrue(Persons.IsCo(playerCoName));
                    break;
                }
                case "player.set-prefab": {
                    playerViewPrefab = ((string)stack.Pop()).LoadAs<PlayerView>();
                    break;
                }
                case "player.set-ai": {
                    playerType = PlayerType.Ai;
                    playerDifficulty = (AiDifficulty)stack.Pop();
                    break;
                }
                case "player.mark-as-local": {
                    playerLocal = true;
                    break;
                }
                case "player.set-unit-look-direction": {
                    playerUnitLookDirection = (Vector2Int)stack.Pop() * transform;
                    break;
                }
                case "player.set-ability-activation-turn": {
                    playerAbilityActivationTurn = (int)stack.Pop();
                    break;
                }
                case "player.set-power-meter": {
                    playerAbilityMeter = (int)stack.Pop();
                    break;
                }
                case "player.on-additive-load-get-by-index": {
                    break;
                }
                case "player.set-side": {
                    playerSide = (int)stack.Pop();
                    break;
                }
                case "player.set-ui-position": {
                    playerUiPosition = (Vector2Int)stack.Pop();
                    break;
                }

                case "tile.add": {
                    var position = (Vector2Int)stack.Pop() * transform;
                    var type = (TileType)stack.Pop();
                    Assert.IsTrue(!level.tiles.ContainsKey(position), position.ToString());
                    level.tiles.Add(position, type);
                    ResetTileValues();
                    break;
                }
                case "tile.set-position": {
                    tilePosition = (Vector2Int)stack.Pop() * (transform);
                    break;
                }
                case "tile.set-type": {
                    tileType = (TileType)stack.Pop();
                    break;
                }

                case "building.add": {

                    Assert.AreNotEqual((TileType)0, buildingType);
                    if (buildingPosition is not { } position)
                        throw new AssertionException("buildingPosition is null", buildingType.ToString());
                    Assert.IsTrue(!level.buildings.ContainsKey(position), position.ToString());

                    var player = (Player)stack.Pop();
                    var viewPrefab = BuildingView.GetPrefab(buildingType);

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
                    buildingType = (TileType)stack.Pop();
                    break;
                }
                case "building.set-position": {
                    buildingPosition = (Vector2Int)stack.Pop() * (transform);
                    break;
                }
                case "building.set-cp": {
                    buildingCp = (int)stack.Pop();
                    break;
                }
                case "building.set-look-direction": {
                    buildingLookDirection = (Vector2Int)stack.Pop() * transform;
                    break;
                }
                case "building.missile-silo.set-last-launch-turn": {
                    buildingMissileSiloLastLaunchTurn = (int)stack.Pop();
                    break;
                }
                case "building.missile-silo.set-launch-cooldown": {
                    buildingMissileSiloLaunchCooldown = (int)stack.Pop();
                    break;
                }
                case "building.missile-silo.set-ammo": {
                    buildingMissileSiloLaunchAmmo = (int)stack.Pop();
                    break;
                }
                case "building.missile-silo.set-range": {
                    buildingMissileSiloRange = (Vector2Int)stack.Pop();
                    break;
                }
                case "building.missile-silo.missile.set-blast-range": {
                    buildingMissileSiloBlastRange = (Vector2Int)stack.Pop();
                    break;
                }
                case "building.missile-silo.missile.set-unit-damage": {
                    buildingMissileSiloMissileUnitDamage = (int)stack.Pop();
                    break;
                }
                case "building.missile-silo.missile.set-bridge-damage": {
                    buildingMissileSiloMissileBridgeDamage = (int)stack.Pop();
                    break;
                }

                case "unit.add": {
                    if (unitType is not { } type)
                        throw new AssertionException("unitType == null", "");
                    var player = (Player)stack.Pop();
                    stack.Push(new Unit(player, type, unitPosition, unitLookDirection, unitHp, unitFuel, unitMoved, unitViewPrefab));
                    ResetUnitValues();
                    break;
                }
                case "unit.get-player": {
                    stack.Push(((Unit)stack.Pop()).Player);
                    break;
                }
                case "unit.set-position": {
                    unitPosition = (Vector2Int)stack.Pop() * transform;
                    break;
                }
                case "unit.set-moved": {
                    unitMoved = (bool)stack.Pop();
                    break;
                }
                case "unit.set-type": {
                    unitType = (UnitType)stack.Pop();
                    break;
                }
                case "unit.set-look-direction": {
                    //unitLookDirection = main.stack.Pop<Vector2Int>() * transform;
                    Vector2Int temp = (Vector2Int)stack.Pop();
                    break;
                }
                case "unit.set-hp": {
                    unitHp = (int)stack.Pop();
                    break;
                }
                case "unit.set-fuel": {
                    unitFuel = (int)stack.Pop();
                    break;
                }
                case "unit.set-view-prefab": {
                    unitViewPrefab = (UnitView)stack.Pop();
                    break;
                }
                case "unit.put-into": {
                    var unit = (Unit)stack.Pop();
                    var carrier = (Unit)stack.Pop();
                    carrier.AddCargo(unit);
                    unit.Carrier = carrier;
                    break;
                }
                case "unit.brain.set-assigned-zone": {
                    var zoneName = (string)stack.Pop();
                    var unit = (Unit)stack.Peek();
                    Assert.IsTrue(zones.TryGetValue(zoneName, out var zone), zoneName);
                    unit.brain.assignedZone = zone;
                    break;
                }
                case "unit.brain.add-state": {
                    var type = (Type)stack.Pop();
                    var unit = (Unit)stack.Peek();
                    var brainState = (UnitBrainState)Activator.CreateInstance(type, unit.brain);
                    unit.brain.states.Push(brainState);
                    stack.Push(brainState);
                    break;
                }
                case "unit.brain.state.attacking-an-enemy.set-target-position": {
                    var targetPosition = (Vector2Int)stack.Pop();
                    var state = (AttackingAnEnemyUnitBrainState)stack.Peek();
                    unitTargetAssignmentActions.Add(() => state.target = level.units[targetPosition]);
                    break;
                }

                case "trigger.select": {
                    trigger = (TriggerName)stack.Pop();
                    break;
                }

                case "trigger.add-position": {
                    var position = (Vector2Int)stack.Pop() * transform;
                    if (trigger is not { } value)
                        throw new AssertionException("trigger is null", position.ToString());
                    Assert.IsTrue(level.triggers.ContainsKey(value), value.ToString());
                    level.triggers[value].Add(position);
                    break;
                }

                case "bridge.add":
                    Assert.AreNotEqual(0, bridgePositions.Count);
                    Assert.IsTrue(bridgeView);
                    stack.Push(new Bridge(level, bridgePositions, bridgeView, bridgeHp));
                    ResetBridgeValues();
                    break;
                case "bridge.set-view":
                    bridgeView = (BridgeView)(object)stack.Pop();
                    break;
                case "bridge.add-position": {
                    var position = (Vector2Int)stack.Pop() * transform;
                    Assert.IsFalse(level.bridges.Any(bridge => bridge.tiles.ContainsKey(position)));
                    bridgePositions.Add(position);
                    break;
                }

                case "bridge.set-hp":
                    bridgeHp = (int)stack.Pop();
                    break;

                case "camera-rig.set-position":
                    cameraRig.transform.position = (Vector3)stack.Pop();
                    break;
                case "camera-rig.set-rotation":
                    cameraRig.transform.rotation = Quaternion.Euler(0, (dynamic)stack.Pop(), 0);
                    break;
                case "camera-rig.set-distance":
                    cameraRig.SetDistance((dynamic)stack.Pop(), false);
                    break;
                case "camera-rig.set-pitch-angle":
                    cameraRig.PitchAngle = (dynamic)stack.Pop();
                    break;

                case "find-in-level-with-name": {
                    var name = (string)stack.Pop();
                    var matches = level.view.GetComponentsInChildren<Transform>().Where(t => t.name == name).ToList();
                    Assert.IsTrue(matches.Count == 1);
                    stack.Push(matches[0].gameObject);
                    break;
                }

                case "zone.add": {
                    var isRoot = (bool)stack.Pop();
                    var name = (string)stack.Pop();
                    var player = (Player)stack.Peek();
                    var zone = new Zone { name = name };
                    if (isRoot)
                        player.rootZone = zone;
                    stack.Push(zone);
                    zones.Add(name, zone);
                    break;
                }
                case "zone.add-position": {
                    var position = (Vector2Int)stack.Pop();
                    var zone = (Zone)stack.Peek();
                    zone.tiles.Add(position);
                    break;
                }
                case "zone.add-distance": {
                    var distance = (int)stack.Pop();
                    var position = (Vector2Int)stack.Pop();
                    var moveType = (MoveType)stack.Pop();
                    var zone = (Zone)stack.Peek();
                    zone.distances.Add((moveType,position), distance);
                    break;
                }
                case "zone.connect": {
                    var toName = (string)stack.Pop();
                    var fromName = (string)stack.Pop();
                    var player = (Player)stack.Peek();
                    Assert.IsTrue(player.rootZone != null);
                    Assert.IsTrue(zones.TryGetValue(fromName, out var from));
                    Assert.IsTrue(zones.TryGetValue(toName, out var to));
                    from.neighbors.Add(to);
                    to.neighbors.Add(from);
                    break;
                }

                default:
                    stack.ExecuteToken(token);
                    break;
            }
        }
    }
}