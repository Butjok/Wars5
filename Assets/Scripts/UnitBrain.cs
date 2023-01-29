using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;

public static class UnitBrain {

    public struct PotentialAttack {
        public Vector2Int destination;
        public Unit target;
        public WeaponName weaponName;
        public int damage;
    }

    public static IEnumerable<PotentialAttack> GetPotentialImmediateArtilleryAttacks(Unit artillery) {
        Assert.IsTrue(IsArtillery(artillery));

        var hasAttackRange = TryGetAttackRange(artillery, out var attackRange);
        Assert.IsTrue(hasAttackRange);
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
        Assert.IsTrue(hasAttackRange);
        if (unit.Position is not { } position)
            throw new AssertionException("unit.Position == null", null);

        var main = unit.Player.main;
        main.traverser.Traverse(main.tiles.Keys, position, GetMoveCostFunction(unit));
        var targetPositions = new HashSet<Vector2Int>();
        var destinations = new Dictionary<Vector2Int, Vector2Int>();
        foreach (var reachablePosition in main.traverser.Reachable)
        foreach (var targetPosition in main.PositionsInRange(reachablePosition, attackRange)) {
            targetPositions.Add(targetPosition);
            destinations[targetPosition] = reachablePosition;
        }

        foreach (var targetPosition in targetPositions) {
            if (unit.Player.main.TryGetUnit(targetPosition, out var target))
                foreach (var weaponName in GetWeaponNames(unit))
                    if (TryGetDamage(unit, target, weaponName, out var damage))
                        yield return new PotentialAttack {
                            destination = destinations[(Vector2Int)target.Position],
                            target = target,
                            weaponName = weaponName,
                            damage = damage
                        };
        }
    }

    public static IEnumerable<PotentialAttack> GetPotentialImmediateAttacks(Unit unit) {
        return IsArtillery(unit) ? GetPotentialImmediateArtilleryAttacks(unit) : GetPotentialImmediateNonArtilleryAttacks(unit);
    }

    [Command]
    public static void DrawPotentialImmediateAttacks(float duration) {
        var main = Object.FindObjectOfType<Main2>();
        if (main && Mouse.TryGetPosition(out Vector2Int mousePosition) && main.TryGetUnit(mousePosition, out var unit)) {
            foreach (var group in GetPotentialImmediateAttacks(unit).GroupBy(pa => pa.target)) {
                using (Draw.ingame.WithDuration(duration))
                using (Draw.ingame.WithLineWidth(2))
                using (Draw.ingame.WithColor(Color.red)) {
                    var text = string.Join("\n", @group.Select(e => $"{e.weaponName}: {e.damage}"));
                    var position = (Vector3)((Vector2Int)@group.Key.Position).ToVector3Int();
                    Draw.ingame.Label2D(position, text, 14, LabelAlignment.Center);
                    Draw.ingame.CircleXZ(position, .4f);
                }
            }
        }
    }
}