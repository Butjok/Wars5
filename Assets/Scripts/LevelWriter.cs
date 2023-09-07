using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelWriter {

    public static PrefixWriter Level(this PrefixWriter writer, Level level, Predicate<Vector2Int> filter = null) {

        writer
            .PushPrefix("game")
            .Command(".load-scene", SceneManager.GetActiveScene().name)
            .Command(":turn", level.turn)
            .PopPrefix()
            .BlankLine();

        foreach (var player in level.players) {

            writer
                .Comment(player.ToString())
                .Player(player)
                .BlankLine();

            if (player.rootZone != null) {
                writer.PushPrefix("zone");
                foreach (var zone in Zone.GetConnected(player.rootZone)) {
                    writer
                        .Command(".add", zone.name, zone == player.rootZone)
                        .BeginCommand(".add-positions").InlineBlock();
                    foreach (var position in zone.tiles)
                        writer.Value(position);
                    writer
                        .Value(zone.tiles.Count)
                        .End()
                        .Command("pop");
                    // if (zone.distances != null)
                    //     foreach (var ((moveType, position), distance) in zone.distances)
                    //         writer.Command(".add-distance", moveType, position, distance);
                }
                foreach (var zone in Zone.GetConnected(player.rootZone))
                foreach (var neighbor in zone.neighbors)
                    writer.Command(".connect", zone.name, neighbor.name);
                writer
                    .PopPrefix()
                    .BlankLine();
            }

            foreach (var building in player.level.FindBuildingsOf(player))
                writer
                    .Comment(building.ToString())
                    .Building(building)
                    .Command("pop")
                    .BlankLine();

            foreach (var unit in player.level.FindUnitsOf(player))
                writer
                    .Comment(unit.ToString())
                    .Unit(unit)
                    .Command("pop")
                    .BlankLine();

            writer
                .Comment(player.ToString())
                .Command("pop")
                .BlankLine();
        }

        var tiles = level.tiles
            .Union(level.bridges.SelectMany(bridge => bridge.tiles));

        foreach (var group in tiles.GroupBy(kv => kv.Value)) {
            if (TileType.Buildings.HasFlag(group.Key))
                continue;
            writer
                .BeginCommand("tiles.add")
                .InlineBlock()
                .Value(group.Key);
            foreach (var (position, _) in group)
                writer.Value(position);
            writer
                .Value(group.Count())
                .End();
        }

        writer
            .BlankLine()
            .Comment("Unowned buildings")
            .Value(null);
        foreach (var building in level.buildings.Values.Where(building => building.Player == null))
            writer
                .BlankLine()
                .Comment(building.ToString())
                .Building(building)
                .Command("pop");
        writer
            .Comment("null player")
            .Command("pop")
            .BlankLine();

        foreach (var (trigger, positions) in level.triggers) {
            writer
                .PushPrefix("trigger")
                .Command(".select", trigger);
            foreach (var position in positions)
                writer.Command(".add-position", position);
            writer
                .PopPrefix()
                .BlankLine();
        }

        foreach (var bridge in level.bridges) {
            writer
                .Bridge(bridge)
                .Command("pop")
                .BlankLine();
        }

        if (level.view.cameraRig)
            writer.CameraRig(level.view.cameraRig);

        return writer;
    }

    public static PrefixWriter CameraRig(this PrefixWriter writer, CameraRig cameraRig) {
        writer
            .PushPrefix("camera-rig")
            .Command(":position", cameraRig.transform.position)
            .Command(":rotation", cameraRig.transform.rotation.eulerAngles.y)
            .Command(":distance", cameraRig.Distance)
            .Command(":pitch-angle", cameraRig.PitchAngle)
            .PopPrefix();
        return writer;
    }

    public static PrefixWriter Bridge(this PrefixWriter writer, Bridge bridge) {
        writer
            .PushPrefix("bridge.add")
            .Command(":hp", bridge.Hp)
            .BeginCommand(":view").InlineBlock()
            .BeginCommand("get-component").InlineBlock()
            .Command("find-in-level-with-name", bridge.view.name)
            .End()
            .End();

        foreach (var position in bridge.tiles.Keys)
            writer.Command(".add-position", position);

        writer.PopPrefix();
        writer.Command("bridge.add");
        return writer;
    }

    public static PrefixWriter Building(this PrefixWriter writer, Building building) {
        writer
            .PushPrefix("building.add")
            .Command(":type", building.type)
            .Command(":position", building.position)
            .Command(":cp", building.Cp)
            .Command(":look-direction", building.view.LookDirection)
            .PopPrefix();

        if (building.type == TileType.MissileSilo)
            writer
                .PushPrefix("building.add.missile-silo")
                .Command(":last-launch-turn", building.missileSiloLastLaunchTurn)
                .Command(":launch-cooldown", building.missileSiloLaunchCooldown)
                .Command(":ammo", building.missileSiloAmmo)
                .Command(":range", building.missileSiloRange)
                .PopPrefix()
                .PushPrefix("building.add.missile-silo.missile")
                .Command(":blast-range", building.missileBlastRange)
                .Command(":unit-damage", building.missileUnitDamage)
                .Command(":bridge-damage", building.missileBridgeDamage)
                .PopPrefix();

        writer.Command("building.add");
        return writer;
    }

    public static PrefixWriter Player(this PrefixWriter writer, Player player) {
        writer
            .PushPrefix("player.add")
            .Command(":color-name", player.ColorName)
            .Command(":team", player.team)
            .Command(":co-name", player.coName)
            .Command(":ui-position", player.uiPosition)
            .Command(":credits", player.Credits)
            .Command(":power-meter", player.AbilityMeter)
            .Command(":unit-look-direction", player.unitLookDirection)
            .Command(":side", player.side);

        if (player.level.localPlayer == player)
            writer.Command(".mark-as-local");

        if (player.abilityActivationTurn != null)
            writer.Command(":ability-activation-turn", player.abilityActivationTurn);

        writer.PopPrefix();
        writer.Command("player.add");
        return writer;
    }

    public static PrefixWriter Unit(this PrefixWriter writer, Unit unit) {
        {
            writer
                .PushPrefix("unit.add")
                .Command(":type", unit.type)
                .Command(":moved", unit.Moved)
                .Command(":hp", unit.Hp);

            if (unit.Position is { } position)
                writer.Command(":position", position);

            if (unit.view) {
                writer.Command(":look-direction", unit.view.LookDirection);
                if (unit.view.prefab)
                    writer.BeginCommand(":view-prefab").InlineBlock()
                        .Command("load-resource", "UnitView", unit.view.prefab.name)
                        .End();
            }

            writer.PopPrefix();
            writer.Command("unit.add");
        }

        {
            writer.PushPrefix("unit.brain");

            if (unit.brain.assignedZone != null)
                writer.Command(":assigned-zone", unit.brain.assignedZone.name);

            foreach (var state in unit.brain.states.Reverse()) {
                writer.Command(".add-state", state.GetType());
                switch (state) {
                    case StayingInZoneUnitBrainState stayingInZone:
                        break;
                    case ReturningToZoneUnitBrainState returningToZone:
                        break;
                    case AttackingAnEnemyUnitBrainState attackingAnEnemy:
                        if (attackingAnEnemy.target != null)
                            writer.Command(".state.attacking-an-enemy:target", attackingAnEnemy.target.NonNullPosition);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(state.ToString());
                }
                writer.Command("pop");
            }

            writer.PopPrefix();
        }

        if (unit.Cargo.Count > 0)
            foreach (var cargo in unit.Cargo) {
                writer
                    .Command("dup")
                    .Command("dup")
                    .Command("unit.get-player");
                writer.Unit(cargo);
                writer.Command("unit.put-into");
            }

        return writer;
    }
}