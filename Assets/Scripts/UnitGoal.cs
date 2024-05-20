using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitBrainAction : UnitAction {
    public const int scoresCount = 10;
    public float[] priorities = new float[scoresCount];
    public UnitGoal sourceGoal;
    public UnitBrainAction(UnitGoal sourceGoal, UnitActionType type, Unit unit, IEnumerable<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null, WeaponName weaponName = default, Vector2Int targetPosition = default) : base(type, unit, path, targetUnit, targetBuilding, weaponName, targetPosition) {
        this.sourceGoal = sourceGoal;
        ResetPriorities();
    }
    public void ResetPriorities() {
        for (var i = 0; i < priorities.Length; i++)
            priorities[i] = 0;
    }
}

public abstract class UnitGoal {
    public Unit unit;
    [DontSave] public abstract UnitBrainAction NextAction { get; }
    public IEnumerable<Unit> FindEnemiesNearby(int maxDistance = 5) {
        return FindEnemiesNearby(unit.NonNullPosition, maxDistance);
    }
    public IEnumerable<Unit> FindEnemiesNearby(Vector2Int position, int maxDistance = 5) {
        var level = unit.Player.level;
        var positions = level.PositionsInRange(position, new Vector2Int(0, maxDistance));
        foreach (var p in positions)
            if (level.TryGetUnit(p, out var other) && Rules.AreEnemies(unit.Player, other.Player))
                yield return other;
    }
    public bool TryAttackNearbyEnemy(out UnitBrainAction action, int maxDistance = 5) {

        var unitsToIgnore = new List<Unit>();
        foreach (var goal in unit.goals)
            if (goal is UnitKillGoal kg)
                unitsToIgnore.Add(kg.target);

        var enemy = FindEnemiesNearby(maxDistance).Except(unitsToIgnore).FirstOrDefault();
        if (enemy == null) {
            action = null;
            return false;
        }

        var result = new UnitKillGoal { unit = unit, target = enemy };
        unit.goals.Push(result);
        action = result.NextAction;
        return true;
    }
    public UnitBrainAction CancelGoalAndReturnPreviousGoalNextAction() {
        Assert.IsTrue(unit.goals.Peek() == this);
        unit.goals.Pop();
        return unit.goals.Count > 0 ? unit.goals.Peek().NextAction : null;
    }
}

public class UnitIdleGoal : UnitGoal {
    public override UnitBrainAction NextAction {
        get {
            if (unit.type != UnitType.Apc && TryAttackNearbyEnemy(out var killGoal))
                return killGoal;

            if (unit.Moved)
                return null;

            return new UnitBrainAction(this, UnitActionType.Stay, unit, new[] { unit.NonNullPosition });
        }
    }
}

public class UnitTransferGoal : UnitGoal {

    public Unit pickUpUnit;
    public Vector2Int dropPosition;

    public override UnitBrainAction NextAction {
        get {
            if (pickUpUnit is not { Initialized: true } || pickUpUnit.Position is { } pickUpUnitPosition && pickUpUnitPosition == dropPosition)
                return CancelGoalAndReturnPreviousGoalNextAction();

            var level = unit.Player.level;
            PathFinder unitPathFinder = null;
            PathFinder UnitPathFinder() => unitPathFinder ??= new PathFinder(unit);

            if (pickUpUnit.Carrier != unit) {
                if (pickUpUnit.Carrier != null && !unit.Moved &&
                    UnitPathFinder().TryFindPath(out var shortPath, out _, targets: level.PositionsInRange(unit.NonNullPosition, Vector2Int.one)))
                    return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);

                if (!pickUpUnit.Moved &&
                    new PathFinder(pickUpUnit, PathFinder.ShortPathDestinationsAreValidTo.MoveThrough).TryFindPath(out var shortPath1, out _, unit.NonNullPosition) && shortPath1[^1] == unit.NonNullPosition)
                    return new UnitBrainAction(this, UnitActionType.GetIn, pickUpUnit, shortPath1);

                if (!unit.Moved &&
                    UnitPathFinder().TryFindPath(out shortPath, out _, targets: level.PositionsInRange(pickUpUnit.NonNullPosition, Vector2Int.one)))
                    return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);
            }

