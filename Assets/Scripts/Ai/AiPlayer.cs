using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

public static class AiPlayer {

    /*
     * TODO: keep exising assignments
     * TODO: sort units more properly
     * TODO: do not let units get outside of the assigned zone?
     * TODO: when assigning units, use real distance instead of precalculated one
     * TODO: keep proper distances for each unit and update them when units move
     * TODO: add upper limit G value for pathfinder exactly for the purpose of calculating distances from units cheaper
     */

    public static bool DistributeUnits(Player player, Zone seed = null) {

        seed ??= player.rootZone;

        if (seed == null)
            return false;

        var zones = Zone.GetConnected(seed);
        var tiles = zones.Aggregate(new HashSet<Vector2Int>(), (a, b) => {
            a.UnionWith(b.tiles);
            return a;
        });

        var all = player.level.units.Values.ToHashSet();
        var units = all.Where(unit => unit.Player == player).ToHashSet();
        var enemies = all.Except(units).ToHashSet();
        var unitsCount = zones.ToDictionary(zone => zone, zone => zone == seed ? units.Count : 0f);

        var queue = new SimplePriorityQueue<Zone, float>();
        foreach (var zone in zones)
            queue.Enqueue(zone, -unitsCount[zone] / zone.Area);

        while (queue.TryDequeue(out var zone) && unitsCount[zone] > 0) {
            var enemiesCount = enemies.Count(unit => zone.tiles.Contains(unit.NonNullPosition));
            var required = Mathf.Max(enemiesCount, (float)units.Count * zone.Area / tiles.Count);
            var input = unitsCount[zone];
            var excess = input - required;
            if (excess > 0) {
                var neighbors = zone.neighbors.Where(neighbor => queue.Contains(neighbor)).ToList();
                if (neighbors.Count > 0) {
                    var neighborsArea = neighbors.Sum(n => n.Area);
                    foreach (var neighbor in zone.neighbors.Where(neighbor => queue.Contains(neighbor))) {
                        unitsCount[neighbor] += excess * neighbor.Area / neighborsArea;
                        queue.UpdatePriority(neighbor, -unitsCount[neighbor] / neighbor.Area);
                    }
                    unitsCount[zone] = required;
                }
            }
        }

        var roundedUnitsCount = unitsCount.ToDictionary(pair => pair.Key, pair => Mathf.RoundToInt(pair.Value));
        var assignmentsToSkip = new HashSet<(Unit, Zone)>();

        while (units.Count > 0 && zones.Count > 0) {
        
            zones.RemoveWhere(zone => roundedUnitsCount[zone] <= 0);
            (Unit unit, Zone zone, int length)? best = null;
        
            foreach (var unit in units)
            foreach (var zone in zones) {
                if (assignmentsToSkip.Contains((unit, zone)))
                    continue;
                if (zone.distances.TryGetValue((Rules.GetMoveType(unit), unit.NonNullPosition), out var length) &&
                    (best is not { } previous || length < previous.length))
                    best = (unit, zone, length);
            }
        
            if (best is not { } found)
                break;

            if (found.unit.brain.states.Count == 0)
                found.unit.brain.states.Push(new StayingInZoneUnitBrainState(found.unit.brain));

            if (found.unit.brain.assignedZone != found.zone) {
                Assert.IsTrue(found.unit.brain.states.TryPeek(out var state));
                if (!state.TryAssignZone(found.zone)) {
                    assignmentsToSkip.Add((found.unit, found.zone));
                    continue;
                }
            }

            units.Remove(found.unit);
            roundedUnitsCount[found.zone]--;
        }

        return true;
    }
    
    [Command]
    public static void DistributeUnits() {
        foreach (var player in Game.Instance.Level.players)
            DistributeUnits(player);
    }
}