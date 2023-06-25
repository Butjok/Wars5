using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public static class LevelWriter {

    public static TextWriter WriteLevel(this TextWriter tw, Level level, Predicate<Vector2Int> filter = null) {
        tw
            .WriteCommand("game.load-scene", SceneManager.GetActiveScene().name)
            .WriteCommand("game.set-mission-name", level.missionName)
            .WriteCommand("game.set-turn", level.turn);

        foreach (var player in level.players) {
            tw
                .WritePlayer(player);

            if (player.rootZone != null) {
                foreach (var zone in Zone.GetConnected(player.rootZone)) {
                    tw.WriteCommand("zone.add", zone.name, zone == player.rootZone);
                    foreach (var position in zone.tiles)
                        tw.WriteCommand("zone.add-position", position);
                    if (zone.distances != null)
                        foreach (var ((moveType, position), distance) in zone.distances)
                            tw.WriteCommand("zone.add-distance", moveType, position, distance);
                    tw.WriteCommand("pop");
                }
                foreach (var zone in Zone.GetConnected(player.rootZone))
                foreach (var neighbor in zone.neighbors)
                    tw.WriteCommand("zone.connect", zone.name, neighbor.name);
            }

            foreach (var building in player.level.FindBuildingsOf(player))
                tw
                    .WriteCommand("dup")
                    .WriteBuilding(building)
                    .WriteCommand("pop");

            foreach (var unit in player.level.FindUnitsOf(player))
                tw
                    .WriteCommand("dup")
                    .WriteUnit(unit)
                    .WriteCommand("pop");

            tw.WriteCommand("pop");
        }

        var tiles = level.tiles
            .Union(level.bridges.SelectMany(bridge => bridge.tiles));

        foreach (var (position, tileType) in tiles.OrderBy(kv => kv.Value).ThenBy(kv => kv.Key.x).ThenBy(kv => kv.Key.y)) {
            if (TileType.Buildings.HasFlag(tileType))
                continue;
            tw.WriteCommand("tile.add", tileType, position);
        }

        foreach (var building in level.buildings.Values.Where(building => building.Player == null))
            tw
                .WriteCommand("null")
                .WriteBuilding(building)
                .WriteCommand("pop");

        foreach (var (trigger, positions) in level.triggers) {
            tw.WriteCommand("trigger.select", trigger);
            foreach (var position in positions)
                tw.WriteCommand("trigger.add-position", position);
        }

        foreach (var bridge in level.bridges)
            tw
                .WriteBridge(bridge)
                .WriteCommand("pop");

        if (level.view.cameraRig)
            tw.WriteCameraRig(level.view.cameraRig);

        return tw;
    }

    public static TextWriter WriteCameraRig(this TextWriter tw, CameraRig cameraRig) {
        return tw
            .WriteCommand("camera-rig.set-position", cameraRig.transform.position)
            .WriteCommand("camera-rig.set-rotation", cameraRig.transform.rotation.eulerAngles.y)
            .WriteCommand("camera-rig.set-distance", cameraRig.Distance)
            .WriteCommand("camera-rig.set-pitch-angle", cameraRig.PitchAngle);
    }

    public static TextWriter WriteBridge(this TextWriter tw, Bridge bridge) {

        tw
            .WriteCommand("find-in-level-with-name", bridge.view.name)
            .WriteCommand("get-component", typeof(BridgeView))
            .WriteCommand("bridge.set-view")
            .WriteCommand("bridge.set-hp", bridge.Hp);

        foreach (var position in bridge.tiles.Keys)
            tw.WriteCommand("bridge.add-position", position);

        return tw.WriteCommand("bridge.add");
    }

    public static TextWriter WriteBuilding(this TextWriter tw, Building building) {
        tw
            .WriteCommand("building.set-type", building.type)
            .WriteCommand("building.set-position", building.position)
            .WriteCommand("building.set-cp", building.Cp)
            .WriteCommand("building.set-look-direction", building.view.LookDirection);

        if (building.type == TileType.MissileSilo)
            tw
                .WriteCommand("building.missile-silo.set-last-launch-turn", building.missileSiloLastLaunchTurn)
                .WriteCommand("building.missile-silo.set-launch-cooldown", building.missileSiloLaunchCooldown)
                .WriteCommand("building.missile-silo.set-ammo", building.missileSiloAmmo)
                .WriteCommand("building.missile-silo.set-range", building.missileSiloRange)
                .WriteCommand("building.missile-silo.missile.set-blast-range", building.missileBlastRange)
                .WriteCommand("building.missile-silo.missile.set-unit-damage", building.missileUnitDamage)
                .WriteCommand("building.missile-silo.missile.set-bridge-damage", building.missileBridgeDamage);

        return tw.WriteCommand("building.add");
    }

    public static TextWriter WritePlayer(this TextWriter tw, Player player) {
        tw
            .WriteCommand("player.set-color-name", player.ColorName)
            .WriteCommand("player.set-team", player.team)
            .WriteCommand("player.set-co-name", player.coName)
            .WriteCommand("player.set-ui-position", player.uiPosition)
            .WriteCommand("player.set-credits", player.Credits)
            .WriteCommand("player.set-power-meter", player.AbilityMeter)
            .WriteCommand("player.set-unit-look-direction", player.unitLookDirection)
            .WriteCommand("player.set-side", player.side);

        if (player.level.localPlayer == player)
            tw.WriteCommand("player.mark-as-local");

        if (player.abilityActivationTurn != null)
            tw.WriteCommand("player.set-ability-activation-turn", player.abilityActivationTurn);

        return tw.WriteCommand("player.add");
    }

    public static TextWriter WriteUnit(this TextWriter tw, Unit unit) {
        tw
            .WriteCommand("unit.set-type", unit.type)
            .WriteCommand("unit.set-moved", unit.Moved)
            .WriteCommand("unit.set-hp", unit.Hp);

        if (unit.Position is { } position)
            tw.WriteCommand("unit.set-position", position);

        if (unit.view) {
            tw.WriteCommand("unit.set-look-direction", unit.view.LookDirection);
            if (unit.view.prefab)
                tw
                    .WriteCommand("load-resource", unit.view.prefab.name, typeof(UnitView))
                    .WriteCommand("unit.set-view-prefab");
        }

        tw.WriteCommand("unit.add");

        if (unit.brain.assignedZone!=null)
            tw.WriteCommand("unit.brain.set-assigned-zone", unit.brain.assignedZone.name);
        foreach (var state in unit.brain.states.Reverse()) {
            tw.WriteCommand("unit.brain.add-state", state.GetType());
            switch (state) {
                case StayingInZoneUnitBrainState stayingInZone:
                    break;
                case ReturningToZoneUnitBrainState returningToZone:
                    break;
                case AttackingAnEnemyUnitBrainState attackingAnEnemy:
                    if (attackingAnEnemy.target != null)
                        tw.WriteCommand("unit.brain.state.attacking-an-enemy.set-target-position", attackingAnEnemy.target.NonNullPosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(state.ToString());
            }
            tw.WriteCommand("pop");
        }

        if (unit.Cargo.Count > 0)
            foreach (var cargo in unit.Cargo)
                tw
                    .WriteCommand("dup")
                    .WriteCommand("dup")
                    .WriteCommand("unit.get-player")
                    .WriteUnit(cargo)
                    .WriteCommand("unit.put-into");

        return tw;
    }

    public static TextWriter WriteCommand(this TextWriter tw, string command, params object[] arguments) {
        if (command != null) {
            Assert.AreNotEqual(0, command.Length);
            Assert.IsTrue(char.IsLower(command[0]));
        }
        var left = string.Join(" ", arguments.Select(PostfixInterpreter.Format));
        var right = command ?? "";
        tw.WriteLine($"{left,-64} {right}");
        return tw;
    }
}