            else if (!unit.Moved &&
                     UnitPathFinder().TryFindPath(out var shortPath, out _, targets: level.PositionsInRange(dropPosition, Vector2Int.one))) {
                if ((shortPath[^1] - dropPosition).ManhattanLength() == 1 && Rules.CanStay(unit, dropPosition)) {
                    return new UnitBrainAction(this, UnitActionType.Drop, unit, shortPath, targetPosition: dropPosition, targetUnit: pickUpUnit);
                }
                return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);
            }

            return null;
        }
    }
}

public class UnitCaptureGoal : UnitGoal {
    public Building building;
    public override UnitBrainAction NextAction {
        get {
            if (building is not { Initialized: true })
                return CancelGoalAndReturnPreviousGoalNextAction();

            if (unit.Moved)
                return null;

            if (TryAttackNearbyEnemy(out var killGoal))
                return killGoal;

            if (building.Player != unit.Player || FindEnemiesNearby(building.position, 3).Any()) {
                if (!new PathFinder(unit).TryFindPath(out var shortPath, out _, building.position))
                    return null;

                var action = new UnitBrainAction(this, UnitActionType.Capture, unit, shortPath, targetBuilding: building);
                if (shortPath[^1] != building.position)
                    action.type = UnitActionType.Stay;

                return action;
            }

            return null;
        }
    }
}

public class UnitHealGoal : UnitGoal {
    public override UnitBrainAction NextAction {
        get {
            if (unit.Hp >= Rules.MaxHp(unit.type))
                return CancelGoalAndReturnPreviousGoalNextAction();

            if (unit.Moved)
                return null;

            var level = unit.Player.level;
            var buildings = level.Buildings.Where(b => Rules.CanRepair(b, unit)).ToList();
            var minDistance = 999;
            Building building = null;
            List<Vector2Int> path = null;

            var pathFinder = new PathFinder(unit);

            foreach (var b in buildings) {
                if (!pathFinder.TryFindPath(out var shortPath, out var restPath, b.position))
                    continue;
                var distance = pathFinder.tiles[restPath[^1]].g;
                if (distance < minDistance) {
                    minDistance = distance;
                    building = b;
                    path = shortPath;
                }
            }

            Assert.IsTrue(building != null);

            // if the unit is ready standing on the building, and there is an enemy nearby, attack it
            if (unit.NonNullPosition == building.position && TryAttackNearbyEnemy(out var killGoal))
                return killGoal;

            return new UnitBrainAction(this, UnitActionType.Stay, unit, path, targetBuilding: building);
        }
    }
}

public class UnitKillGoal : UnitGoal {

    public Unit target;

    public override UnitBrainAction NextAction {
        get {
            if (target is not { Initialized: true })
                return CancelGoalAndReturnPreviousGoalNextAction();

            if (unit.Moved)
                return null;

            if (unit.Hp <= 4) {
                unit.goals.Push(new UnitHealGoal { unit = unit });
                return null;
            }

            if (TryAttackNearbyEnemy(out var killGoal))
                return killGoal;

            var hasAttackRange = Rules.TryGetAttackRange(unit, out var attackRange);
            Assert.IsTrue(hasAttackRange);

            if (target.Position is not { } actualTargetPosition)
                return null;

            var level = unit.Player.level;
            var attackPositions = level.PositionsInRange(actualTargetPosition, attackRange).ToHashSet();

            var weaponName = unit.type switch {
                UnitType.Infantry => WeaponName.Rifle,
                UnitType.AntiTank => WeaponName.RocketLauncher,
                UnitType.Artillery => WeaponName.Cannon,
                UnitType.Recon => WeaponName.MachineGun,
                UnitType.LightTank => WeaponName.Cannon,
                UnitType.Rockets => WeaponName.RocketLauncher,
                UnitType.MediumTank => WeaponName.Cannon,
                _ => throw new NotImplementedException()
            };

            var action = new UnitBrainAction(this, UnitActionType.Attack, unit, new[] { unit.NonNullPosition }, target, weaponName: weaponName);

            // unit is already in the attack position, just attack
            if (attackPositions.Contains(unit.NonNullPosition))
                return action;

            if (!new PathFinder(unit).TryFindPath(out var shortPath, out _, targets: attackPositions))
                return null;

            action.path = shortPath;

            // can get into the attack position in one move
            if (attackPositions.Contains(shortPath[^1])) {
                if (Rules.IsIndirect(unit))
                    action.type = UnitActionType.Stay;
            }
            else
                action.type = UnitActionType.Stay;

            return action;
        }
    }
}

