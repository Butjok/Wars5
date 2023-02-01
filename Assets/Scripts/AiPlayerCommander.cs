using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using static Rules;

public class AiPlayerCommander : MonoBehaviour {

    public Main2 main;
    public Vector2Int selectPosition;
    public Vector2Int movePosition;

    [Command]
    public bool playForHuman = true;
    [Command]
    public bool waitForKeyPress = true;
    [Command]
    public KeyCode nextStepKeyCode = KeyCode.RightArrow;

    [Command]
    public void StartPlaying() {
        StartCoroutine(Loop());
    }
    [Command]
    public void StopPlaying() {
        StopAllCoroutines();
    }

    public IEnumerator Loop() {
        while (true) {
            yield return null;

            if (!playForHuman && main.CurrentPlayer.type != PlayerType.Ai)
                continue;

            while (!main.IsInState(nameof(SelectionState)) || !main.IsReadyForInput())
                yield return null;
            while (waitForKeyPress && !Input.GetKeyDown(nextStepKeyCode))
                yield return null;

            main.commands.Enqueue($"{selectPosition.x} {selectPosition.y} int2 {SelectionState.@select}");

            while (!main.IsInState(nameof(PathSelectionState)) || !main.IsReadyForInput())
                yield return null;
            while (waitForKeyPress && !Input.GetKeyDown(nextStepKeyCode))
                yield return null;

            main.commands.Enqueue($"{movePosition.x} {movePosition.y} int2 {PathSelectionState.reconstructPath}");
            main.commands.Enqueue($"false {PathSelectionState.move}");

            while (!main.IsInState(nameof(ActionSelectionState)) || !main.IsReadyForInput())
                yield return null;
            while (waitForKeyPress && !Input.GetKeyDown(nextStepKeyCode))
                yield return null;

            var actions = main.stack.Peek<List<UnitAction>>();
            actions.RemoveAll(a => a.type != UnitActionType.Stay);

            main.commands.Enqueue($"{ActionSelectionState.execute}");
        }
    }

    [Command]
    public void DrawPotentialUnitActions() {
        StartCoroutine(DrawPotentialUnitActionsCoroutine());
    }

    [Command]
    public Color colorStay = Color.blue;
    [Command]
    public Color colorJoin = Color.green;
    [Command]
    public Color colorCapture = Color.cyan;
    [Command]
    public Color colorAttack = Color.red;
    [Command]
    public Color colorGetIn = Color.yellow;
    [Command]
    public Color colorDrop = Color.yellow;
    [Command]
    public Color colorSupply = Color.yellow;
    [Command]
    public Color colorLaunchMissile = Color.red;
    [Command]
    public float thickness = 2;
    [Command]
    public float arrowHeadSize = .1f;
    [Command]
    public float textSize = 14;

    public Color GetUnitActionColor(UnitAction action) {
        return action.type switch {
            UnitActionType.Stay => colorStay,
            UnitActionType.Join => colorJoin,
            UnitActionType.Capture => colorCapture,
            UnitActionType.Attack => colorAttack,
            UnitActionType.GetIn => colorGetIn,
            UnitActionType.Drop => colorDrop,
            UnitActionType.Supply => colorSupply,
            UnitActionType.LaunchMissile => colorLaunchMissile,
            _ => throw new ArgumentOutOfRangeException(action.type.ToString())
        };
    }

    [Command]
    public int maxPathCost = 999;

