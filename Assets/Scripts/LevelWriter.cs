using System;
using System.IO;
using System.Linq;
using Stable;

public class LevelWriter {

    public TextWriter tw;
    public LevelWriter(TextWriter tw) => this.tw = tw;
    public void Write(string format, params object[] values) => tw.PostfixWrite(format, values);
    public void WriteLine(string format, params object[] values) => Write(format + "\n", values);
    public void WriteLine() => tw.WriteLine();

    public void WriteLevel(Level level) {
        WriteLine("game {{");
        WriteLine("    :turn ( {0} )", level.turn);
        WriteLine("    :level-name ( {0} )", level.name);
        WriteLine("}}");

        foreach (var player in level.players) {
            WriteLine($"// {player}");
            WritePlayer(player);

            if (player.rootZone != null) {
                WriteLine("zone {{");
                foreach (var zone in Zone.GetConnected(player.rootZone)) {
                    WriteLine("    .add ( {0} {1} )", zone.name, zone == player.rootZone);
                    Write("    .add-positions ( ");
                    foreach (var position in zone.tiles)
                        Write("{0} ", position);
                    WriteLine("{0} )", zone.tiles.Count);
                    WriteLine("    pop");
                }

                foreach (var zone in Zone.GetConnected(player.rootZone))
                foreach (var neighbor in zone.neighbors)
                    WriteLine("    .connect ( {0} {1} )", zone.name, neighbor.name);
                WriteLine("}}");
            }

            foreach (var building in player.level.FindBuildingsOf(player)) {
                WriteLine($"// {building}");
                WriteBuilding(building);
                WriteLine("pop");
            }

            foreach (var unit in player.level.FindUnitsOf(player)) {
                WriteLine($"// {unit}");
                WriteUnit(unit);
                WriteLine("pop");
            }
        }

        var tiles = level.tiles
            .Union(level.bridges.SelectMany(bridge => bridge.tiles));

        WriteLine("tiles {{");
        foreach (var group in tiles.GroupBy(kv => kv.Value)) {
            if (TileType.Buildings.HasFlag(group.Key))
                continue;
            Write("    .add ( {0} ", group.Key);
            foreach (var (position, _) in group)
                Write("{0} ", position);
            WriteLine("{0} )", group.Count());
        }

        WriteLine("}}");

        WriteLine("// Unowned buildings");
        WriteLine("null");
        foreach (var building in level.buildings.Values.Where(building => building.Player == null)) {
            WriteLine($"// {building}");
            WriteBuilding(building);
            WriteLine("pop");
        }

        WriteLine("pop");

        foreach (var (trigger, positions) in level.triggers)
            if (positions.Count > 0) {
                WriteLine("trigger {{");
                WriteLine("    .select ( {0} )", trigger);
                foreach (var position in positions)
                    WriteLine("    .add-position ( {0} )", position);
                WriteLine("}}");
            }

        foreach (var bridge in level.bridges)
            WriteBridge(bridge);

        if (level.view.cameraRig)
            WriteCameraRig(level.view.cameraRig);

        foreach (var path in level.paths)
            WritePath(path);
    }

    public void WritePlayer(Player player) {
        WriteLine("player {{");

        WriteLine("    :color-name          ( {0} )", player.ColorName);
        WriteLine("    :team                ( {0} )", player.team);
        WriteLine("    :co-name             ( {0} )", player.coName);
        WriteLine("    :ui-position         ( {0} )", player.uiPosition);
        WriteLine("    :credits             ( {0} )", player.Credits);
        WriteLine("    :power-meter         ( {0} )", player.AbilityMeter);
        WriteLine("    :unit-look-direction ( {0} )", player.unitLookDirection);
        WriteLine("    :side                ( {0} )", player.side);

        if (player.level.localPlayer == player)
            WriteLine("    .mark-as-local");

        if (player.abilityActivationTurn != null)
            WriteLine("    :ability-activation-turn ( {0} )", player.abilityActivationTurn);

        if (player.IsAi)
            WriteLine("    :ai ( {0} )", player.difficulty);

        WriteLine("    .add");
        WriteLine("}}");
    }