public class UnitMoveGoal : UnitGoal {
    public Vector2Int position;
    public override UnitBrainAction NextAction {
        get {
            if (unit.Moved)
                return null;

            if (TryAttackNearbyEnemy(out var killGoal))
                return killGoal;

            if (!new PathFinder(unit).TryFindPath(out var shortPath, out _, position))
                return null;

            return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);
        }
    }
}

public class AiPlayerController {
    public Player player;

    public void MakeMove(UnitBrainAction action) {
        Game.Instance.StartCoroutine(MakeMoveCoroutine(action));
    }
    public IEnumerator MakeMoveCoroutine(UnitBrainAction action) {
        var game = Game.Instance;

        while (!game.stateMachine.IsInState<SelectionState>())
            yield return null;
        game.EnqueueCommand(SelectionState.Command.Select, action.unit.NonNullPosition);

        while (!game.stateMachine.IsInState<PathSelectionState>())
            yield return null;
        foreach (var position in action.path)
            game.EnqueueCommand(PathSelectionState.Command.AppendToPath, position);
        game.EnqueueCommand(PathSelectionState.Command.Move);

        while (!game.stateMachine.IsInState<ActionSelectionState>())
            yield return null;

        game.EnqueueCommand(ActionSelectionState.Command.Execute, action);
    }
    public void EndTurn() {
        Game.Instance.EnqueueCommand(SelectionState.Command.EndTurn);
    }

    public void MakeMove() {
        var game = Game.Instance;
        var level = game.stateMachine.Find<LevelSessionState>().level;
        var player = level.CurrentPlayer;
        if (player != this.player)
            return;

        var units = level.Units.Where(u => u.Player == player).ToList();
        foreach (var unit in units)
            // if unit is idle come up with a new order
            if (unit.goals.Count == 0 || unit.goals.Peek().GetType() == typeof(UnitIdleGoal)) { }

        var actions = new List<UnitBrainAction>();
        foreach (var unit in units)
            if (unit.goals.Count > 0) {
                var action = unit.goals.Peek().NextAction;
                if (action != null)
                    actions.Add(action);
            }

        if (actions.Count == 0) {
            EndTurn();
            return;
        }

        const float lowerScore = -99999;

        foreach (var action in actions) {
            action.ResetPriorities();
            action.priorities[0] = action.type switch {
                UnitActionType.LaunchMissile => 0,
                UnitActionType.Attack when Rules.IsIndirect(action.unit) => -1,
                UnitActionType.Attack => -2,
                UnitActionType.GetIn => -3,
                UnitActionType.Drop => -4,
                UnitActionType.Capture => -5,
                UnitActionType.Stay when action.unit.type == UnitType.Apc => -6,
                UnitActionType.Stay when !Rules.IsIndirect(action.unit) => -7,
                UnitActionType.Stay => -8,
                UnitActionType.Join => -9,
                UnitActionType.Supply => -10,
                _ => lowerScore
            };
            if (action.type == UnitActionType.Attack) {
                if (Rules.TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damagePercentage)) {
                    var targetCost = Rules.Cost(action.targetUnit);
                    var damageCost = targetCost * damagePercentage;
                    action.priorities[1] = damageCost;
                }
                else
                    action.priorities[1] = lowerScore;
            }
        }

        var sortedActions = actions.OrderByDescending(a => a.priorities[0]);
        for (var i = 1; i < UnitBrainAction.scoresCount; i++) {
            var ii = i;
            sortedActions = sortedActions.ThenByDescending(a => a.priorities[ii]);
        }

        var bestAction = sortedActions.First();
        MakeMove(bestAction);
    }
}