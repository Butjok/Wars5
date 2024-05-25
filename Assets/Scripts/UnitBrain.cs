using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Plugins.Core.PathCore;
using Drawing;
using SaveGame;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public struct Order {
    public enum Type {
        Kill,
        Capture,
        Move
    }
    public Type type;
    public Unit unit;
    public float score;
    public Unit targetUnit;
    public Building targetBuilding;
    public Vector2Int targetPosition;
    public int pathCost;
}

public class UnitBrainAction : UnitAction {
    public float[] order = new float[10];
    public UnitState sourceState;
    public UnitBrainAction(UnitState sourceState, UnitActionType type, Unit unit, IEnumerable<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null, WeaponName weaponName = default, Vector2Int targetPosition = default) : base(type, unit, path, targetUnit, targetBuilding, weaponName, targetPosition) {
        this.sourceState = sourceState;
        ResetOrder();
    }
    public void ResetOrder() {
        for (var i = 0; i < order.Length; i++)
            order[i] = 0;
    }
}
public static class UnitBrainActionExtensions {
    public static int CompareLexicographically(float[] a, float[] b) {
        Assert.IsTrue(a.Length == b.Length);
        for (var i = 0; i < a.Length; i++) {
            if (a[i] > b[i])
                return -1;
            if (a[i] < b[i])
                return 1;
        }
        return 0;
    }
}

public abstract class UnitState {
    public Unit unit;
    public int createdOnDay;
    public int lifetimeInDays = 999;
    [DontSave] public int AgeInDays => unit.Player.level.Day() - createdOnDay;
    [DontSave] public int DaysLeft => lifetimeInDays - AgeInDays;
    [DontSave] public bool Expired => DaysLeft <= 0;
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
        if (!Rules.TryGetAttackRange(unit, out _)) {
            action = null;
            return false;
        }

        var unitsToIgnore = new List<Unit>();
        foreach (var goal in unit.states)
            if (goal is UnitKillState kg)
                unitsToIgnore.Add(kg.target);

        var enemy = FindEnemiesNearby(maxDistance).Except(unitsToIgnore).FirstOrDefault();
        if (enemy == null) {
            action = null;
            return false;
        }

        var result = new UnitKillState {
            unit = unit,
            createdOnDay = unit.Player.level.Day(),
            lifetimeInDays = UnitKillState.defaultLifetimeInDays,
            target = enemy
        };
        unit.states.Push(result);
        action = result.NextAction;
        return true;
    }
    public UnitBrainAction PopStateAndReturnPreviousStateNextAction() {
        Assert.IsTrue(unit.states.Peek() == this);
        unit.states.Pop();
        return unit.states.Count > 0 ? unit.states.Peek().NextAction : null;
    }
}

public class UnitTransferState : UnitState {

    public Unit pickUpUnit;
    public Vector2Int dropPosition;

