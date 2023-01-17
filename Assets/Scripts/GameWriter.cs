using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameWriter {

    public static TextWriter Write(TextWriter tw, Main main) {

        WriteLine(tw, SceneManager.GetActiveScene().name, "game.load-scene");
        WriteLine(tw, $"{main.missionName} MissionName type enum", "game.set-mission-name");
        WriteLine(tw, main.turn, "game.set-turn");

        WriteLine(tw, "\n\n");

        foreach (var player in main.players) {

            WriteComment(tw);
            WriteComment(tw, $"Player {main.players.IndexOf(player)} {player}");
            WriteComment(tw);
            
            WriteLine(tw);
            
            Write(tw, player);
            WriteLine(tw);

            foreach (var building in player.main.FindBuildingsOf(player)) {
                WriteLine(tw, "dup");
                Write(tw, building);
                WriteLine(tw, "pop");
                WriteLine(tw);
            }

            WriteLine(tw);

            foreach (var unit in player.main.FindUnitsOf(player)) {
                WriteLine(tw, "dup");
                Write(tw, unit);
                WriteLine(tw, "pop");
                WriteLine(tw);
            }

            WriteLine(tw, "pop");
            WriteLine(tw, "\n");
        }

        foreach (var (position, tileType) in main.tiles.OrderBy(kv=>kv.Key.x).ThenBy(kv=>kv.Key.y)) {
            if (TileType.Buildings.HasFlag(tileType))
                continue;
            WriteLine(tw, $"{position.x} {position.y} int2", "tile.set-position");
            WriteLine(tw, $"{tileType} TileType type enum", "tile.set-type");
            WriteLine(tw, "tile.add");
            WriteLine(tw);
        }
        WriteLine(tw);

        foreach (var building in main.buildings.Values.Where(building => building.player.v == null)) {
            WriteLine(tw, "null");
            Write(tw, building);
            WriteLine(tw, "pop");
            WriteLine(tw);
        }
        WriteLine(tw);

        return tw;
    }

    public static TextWriter WriteComment(TextWriter tw, string text="") {
        text = text.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t","");
        tw.WriteLine($"#{text}");
        return tw;
    }

    public static TextWriter Write(TextWriter tw, Vector2Int position, TileType tileType) {
        WriteLine(tw, $"{position.x} {position.y} int2", "tile.set-position");
        WriteLine(tw, $"{tileType} TileType type enum", "tile.set-type");
        WriteLine(tw, "tile.add");
        return tw;
    }

    public static TextWriter Write(TextWriter tw, Building building) {
        WriteLine(tw, $"{building.type} TileType type enum", "building.set-type");
        WriteLine(tw, $"{building.position.x} {building.position.y} int2", "building.set-position");
        WriteLine(tw, building.cp.v, "building.set-cp");
        WriteLine(tw, $"{building.view.LookDirection.x} {building.view.LookDirection.y} int2", "building.set-look-direction");
        WriteLine(tw, "building.add");
        return tw;
    }

    public static TextWriter Write(TextWriter tw, Player player) {

        WriteLine(tw, $"{player.color.r} {player.color.g} {player.color.b}", "player.set-color");
        WriteLine(tw, $"{player.team} Team type enum", "player.set-team");
        WriteLine(tw, player.credits, "player.set-credits");
        WriteLine(tw, player.powerMeter, "player.set-power-meter");
        WriteLine(tw, $"{player.unitLookDirection.x} {player.unitLookDirection.y} int2", "player.set-unit-look-direction");
        if (player.co)
            WriteLine(tw, player.co.name, "player.set-co");
        if (player.main.localPlayer == player)
            WriteLine(tw, "player.mark-as-local");
        if (player.abilityActivationTurn != null)
            WriteLine(tw, player.abilityActivationTurn, "player.set-ability-activation-turn");
        WriteLine(tw, "player.add");

        return tw;
    }

    public static TextWriter Write(TextWriter tw, Unit unit) {

        WriteLine(tw, $"{unit.type} UnitType type enum", "unit.set-type");
        WriteLine(tw, unit.moved.v ? "true" : "false", "unit.set-moved");
        if (unit.position.v is { } position)
            WriteLine(tw, $"{position.x} {position.y} int2", "unit.set-position");
        if (unit.view) {
            WriteLine(tw, $"{unit.view.LookDirection.x} {unit.view.LookDirection.y} int2", "unit.set-look-direction");
            if (unit.view.prefab)
                WriteLine(tw, $"{unit.view.prefab.name} UnitView type load-resource", "unit.set-view-prefab");
        }
        WriteLine(tw, "unit.add");

        if (unit.cargo.Count != 0) {
            WriteLine(tw);
            foreach (var cargo in unit.cargo) {
                WriteLine(tw, "dup");
                WriteLine(tw, "unit.get-player");
                Write(tw, cargo);
                WriteLine(tw, "unit.put-into");
                WriteLine(tw);
            }
        }

        return tw;
    }

    private static TextWriter WriteLine(TextWriter tw, object left = null, object right = null) {
        if (left == null && right == null) {
            tw.WriteLine();
            return tw;
        }
        if (right == null) {
            right = left;
            left = "";
        }
        tw.WriteLine($"{left,-64} {right}");
        return tw;
    }
}