    public void WriteBuilding(Building building) {
        WriteLine("building {{");

        WriteLine("    :type           ( {0} )", building.Type);
        WriteLine("    :position       ( {0} )", building.position);
        WriteLine("    :cp             ( {0} )", building.Cp);
        WriteLine("    :look-direction ( {0} )", building.view.LookDirection);

        if (building.Type == TileType.MissileSilo) {
            WriteLine("    .missile-silo {{");
            WriteLine("        :last-launch-day ( {0} )", building.missileSilo.lastLaunchDay);
            WriteLine("        :launch-cooldown  ( {0} )", building.missileSilo.launchCooldown);
            WriteLine("        :ammo             ( {0} )", building.missileSilo.ammo);
            WriteLine("        :range            ( {0} )", building.missileSilo.range);
            WriteLine("        :blast-range      ( {0} )", building.missileSilo.blastRange);
            WriteLine("        :unit-damage      ( {0} )", building.missileSilo.unitDamage);
            WriteLine("        :bridge-damage    ( {0} )", building.missileSilo.bridgeDamage);
            WriteLine("    }}");
        }

        WriteLine("    .add");
        WriteLine("}}");
    }

    public void WriteUnit(Unit unit) {
        WriteLine("unit {{");
        WriteLine("    :type  ( {0} )", unit.type);
        WriteLine("    :moved ( {0} )", unit.Moved);
        WriteLine("    :hp    ( {0} )", unit.Hp);

        if (unit.Position is { } position)
            WriteLine("    :position ( {0} )", position);

        if (unit.view) {
            WriteLine("    :look-direction ( {0} )", unit.view.LookDirection);
            if (unit.view.prefab)
                WriteLine("    :view-prefab ( UnitView {0} load-resource )", unit.view.prefab.name);
        }

        WriteLine("    .add");

        WriteLine("    .brain {{");
        if (unit.brain.assignedZone != null)
            WriteLine("        :assigned-size ( {0} )", unit.brain.assignedZone.name);
        foreach (var state in unit.brain.states.Reverse()) {
            WriteLine("        .add-state ( {0} )", state.GetType());
            switch (state) {
                case StayingInZoneUnitBrainState stayingInZone:
                    break;
                case ReturningToZoneUnitBrainState returningToZone:
                    break;
                case AttackingAnEnemyUnitBrainState attackingAnEnemy:
                    if (attackingAnEnemy.target != null)
                        WriteLine("        .attacking-an-enemy:target ( {0} )", attackingAnEnemy.target.NonNullPosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(state.ToString());
            }

            WriteLine("        pop");
        }

        WriteLine("    }}");

        if (unit.Cargo.Count > 0)
            foreach (var cargo in unit.Cargo) {
                WriteLine("    dup");
                WriteLine("    dup");
                WriteLine("    .get-player");
                WriteUnit(cargo);
                WriteLine("    .put-into");
            }

        WriteLine("}}");
    }

    public void WriteBridge(Bridge bridge) {
        WriteLine("bridge {{");
        WriteLine("    :hp ( {0} )", bridge.Hp);
        WriteLine("    :view ( BridgeView type {0} find-in-level-with-name get-component )", bridge.view.name);
        foreach (var position in bridge.tiles.Keys)
            WriteLine("    .add-position ( {0} )", position);
        WriteLine("    .add");
        WriteLine("}}");
    }

    public void WriteCameraRig(CameraRig cameraRig) {
        WriteLine("camera-rig {{");
        WriteLine("    :position    ( {0} )", cameraRig.transform.position);
        WriteLine("    :rotation    ( {0} )", cameraRig.transform.rotation.eulerAngles.y);
        WriteLine("    :pitch-angle ( {0} )", cameraRig.PitchAngle);
        WriteLine("    :dolly-zoom  ( {0} )", cameraRig.DollyZoom);
        WriteLine("}}");
    }

    public void WritePath(Level.Path path) {
        WriteLine("path {{");
        WriteLine("    :name ( {0} )", path.name);
        foreach (var node in path.list)
            WriteLine("    .add-position ( {0} )", node);
        WriteLine("    .add");
        WriteLine("}}");
    }
}