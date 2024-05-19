using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitBrainAction : UnitAction {
    public UnitGoal sourceGoal;
    public UnitBrainAction(UnitGoal sourceGoal, UnitActionType type, Unit unit, IEnumerable<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null, WeaponName weaponName = default, Vector2Int targetPosition = default) : base(type, unit, path, targetUnit, targetBuilding, weaponName, targetPosition) {
        this.sourceGoal = sourceGoal;
    }
}

public abstract class UnitGoal {
    public Unit unit;
    [DontSave] public abstract UnitBrainAction NextAction { get; }
    public IEnumerable<Unit> FindEnemiesNearby(int maxDistance = 5) {
        var level = unit.Player.level;
        var positions = level.PositionsInRange(unit.NonNullPosition, new Vector2Int(0, maxDistance));
        foreach (var position in positions)
            if (level.TryGetUnit(position, out var other) && Rules.AreEnemies(unit.Player, other.Player))
                yield return other;
    }
    public bool TryPushKillGoalOfNearbyEnemy(int maxDistance = 5) {
        var enemy = FindEnemiesNearby().FirstOrDefault();
        if (enemy == null)
            return false;
        unit.goals.Push(new UnitKillGoal { unit = unit, target = enemy });
        return true;
    }
    public void Pop() {
        Assert.IsTrue(unit.goals.Peek() == this);
        unit.goals.Pop();
    }
}

public class UnitIdleGoal : UnitGoal {
    public override UnitBrainAction NextAction {
        get {
            if (TryPushKillGoalOfNearbyEnemy())
                return null;
            
            Assert.IsTrue(unit.Position.HasValue);
            return new UnitBrainAction(this, UnitActionType.Stay, unit, new[] { unit.NonNullPosition });
        }
    }
}

public class UnitCaptureGoal : UnitGoal {
    public Building building;
    public override UnitBrainAction NextAction {
        get {
            if (building.Player == unit.Player) {
                Pop();
                return null;
            }

            if (TryPushKillGoalOfNearbyEnemy())
                return null;

            var pathFinder = new PathFinder();
            pathFinder.FindStayMoves(unit);
            if (!pathFinder.TryFindPath(out var shortPath, out _, building.position))
                return null;

            var action = new UnitBrainAction(this, UnitActionType.Capture, unit, shortPath, targetBuilding: building);
            if (shortPath[^1] != building.position)
                action.type = UnitActionType.Stay;

            return action;
        }
    }
}

public class UnitHealGoal : UnitGoal {
    public override UnitBrainAction NextAction {
        get {
            if (unit.Hp >= Rules.MaxHp(unit.type)) {
                Pop();
                return null;
            }

            var level = unit.Player.level;
            var buildings = level.Buildings.Where(b => Rules.CanRepair(b, unit)).ToList();
            var minDistance = 999;
            Building building = null;
            List<Vector2Int> path = null;

            var pathFinder = new PathFinder();
            pathFinder.FindStayMoves(unit);

            foreach (var b in buildings) {
                if (!pathFinder.TryFindPath(out var shortPath, out var restPath, b.position))
                    continue;
                var distance = pathFinder.nodes[restPath[^1]].g;
                if (distance < minDistance) {
                    minDistance = distance;
                    building = b;
                    path = shortPath;
                }
            }

            Assert.IsTrue(building != null);
            
            // if the unit is ready standing on the building, and there is an enemy nearby, attack it
            if (unit.NonNullPosition == building.position && TryPushKillGoalOfNearbyEnemy(1)) 
                return null;
            
            return new UnitBrainAction(this, UnitActionType.Stay, unit, path, targetBuilding: building);
        }
    }
}

public class UnitKillGoal : UnitGoal {

    public Unit target;

    public override UnitBrainAction NextAction {
        get {
            if (target is not { Initialized: true }) {
                Pop();
                return null;
            }

            if (unit.Hp <= 4) {
                unit.goals.Push(new UnitHealGoal { unit = unit });
                return null;
            }

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

            var pathFinder = new PathFinder();
            pathFinder.FindStayMoves(unit);

            // unit is already in the attack position, just attack
            if (attackPositions.Contains(unit.NonNullPosition))
                return action;

            if (!pathFinder.TryFindPath(out var shortPath, out _, targets: attackPositions))
                return null;

            action.path = shortPath;

            // can get into the attack position in one move
            if (attackPositions.Contains(shortPath[^1])) {
                if (Rules.IsArtillery(unit))
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
            if (unit.NonNullPosition == position) {
                Pop();
                return null;
            }

            if (TryPushKillGoalOfNearbyEnemy())
                return null;

            var pathFinder = new PathFinder();
            pathFinder.FindStayMoves(unit);
            if (!pathFinder.TryFindPath(out var shortPath, out _, position))
                return null;

            return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);
        }
    }
}

public class AiPlayerController {
    public Player player;

    public void MoveNextUnit() {
        Game.Instance.StartCoroutine(MoveNextUnitCoroutine());
    }
    public IEnumerator MoveNextUnitCoroutine() {
        var game = Game.Instance;

        while (!game.stateMachine.IsInState<SelectionState>())
            yield return null;
        var units = player.level.Units.Where(u => u.Player == player && !u.Moved && u.goals.Count > 0).ToList();
        if (units.Count == 0) {
            game.EnqueueCommand(SelectionState.Command.EndTurn);
            yield break;
        }

        // try find next action
        var firstUnit = units[0];
        UnitBrainAction action;
        var count = 0;
        do {
            if (count > 1000) {
                Debug.LogError($"{firstUnit} stuck in infinite loop");
                firstUnit.Moved = true;
                yield break;
            }
            action = firstUnit.goals.Peek().NextAction;
            count++;
        } while (firstUnit.goals.Count > 0 && action == null);

        // no action found for this unit, stay
        action ??= new UnitBrainAction(firstUnit.goals.Peek(), UnitActionType.Stay, firstUnit, new[] { firstUnit.NonNullPosition });

        game.EnqueueCommand(SelectionState.Command.Select, firstUnit.NonNullPosition);

        while (!game.stateMachine.IsInState<PathSelectionState>())
            yield return null;
        foreach (var position in action.path)
            game.EnqueueCommand(PathSelectionState.Command.AppendToPath, position);
        game.EnqueueCommand(PathSelectionState.Command.Move);

        while (!game.stateMachine.IsInState<ActionSelectionState>())
            yield return null;

        game.EnqueueCommand(ActionSelectionState.Command.Execute, action);
    }
}