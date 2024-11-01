using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Stable;
using UnityEngine;
using UnityEngine.Assertions;

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
        AiDifficulty? playerDifficulty;
        int playerCredits;
        PlayerView2 playerViewPrefab;
        bool playerLocal;
        Vector2Int? playerUnitLookDirection;
        ColorName? playerColorName;
        int? playerAbilityActivationTurn;
        int playerAbilityMeter;
        Side playerSide;
        Vector2Int? playerUiPosition;

        void ResetPlayerValues() {
            playerTeam = Team.None;
            playerCoName = PersonName.Natalie;
            playerDifficulty = null;
            playerCredits = 0;
            playerViewPrefab = PlayerView2.DefaultPrefab;
            playerLocal = false;
            playerColorName = null;
            playerUnitLookDirection = null;
            playerAbilityActivationTurn = null;
            playerAbilityMeter = 0;
            playerSide = default;
            playerUiPosition = null;
        }

        ResetPlayerValues();

        Vector2Int? buildingPosition;
        TileType buildingType;
        int buildingCp;
        Vector2Int buildingLookDirection;
        int buildingMissileSiloLastLaunchDay;
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
            buildingLookDirection = Vector2Int.up;
            buildingMissileSiloLastLaunchDay = -99;
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

        var pathPositions = new List<Vector2Int>();
        string pathName;

        void ResetPathValues() {
            pathPositions.Clear();
            pathName = null;
        }

        ResetPathValues();

        TriggerName? trigger = null;

        var cameraRig = level.view.cameraRig;
        var zones = new Dictionary<string, Zone>();

        var unitTargetAssignmentActions = new List<Action>();

        stack.Clear();

        foreach (var token in Tokenizer.Tokenize(input.ToPostfix())) {
            // try {
            switch (token) {
                case "game.set-turn": {
                    level.turn = (int)stack.Pop();
                    break;
                }
                case "game.set-level-name": {
                    level.name = (LevelName)stack.Pop();
                    break;
                }

                case "player.add": {
                    if (playerColorName is not { } colorName)
                        throw new AssertionException("colorName is null", null);

                    var player = new Player {
                        level = level,
                        ColorName = colorName,
                        team = playerTeam,
                        Credits = playerCredits,
                        coName = playerCoName,
                        view = playerViewPrefab,
                        difficulty = playerDifficulty,
                        unitLookDirection = playerUnitLookDirection ?? Vector2Int.up,
                        uiPosition = playerUiPosition ?? Vector2Int.zero,
                        abilityActivationTurn = playerAbilityActivationTurn,
                        side = playerSide,
                        AbilityMeter = playerAbilityMeter,
                    };
                    player.Spawn();

                    if (playerLocal) {
                        Assert.IsNull(level.localPlayer);
                        level.localPlayer = player;
                    }

                    stack.Push(player);

                    ResetPlayerValues();
                    break;
                }

                case "player.add.set-color-name":
                case "player.set-color-name": {
                    playerColorName = (ColorName)stack.Pop();
                    break;
                }
                case "player.add.set-team":
                case "player.set-team": {
                    playerTeam = (Team)stack.Pop();
                    break;
                }
                case "player.add.set-credits":
                case "player.set-credits": {
                    playerCredits = (int)stack.Pop();
                    break;
                }
                case "player.add.set-co-name":
                case "player.set-co-name": {
                    playerCoName = (PersonName)stack.Pop();
                    Assert.IsTrue(Persons.IsCo(playerCoName));
                    break;
                }
                case "player.add.set-prefab":
                case "player.set-prefab": {
                    playerViewPrefab = ((string)stack.Pop()).LoadAs<PlayerView2>();
                    break;
                }
                case "player.add.set-ai":
                case "player.set-ai": {
                    playerDifficulty = (AiDifficulty)stack.Pop();
                    break;
                }
                case "player.add.mark-as-local":
                case "player.mark-as-local": {
                    playerLocal = true;
                    break;
                }
                case "player.add.set-unit-look-direction":
                case "player.set-unit-look-direction": {
                    playerUnitLookDirection = (Vector2Int)stack.Pop() * transform;
                    break;
                }
                case "player.add.set-ability-activation-turn":
                case "player.set-ability-activation-turn": {
                    playerAbilityActivationTurn = (int)stack.Pop();
                    break;
                }
                case "player.add.set-power-meter":
                case "player.set-power-meter": {
                    playerAbilityMeter = (int)stack.Pop();
                    break;
                }
                case "player.on-additive-load-get-by-index": {
                    break;
                }
                case "player.add.set-side":
                case "player.set-side": {
                    playerSide = (Side)stack.Pop();
                    break;
                }
                case "player.add.set-ui-position":
                case "player.set-ui-position": {
                    playerUiPosition = (Vector2Int)stack.Pop();
                    break;
                }

                case "tile.add": {
                    var position = (Vector2Int)stack.Pop() * transform;
                    var type = (TileType)stack.Pop();
                    Assert.IsTrue(!level.tiles.ContainsKey(position), position.ToString());
                    level.tiles.Add(position, type);
                    break;
                }
                case "tiles.add": {
                    var count = (int)stack.Pop();
                    var positions = new List<Vector2Int>();
                    for (var i = 0; i < count; i++)
                        positions.Add((Vector2Int)stack.Pop());
                    var type = (TileType)stack.Pop();
                    foreach (var position in positions)
                        level.tiles.Add(position, type);
                    break;
                }

                case "building.add": {
                    Assert.AreNotEqual((TileType)0, buildingType);
                    if (buildingPosition is not { } position)
                        throw new AssertionException("buildingPosition is null", buildingType.ToString());
                    Assert.IsTrue(!level.buildings.ContainsKey(position), position.ToString());

                    var player = (Player)stack.Peek();
                    var viewPrefab = BuildingView.GetPrefab(buildingType);

                    var building = new Building {
                        level = level,
                        position = position,
                        type = buildingType,
                        Player = player,
                        Cp = buildingCp,
                        ViewPrefab = viewPrefab,
                        lookDirection = buildingLookDirection,
                        missileSilo = new Building.MissileSiloStats {
                            lastLaunchDay = buildingMissileSiloLastLaunchDay,
                            launchCooldown = buildingMissileSiloLaunchCooldown,
                            ammo = buildingMissileSiloLaunchAmmo,
                            range = buildingMissileSiloRange,
                            blastRange = buildingMissileSiloBlastRange,
                            unitDamage = buildingMissileSiloMissileUnitDamage,
                            bridgeDamage = buildingMissileSiloMissileBridgeDamage
                        }
                    };
                    
                    building.level.buildings.Add(building.position, building);
                    building.Spawn();

                    stack.Push(building);

                    ResetBuildingValues();
                    break;
                }
                case "building.add.set-type":
                case "building.set-type": {
                    buildingType = (TileType)stack.Pop();
                    break;
                }
                case "building.add.set-position":
                case "building.set-position": {
                    buildingPosition = (Vector2Int)stack.Pop() * (transform);
                    break;
                }
                case "building.add.set-cp":
                case "building.set-cp": {
                    buildingCp = (int)stack.Pop();
                    break;
                }
                case "building.add.set-look-direction":
                case "building.set-look-direction": {
                    buildingLookDirection = (Vector2Int)stack.Pop() * transform;
                    break;
                }
                case "building.add.missile-silo.set-last-launch-turn":
                case "building.missile-silo.set-last-launch-turn":
                case "building.missile-silo.set-last-launch-day": {
                    buildingMissileSiloLastLaunchDay = (int)stack.Pop();
                    break;
                }
                case "building.add.missile-silo.set-launch-cooldown":
                case "building.missile-silo.set-launch-cooldown": {
                    buildingMissileSiloLaunchCooldown = (int)stack.Pop();
                    break;
                }
                case "building.add.missile-silo.set-ammo":
                case "building.missile-silo.set-ammo": {
                    buildingMissileSiloLaunchAmmo = (int)stack.Pop();
                    break;
                }
                case "building.add.missile-silo.set-range":
                case "building.missile-silo.set-range": {
                    buildingMissileSiloRange = (Vector2Int)stack.Pop();
                    break;
                }
                case "building.add.missile-silo.set-blast-range":
                case "building.add.missile-silo.missile.set-blast-range":
                case "building.missile-silo.missile.set-blast-range":
                case "building.missile-silo.set-blast-range": {
                    buildingMissileSiloBlastRange = (Vector2Int)stack.Pop();
                    break;
                }
                case "building.add.missile-silo.set-unit-damage":
                case "building.add.missile-silo.missile.set-unit-damage":
                case "building.missile-silo.missile.set-unit-damage":
                case "building.missile-silo.set-unit-damage": {
                    buildingMissileSiloMissileUnitDamage = (int)stack.Pop();
                    break;
                }
                case "building.add.missile-silo.set-bridge-damage":
                case "building.add.missile-silo.missile.set-bridge-damage":
                case "building.missile-silo.missile.set-bridge-damage":
                case "building.missile-silo.set-bridge-damage": {
                    buildingMissileSiloMissileBridgeDamage = (int)stack.Pop();
                    break;
                }

                case "unit.add": {
                    if (unitType is not { } type)
                        throw new AssertionException("unitType == null", "");
                    var player = (Player)stack.Peek();
                    var unit = new Unit {
                        Player = player,
                        type = type,
                        Position = unitPosition,
                        lookDirection = unitLookDirection,
                        Hp = unitHp,
                        Fuel = unitFuel,
                        Moved = unitMoved,
                        ViewPrefab = unitViewPrefab
                    };
                    if (unit.Position is { } actualPosition)
                        unit.Player.level.units[actualPosition] = unit;
                    unit.Spawn();
                    stack.Push(unit);
                    ResetUnitValues();
                    break;
                }
                case "unit.get-player": {
                    stack.Push(((Unit)stack.Pop()).Player);
                    break;
                }
                case "unit.add.set-position":
                case "unit.set-position": {
                    unitPosition = (Vector2Int)stack.Pop() * transform;
                    break;
                }
                case "unit.add.set-moved":
                case "unit.set-moved": {
                    unitMoved = (bool)stack.Pop();
                    break;
                }
                case "unit.add.set-type":
                case "unit.set-type": {
                    unitType = (UnitType)stack.Pop();
                    break;
                }
                case "unit.add.set-look-direction":
                case "unit.set-look-direction": {
                    //unitLookDirection = main.stack.Pop<Vector2Int>() * transform;
                    Vector2Int temp = (Vector2Int)stack.Pop();
                    break;
                }
                case "unit.add.set-hp":
                case "unit.set-hp": {
                    unitHp = (int)stack.Pop();
                    break;
                }
                case "unit.add.set-fuel":
                case "unit.set-fuel": {
                    unitFuel = (int)stack.Pop();
                    break;
                }
                case "unit.add.set-view-prefab":
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
                    var found = zones.TryGetValue(zoneName, out var zone);
                    if (found)
                        unit.brain.assignedZone = zone;
                    else
                        Debug.LogWarning($"could not assign zone {zoneName} to unit {unit}");
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
                case "bridge.add.set-view":
                case "bridge.set-view":
                    bridgeView = (BridgeView)(object)stack.Pop();
                    break;
                case "bridge.add.add-position":
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
                    //cameraRig.SetDistance((dynamic)stack.Pop(), false);
                    break;
                case "camera-rig.set-pitch-angle":
                    cameraRig.PitchAngle = (dynamic)stack.Pop();
                    break;
                case "camera-rig.set-dolly-zoom":
                    cameraRig.DollyZoom = (dynamic)stack.Pop();
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
                case "zone.add-positions": {
                    var count = (int)stack.Pop();
                    var positions = new List<Vector2Int>();
                    for (var i = 0; i < count; i++)
                        positions.Add((Vector2Int)stack.Pop());
                    var zone = (Zone)stack.Peek();
                    zone.tiles.UnionWith(positions);
                    break;
                }
                case "zone.add-distance": {
                    var distance = (int)stack.Pop();
                    var position = (Vector2Int)stack.Pop();
                    var moveType = (MoveType)stack.Pop();
                    var zone = (Zone)stack.Peek();
                    zone.distances.Add((moveType, position), distance);
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

                case "path.set-name": {
                    pathName = (string)stack.Pop();
                    break;
                }
                case "path.add-position": {
                    var position = (Vector2Int)stack.Pop();
                    pathPositions.Add(position);
                    break;
                }
                case "path.add": {
                    Assert.IsNotNull(pathName);
                    var path = new Level.Path { name = pathName };
                    foreach (var position in pathPositions)
                        path.list.AddLast(position);
                    level.paths.Add(path);
                    ResetPathValues();
                    break;
                }

                default:
                    stack.ExecuteToken(token);
                    break;
            }
        }

        Assert.IsTrue(level.localPlayer != null);
    }
}