    public IEnumerator DrawPotentialUnitActionsCoroutine() {

        if (!Mouse.TryGetPosition(out Vector2Int position) || !main.TryGetUnit(position, out var unit))
            yield break;

        foreach (var action in PotentialUnitActions(unit).ToArray()) {
            while (!Input.GetKeyDown(KeyCode.Alpha0)) {
                yield return null;

                using (Draw.ingame.WithLineWidth(thickness))
                using (Draw.ingame.WithColor(GetUnitActionColor(action))) {
                    Draw.ingame.Label2D((Vector3)action.destination.ToVector3Int(), action.ToString(), textSize, LabelAlignment.Center);
                    var subPathLength = Traverser.GetSubPathLength(action.Path, unit, out var pathCost, cost => cost <= maxPathCost);
                    for (var i = 1; i < subPathLength; i++)
                        Draw.ingame.Arrow((Vector3)action.Path[i - 1].ToVector3Int(), (Vector3)action.Path[i].ToVector3Int(), Vector3.up, arrowHeadSize);
                }
            }

            yield return null;
            action.Dispose();
            
            if(Input.GetKey(KeyCode.RightShift))
                yield break;
        }
    }

    public IEnumerable<UnitAction> PotentialUnitActions(Unit unit) {

        var traverser = new Traverser();
        List<Vector2Int> path = null;

        // find buildings to capture 
        {
            var buildingToCapture = main.buildings.Values.Where(building => CanCapture(unit, building));
            foreach (var building in buildingToCapture) {
                if (traverser.TryFindPath(unit, building.position, ref path))
                    yield return new UnitAction(UnitActionType.Capture, unit, path: path, targetBuilding: building);
            }
        }

        // find missile silos to use
        {
            var missileSilos = main.buildings.Values.Where(building => building.type == TileType.MissileSilo && CanLaunchMissile(unit, building));
            foreach (var missileSilo in missileSilos) {
                if (traverser.TryFindPath(unit, missileSilo.position, ref path))
                    // WARNING: no launch target is specified here yet!
                    yield return new UnitAction(UnitActionType.LaunchMissile, unit, path: path, targetBuilding: missileSilo);
            }
        }

        // find buildings to capture by dropping infantry
        {
            if (unit.Cargo.Count > 0) {
                var cargo = unit.Cargo[0];
                var buildingToCapture = main.buildings.Values.Where(building => CanCapture(cargo, building));
                foreach (var building in buildingToCapture) {
                    foreach (var destination in main.PositionsInRange(building.position, Vector2Int.one))
                        if (traverser.TryFindPath(unit, destination, ref path))
                            yield return new UnitAction(UnitActionType.Drop, unit, path: path, targetUnit: cargo, targetPosition: building.position);
                }
            }
        }

        // find units to attack
        if (TryGetAttackRange(unit, out var attackRange)) {

            var unitsToAttack = main.units.Values
                .Select(target => {
                    var damageValues = GetDamageValues(unit, target).ToArray();
                    return (target, damageValues);
                })
                .Where(entry => entry.damageValues.Length > 0);

            foreach (var (target, damageValues) in unitsToAttack) {
                var destinations = main.PositionsInRange(target.NonNullPosition, attackRange)
                    .Where(position => CanStay(unit, position));
                foreach (var destination in destinations)
                    if (traverser.TryFindPath(unit, destination, ref path))
                        yield return new UnitAction(UnitActionType.Attack, unit, path: path, targetUnit: target);
            }
        }

        // find units to join / get in / supply
        {
            var allies = main.units.Values.Where(u => AreAllies(unit.Player, u.Player));
            foreach (var ally in allies) {
                var canJoin = CanJoin(unit, ally);
                var canGetIn = CanGetIn(unit, ally);
                var canSupply = CanSupply(unit, ally);
                if ((canJoin || canGetIn) && traverser.TryFindPath(unit, ally.NonNullPosition, ref path)) {
                    if (canJoin)
                        yield return new UnitAction(UnitActionType.Join, unit, path: path, targetUnit: ally);
                    if (canGetIn)
                        yield return new UnitAction(UnitActionType.GetIn, unit, path: path, targetUnit: ally);
                }
                if (canSupply)
                    foreach (var destination in main.PositionsInRange(ally.NonNullPosition, Vector2Int.one))
                        if (traverser.TryFindPath(unit, destination, ref path))
                            yield return new UnitAction(UnitActionType.Supply, unit, path: path, targetUnit: ally);
            }
        }
    }
}