using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public static class GameSaver {

    public static readonly string[] playerLookupIds = { "A", "B", "C", "D" };
    public static readonly Dictionary<TileType, string> getMnemonic;

    static GameSaver() {
        getMnemonic = GameLoader.parseMnemonic.ToDictionary(pair => pair.Value,pair=>pair.Key);
    }

    public static void Save(TextWriter tw, Main main) {
        
        void Line(object left = null, object right = null) {
            if (left == null && right == null) {
                tw.WriteLine();
                return;
            }
            if (right == null) {
                right = left;
                left = "";
            }
            tw.WriteLine($"{left,-32} {right}");
        }

        void WriteUnit(Unit unit) {

            Line($"{unit.type} UnitType type enum", "unit.set-type");
            if (unit.position.v is { } position)
                Line($"{position.x} {position.y} int2", "unit.set-position");
            if (unit.view)
                Line($"{unit.view.LookDirection.x} {unit.view.LookDirection.y} int2", "unit.set-look-direction");
            Line("unit.create");

            if (unit.cargo.Count != 0) {
                Line();
                foreach (var cargo in unit.cargo) {
                    Line("dup");
                    Line("unit.get-player");
                    WriteUnit(cargo);
                    Line("unit.put-into");
                    Line();
                }
            }
        }

        Line(SceneManager.GetActiveScene().name, "game.load-scene");
        Line($"{main.missionName} MissionName type enum", "game.set-mission-name");
        if (main.turn != null)
            Line(main.turn, "game.set-turn");

        Line("\n\n");

        var nextLookupIdIndex = 0;
        var getLookupId = new Dictionary<Player, string>();
        foreach (var player in main.players) {
            Assert.IsTrue(nextLookupIdIndex < playerLookupIds.Length);
            var lookupId = playerLookupIds[nextLookupIdIndex++];
            getLookupId.Add(player,lookupId);

            Line(lookupId, "player.set-lookup-id");
            Line($"{player.color.r} {player.color.g} {player.color.b}", "player.set-color");
            Line($"{player.team} Team type enum", "player.set-team");
            Line(player.credits, "player.set-credits");
            Line(player.powerMeter, "player.set-power-meter");
            if (player.co)
                Line(player.co.name, "player.set-co");
            if (main.localPlayer == player)
                Line("player.mark-as-local");
            if (player.abilityActivationTurn!=null)
                Line(player.abilityActivationTurn, "player.set-ability-activation-turn");
            Line("player.create");
            Line();

            foreach (var unit in main.FindUnitsOf(player)) {
                Line("dup");
                WriteUnit(unit);
                Line("pop");
                Line();
            }

            Line("\n");
        }

        if (main.tiles.Count > 0) {

            var minX = main.tiles.Keys.Min(p => p.x);
            var maxY = main.tiles.Keys.Max(p => p.y);
            var startPosition = new Vector2Int(minX, maxY);

            var firstLine = main.tiles.Keys.Where(p => p.y == maxY).OrderBy(p => p.x).ToArray();
            Assert.AreEqual(startPosition, firstLine[0]);
            for (var i = 1; i < firstLine.Length; i++)
                Assert.AreEqual(1, firstLine[i].x - firstLine[i - 1].x);

            var firstColumn = main.tiles.Keys.Where(p => p.x == minX).OrderByDescending(p => p.y).ToArray();
            Assert.AreEqual(startPosition, firstColumn[0]);
            for (var i = 1; i < firstColumn.Length; i++)
                Assert.AreEqual(1, firstColumn[i - 1].y - firstColumn[i].y);

            Line($"{minX} {maxY} int2", "tilemap.set-start-position");
            Line("0 -1 int2", "tilemap.set-next-line-offset");
            Line();

            var lastY = startPosition.y;
            foreach (var position in main.tiles.Keys.OrderByDescending(p => p.y).ThenBy(p => p.x)) {
                
                if (position.y != lastY) {
                    lastY = position.y;
                    tw.WriteLine("nl\n");
                }

                var tileType = main.tiles[position];
                var foundMnemonic = getMnemonic.TryGetValue(tileType, out var mnemonic);
                Assert.IsTrue(foundMnemonic, tileType.ToString());

                if (TileType.Buildings.HasFlag(tileType)) {
                    var foundBuilding = main.buildings.TryGetValue(position, out var building);
                    Assert.IsTrue(foundBuilding,position.ToString());
                    var lookupId = "n";
                    if (building.player.v != null) {
                        var foundLookupId = getLookupId.TryGetValue(building.player.v, out lookupId);
                        Assert.IsTrue(foundLookupId);
                    }
                    tw.Write($"{lookupId} {building.cp.v} {mnemonic}\t");
                }
                else 
                    tw.Write($"{mnemonic}\t");
            }
        }
    }

    public static string SaveToString(Main main) {
        using var tw = new StringWriter();
        Save(tw, main);
        return tw.ToString();
    }
}