using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;

public static class UnitBrain {

    public abstract class PotentialAction {
        public Unit unit;
        public Vector2Int destination;
    }
    public class PotentialAttack : PotentialAction {
        public Unit target;
        public WeaponName weaponName;
        public int damage;
    }
    public class PotentialCapture : PotentialAction {
        public Building building;
        public int newCp;
    }

    public static IEnumerable<PotentialAttack> GetPotentialImmediateArtilleryAttacks(Unit artillery) {
        Assert.IsTrue(IsArtillery(artillery));

        var hasAttackRange = TryGetAttackRange(artillery, out var attackRange);
        if (!hasAttackRange)
            yield break;
        if (artillery.Position is not { } position)
            throw new AssertionException("artillery.Position == null", null);

        foreach (var targetPosition in artillery.Player.main.PositionsInRange(position, attackRange)) {
            if (artillery.Player.main.TryGetUnit(targetPosition, out var target))
                foreach (var weaponName in GetWeaponNames(artillery))
                    if (TryGetDamage(artillery, target, weaponName, out var damage))
                        yield return new PotentialAttack {
                            destination = position,
                            target = target,
                            weaponName = weaponName,
                            damage = damage
                        };
        }
    }

    public static IEnumerable<PotentialAttack> GetPotentialImmediateNonArtilleryAttacks(Unit unit) {
        Assert.IsFalse(IsArtillery(unit));

        var hasAttackRange = TryGetAttackRange(unit, out var attackRange);
        if (!hasAttackRange)
            yield break;
        if (unit.Position is not { } position)
            throw new AssertionException("unit.Position == null", null);

        var main = unit.Player.main;
        main.traverser.Traverse(main.tiles.Keys, position, GetMoveCostFunction(unit));

        foreach (var reachablePosition in main.traverser.Reachable) {
            if (!CanStay(unit, reachablePosition))
                continue;

            foreach (var targetPosition in main.PositionsInRange(reachablePosition, attackRange)) {
                if (unit.Player.main.TryGetUnit(targetPosition, out var target))
                    foreach (var weaponName in GetWeaponNames(unit))
                        if (TryGetDamage(unit, target, weaponName, out var damage))
                            yield return new PotentialAttack {
                                destination = reachablePosition,
                                target = target,
                                weaponName = weaponName,
                                damage = damage
                            };
            }
        }
    }

    public static IEnumerable<PotentialAttack> GetPotentialImmediateAttacks(Unit unit) {
        return IsArtillery(unit) ? GetPotentialImmediateArtilleryAttacks(unit) : GetPotentialImmediateNonArtilleryAttacks(unit);
    }

    public static IEnumerable<PotentialCapture> GetPotentialImmediateCaptures(Unit unit) {

        if (unit.Position is not { } position)
            throw new AssertionException("unit.Position == null", null);

        var main = unit.Player.main;
        main.traverser.Traverse(main.tiles.Keys, position, GetMoveCostFunction(unit));

        foreach (var reachablePosition in main.traverser.Reachable) {
            if (!main.TryGetBuilding(reachablePosition, out var building) || !CanCapture(unit, building))
                continue;

            yield return new PotentialCapture {
                destination = reachablePosition,
                building = building,
                newCp = Mathf.Max(0, building.Cp - Cp(unit))
            };
        }
    }

    [Command]
    public static void DrawPotentialImmediateAttacks(float duration) {

        using (Draw.ingame.WithDuration(duration))
        using (Draw.ingame.WithLineWidth(2))
        using (Draw.ingame.WithColor(Color.red)) {

            var traverser = new Traverser();
            var path = new List<Vector2Int>();

            var main = Object.FindObjectOfType<Main2>();
            if (main && Mouse.TryGetPosition(out Vector2Int mousePosition) && main.TryGetUnit(mousePosition, out var unit)) {
                foreach (var group in GetPotentialImmediateAttacks(unit).GroupBy(pa => pa.target)) {

                    var targetPosition3d = (Vector3)((Vector2Int)group.Key.Position).ToVector3Int();
                    var text = string.Join("\n", group.Select(i => $"{i.weaponName}: {i.damage}"));
                    Draw.ingame.Label2D(targetPosition3d, text, 14, LabelAlignment.Center, Color.white);
                    Draw.ingame.CircleXZ(targetPosition3d, .4f);

                    foreach (var potentialAttack in group) {
                        if (traverser.TryFindPath(unit, potentialAttack.destination, path))
                            for (var i = 1; i < path.Count; i++)
                                Draw.ingame.Arrow((Vector3)path[i - 1].ToVector3Int(), (Vector3)path[i].ToVector3Int());
                    }
                }
            }
        }
    }
}