    public override UnitBrainAction NextAction {
        get {
            if (pickUpUnit is not { Initialized: true } || pickUpUnit.Position is { } pickUpUnitPosition && pickUpUnitPosition == dropPosition)
                return PopStateAndReturnPreviousStateNextAction();

            var level = unit.Player.level;
            PathFinder unitPathFinder = null;
            PathFinder UnitPathFinder() => unitPathFinder ??= new PathFinder(unit);

            if (pickUpUnit.Carrier != unit) {
                if (pickUpUnit.Carrier != null && !unit.Moved &&
                    UnitPathFinder().TryFindPath(out var shortPath, out _, targets: level.PositionsInRange(unit.NonNullPosition, Vector2Int.one)))
                    return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);

                if (!pickUpUnit.Moved &&
                    new PathFinder(pickUpUnit, allowedStayPositions: new HashSet<Vector2Int> { unit.NonNullPosition }).TryFindPath(out var shortPath1, out _, unit.NonNullPosition) &&
                    shortPath1[^1] == unit.NonNullPosition)
                    return new UnitBrainAction(this, UnitActionType.GetIn, pickUpUnit, shortPath1);

                if (!unit.Moved &&
                    UnitPathFinder().TryFindPath(out shortPath, out _, targets: level.PositionsInRange(pickUpUnit.NonNullPosition, Vector2Int.one))) {
                    return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);
                }
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

public class UnitCaptureState : UnitState {

    public const int defaultLifetimeInDays = 6;
    public Building building;

    public override UnitBrainAction NextAction {
        get {
            if (building is not { Initialized: true } || building.Player == unit.Player)
                return PopStateAndReturnPreviousStateNextAction();

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

public class UnitHealState : UnitState {
    public override UnitBrainAction NextAction {
        get {
            if (unit.Hp >= Rules.MaxHp(unit.type))
                return PopStateAndReturnPreviousStateNextAction();

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
                var distance = pathFinder.tiles[restPath[^1]].fullCost;
                if (distance < minDistance) {
                    minDistance = distance;
                    building = b;
                    path = shortPath;
                }
            }

            Assert.IsTrue(building != null);

            /*using (Draw.ingame.WithDuration(10))
                foreach (var position in pathFinder.tiles.Keys) {
                    var position3d = position.Raycasted();
                    Draw.ingame.Label3D(position3d, quaternion.identity, $"{pathFinder.tiles[position].g}", .2f, LabelAlignment.Center, Color.black);
                    var shortCameFrom = pathFinder.tiles[position].shortCameFrom;
                    if (shortCameFrom.HasValue)
                        Draw.ingame.Line(position3d, shortCameFrom.Value.Raycasted(), Color.green);
                    
                }*/

            // if the unit is ready standing on the building, and there is an enemy nearby, attack it
            if (unit.NonNullPosition == building.position && TryAttackNearbyEnemy(out var killGoal))
                return killGoal;

            return new UnitBrainAction(this, UnitActionType.Stay, unit, path, targetBuilding: building);
        }
    }
}

public class UnitKillState : UnitState {

    public const int defaultLifetimeInDays = 2;
    public Unit target;

    public override UnitBrainAction NextAction {
        get {
            if (target is not { Initialized: true })
                return PopStateAndReturnPreviousStateNextAction();

            if (unit.Moved)
                return null;

            if (unit.Hp <= 4) {
                var healState = new UnitHealState {
                    unit = unit,
                    createdOnDay = unit.Player.level.Day()
                };
                unit.states.Push(healState);
                return healState.NextAction;
            }

            if (TryAttackNearbyEnemy(out var killGoal) &&
                (killGoal.targetUnit.NonNullPosition - unit.NonNullPosition).ManhattanLength() <
                (target.NonNullPosition - unit.NonNullPosition).ManhattanLength())
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

            var pathFinder = new PathFinder(unit);
            if (!pathFinder.TryFindPath(out var shortPath, out _, targets: attackPositions))
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

public class UnitMoveState : UnitState {

    public const int defaultLifetimeInDays = 6;
    public Vector2Int position;

    public override UnitBrainAction NextAction {
        get {
            if (unit.NonNullPosition == position)
                return PopStateAndReturnPreviousStateNextAction();

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

public class UnitBrainController {
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
        var enemyUnits = level.Units.Where(u => Rules.AreEnemies(u.Player, this.player)).ToList();
        var buildingsToCapture = level.Buildings.Where(b => b.Player != this.player).ToList();

        //
        //
        // populate all possible "far" orders
        //
        //

        var orders = new List<Order>();
        foreach (var unit in units) {
            // ignore units with assigned states
            if (unit.states.Count > 0)
                continue;

            var pathFinder = new PathFinder(unit);
            List<Vector2Int> shortPath, restPath;

            // add capture orders
            foreach (var building in buildingsToCapture)
                if (Rules.CanCapture(unit, building) && pathFinder.TryFindPath(out shortPath, out restPath, building.position))
                    orders.Add(new Order {
                        type = Order.Type.Capture,
                        unit = unit,
                        targetBuilding = building,
                        pathCost = pathFinder.FullCost(building.position),
                    });

            // add kill orders
            if (unit.type != UnitType.Apc) {
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

                foreach (var enemy in enemyUnits)
                    if (Rules.TryGetDamage(unit.type, enemy.type, weaponName, out _) && pathFinder.TryFindPath(out shortPath, out restPath, enemy.NonNullPosition))
                        orders.Add(new Order {
                            type = Order.Type.Kill,
                            unit = unit,
                            targetUnit = enemy,
                            pathCost = pathFinder.FullCost(enemy.NonNullPosition),
                        });
            }

            // add move orders
            foreach (var position in level.tiles.Keys)
                if (Rules.CanStay(unit, position) && pathFinder.TryFindPath(out shortPath, out restPath, position))
                    orders.Add(new Order {
                        type = Order.Type.Move,
                        unit = unit,
                        targetPosition = position,
                        pathCost = pathFinder.FullCost(position),
                    });
        }

        // score orders by their "value"

        for (var i = 0; i < orders.Count; i++) {
            var order = orders[i];
            switch (order.type) {
                case Order.Type.Capture:
                    order.score = 1.5f / (order.pathCost + 1);
                    break;
                case Order.Type.Kill:
                    order.score = 1.25f / (order.pathCost + 1);
                    break;
                case Order.Type.Move:
                    order.score = 0; // 1f / (order.pathCost + 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            orders[i] = order;
        }

        // assign order to according units

        // order descending
        orders.Sort((a, b) => -a.score.CompareTo(b.score));

        /*foreach (var order in orders)
            using (Draw.ingame.WithLineWidth(.5f))
            using (Draw.ingame.WithDuration(5)) {
                var color = order.type switch {
                    Order.Type.Kill => Color.red,
                    Order.Type.Capture => Color.cyan,
                    Order.Type.Move => Color.green,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var targetPosition = order.type switch {
                    Order.Type.Kill => order.targetUnit.NonNullPosition.Raycasted(),
                    Order.Type.Capture => order.targetBuilding.position.Raycasted(),
                    Order.Type.Move => order.targetPosition.Raycasted(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                Draw.ingame.Line(order.unit.NonNullPosition.Raycasted(), targetPosition, color);
            }*/

        var assignedUnits = new HashSet<Unit>();
        foreach (var order in orders) {
            if (assignedUnits.Contains(order.unit))
                continue;
            assignedUnits.Add(order.unit);

            order.unit.states.Clear();
            switch (order.type) {
                case Order.Type.Capture:
                    order.unit.states.Push(new UnitCaptureState {
                        unit = order.unit,
                        createdOnDay = order.unit.Player.level.Day(),
                        lifetimeInDays = UnitCaptureState.defaultLifetimeInDays,
                        building = order.targetBuilding
                    });
                    break;
                case Order.Type.Kill:
                    order.unit.states.Push(new UnitKillState {
                        unit = order.unit,
                        createdOnDay = order.unit.Player.level.Day(),
                        lifetimeInDays = UnitKillState.defaultLifetimeInDays,
                        target = order.targetUnit
                    });
                    break;
                case Order.Type.Move:
                    order.unit.states.Push(new UnitMoveState {
                        unit = order.unit,
                        createdOnDay = order.unit.Player.level.Day(),
                        lifetimeInDays = UnitMoveState.defaultLifetimeInDays,
                        position = order.targetPosition
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //
        //
        // populate all unit actions from their "brain states"
        //
        //

        var actions = new List<UnitBrainAction>();
        foreach (var unit in units) {
            while (unit.states.TryPeek(out var state) && state.Expired) {
                if (state.lifetimeInDays == 0)
                    Debug.LogError($"Unit {unit} has a state with invalid lifetime, state: {state}");
                unit.states.Pop();
            }
            if (unit.states.Count > 0) {
                var action = unit.states.Peek().NextAction;
                if (action != null && !action.unit.Moved)
                    actions.Add(action);
            }
        }

        if (actions.Count == 0) {
            EndTurn();
            return;
        }

        const float lowerScore = -99999;

        foreach (var action in actions) {
            action.ResetOrder();
            action.order[0] = action.type switch {
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
                    action.order[1] = damageCost;
                }
                else
                    action.order[1] = lowerScore;
            }
        }
        actions.Sort((a, b) => UnitBrainActionExtensions.CompareLexicographically(a.order, b.order));

        //DrawAction(actions[0]);

        //Debug.Log(actions[0]);

        MakeMove(actions[0]);
    }

    public static void DrawAction(UnitBrainAction action, float duration = 10) {
        using (Draw.ingame.WithDuration(duration)) {
            var color = action.type switch {
                UnitActionType.LaunchMissile => Color.red,
                UnitActionType.Attack => Color.red,
                UnitActionType.GetIn => Color.blue,
                UnitActionType.Drop => Color.blue,
                UnitActionType.Capture => Color.cyan,
                UnitActionType.Stay => Color.green,
                UnitActionType.Join => Color.yellow,
                UnitActionType.Supply => Color.yellow,
                _ => Color.black
            };
            using (Draw.ingame.WithLineWidth(1.5f))
                Draw.ingame.Line(action.unit.NonNullPosition.Raycasted(), action.path.Last().Raycasted(), color);
        }
    }
}