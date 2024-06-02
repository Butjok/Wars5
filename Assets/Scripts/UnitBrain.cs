using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Drawing;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class Order {
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
}

public class UnitBrainAction : UnitAction {
    public float[] precedence = new float[10];
    public UnitState sourceState;
    public UnitBrainAction(UnitState sourceState, UnitActionType type, Unit unit, IEnumerable<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null, WeaponName weaponName = default, Vector2Int targetPosition = default) : base(type, unit, path, targetUnit, targetBuilding, weaponName, targetPosition) {
        this.sourceState = sourceState;
        ResetPrecedence();
    }
    public void ResetPrecedence() {
        for (var i = 0; i < precedence.Length; i++)
            precedence[i] = 0;
    }
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
    public static readonly UnitBrainAction requestOneMoreIteration = null;
    public Unit unit;
    public int createdOnDay;
    public int lifetimeInDays = 999;
    public Order sourceOrder;
    [DontSave] public int AgeInDays => unit.Player.level.Day() - createdOnDay;
    [DontSave] public int DaysLeft => lifetimeInDays - AgeInDays;
    [DontSave] public bool HasExpired => DaysLeft <= 0;
    public abstract UnitBrainAction GetNextAction();
    public IEnumerable<Unit> FindEnemiesNearby(int maxDistance = 5) {
        return FindEnemiesNear(unit.NonNullPosition, maxDistance);
    }
    public IEnumerable<Unit> FindEnemiesNear(Vector2Int position, int maxDistance = 5) {
        var level = unit.Player.level;
        var positions = level.PositionsInRange(position, new Vector2Int(0, maxDistance));
        foreach (var p in positions)
            if (level.TryGetUnit(p, out var other) && Rules.AreEnemies(unit.Player, other.Player))
                yield return other;
    }
    public bool TryActualizeKillStateForTheClosestEnemy(int? maxDistance = null) {
        if (!Rules.TryGetAttackRange(unit, out _))
            return false;

        if (Rules.IsIndirect(unit))
            return false;

        var actualMaxDistance = maxDistance ?? Rules.MoveCapacity(unit) + 1;
        var enemiesNearby = FindEnemiesNearby(actualMaxDistance).ToList();
        if (enemiesNearby.Count == 0)
            return false;

        var target = enemiesNearby.OrderBy(e => (e.NonNullPosition - unit.NonNullPosition).ManhattanLength()).First();
        return TryActualizeKillStateFor(target);
    }
    public bool TryActualizeKillStateFor(Unit target) {
        var existingKillState = unit.states2.SingleOrDefault(s => s is UnitKillState ks && ks.target == target);
        if (existingKillState == null) {
            var result = new UnitKillState {
                unit = unit,
                createdOnDay = unit.Player.level.Day(),
                lifetimeInDays = UnitKillState.defaultLifetimeInDays,
                target = target,
            };
            unit.states2.Add(result);
            return true;
        }
        else {
            if (existingKillState.createdOnDay == unit.Player.level.Day())
                return false;
            // move the state to the end 
            unit.states2.Remove(existingKillState);
            unit.states2.Add(existingKillState);
            // refresh the expiration date
            existingKillState.createdOnDay = unit.Player.level.Day();
            return true;
        }
    }
    public UnitBrainAction Cancel(int popCount = 1) {
        Assert.IsTrue(unit.states2[^1] == this);
        Assert.IsTrue(popCount <= unit.states2.Count);
        unit.states2.RemoveRange(unit.states2.Count - popCount, popCount);
        return requestOneMoreIteration;
    }
    public UnitBrainAction DoNothing() {
        return new UnitBrainAction(this, UnitActionType.Stay, unit, new[] { unit.NonNullPosition });
    }
}

public class UnitTransferState : UnitState {

    public Unit pickUpUnit;
    public Vector2Int dropPosition;

    public override UnitBrainAction GetNextAction() {
        if (pickUpUnit is not { Initialized: true } || pickUpUnit.Position is { } pickUpUnitPosition && pickUpUnitPosition == dropPosition)
            return Cancel();

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

        return DoNothing();
    }
}

public class UnitCaptureState : UnitState {

    public const int defaultLifetimeInDays = 6;
    public Building building;

    public override UnitBrainAction GetNextAction() {
        if (unit.Moved)
            return DoNothing();

        if (building is not { Initialized: true } || building.Player == unit.Player)
            return Cancel();

        if (TryActualizeKillStateForTheClosestEnemy())
            return requestOneMoreIteration;

        if (building.Player != unit.Player || FindEnemiesNear(building.position, 5).Any()) {
            if (!new PathFinder(unit).TryFindPath(out var shortPath, out _, building.position))
                return DoNothing();

            var action = new UnitBrainAction(this, UnitActionType.Capture, unit, shortPath, targetBuilding: building);
            if (shortPath[^1] != building.position)
                action.type = UnitActionType.Stay;

            return action;
        }

        return DoNothing();
    }
}

public class UnitHealState : UnitState {
    public override UnitBrainAction GetNextAction() {
        if (unit.Moved)
            return DoNothing();

        if (unit.Hp >= Rules.MaxHp(unit.type))
            return Cancel();

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

        if (building == null)
            return Cancel();

        // if the unit is ready standing on the building, and there is an enemy nearby, attack it
        if (unit.NonNullPosition == building.position && TryActualizeKillStateForTheClosestEnemy(1))
            return requestOneMoreIteration;

        return new UnitBrainAction(this, UnitActionType.Stay, unit, path);
    }
}

public class UnitKillState : UnitState {

    public const int defaultLifetimeInDays = 2;
    public Unit target;

    public override UnitBrainAction GetNextAction() {
        if (unit.Moved)
            return DoNothing();

        if (target is not { Initialized: true })
            return Cancel();

        if (unit.Hp <= 2) {
            unit.states2.Add(new UnitHealState {
                unit = unit,
                createdOnDay = unit.Player.level.Day(),
                lifetimeInDays = 999
            });
            return requestOneMoreIteration;
        }


        if (TryActualizeKillStateForTheClosestEnemy() && unit.states2[^1] != this)
            return requestOneMoreIteration;

        var hasAttackRange = Rules.TryGetAttackRange(unit, out var attackRange);
        Assert.IsTrue(hasAttackRange);

        if (target.Position is not { } actualTargetPosition) {
            if (target.Carrier != null && TryActualizeKillStateFor(target.Carrier) && unit.states2[^1] != this)
                return requestOneMoreIteration;
            return DoNothing();
        }

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
        if (!Rules.TryGetDamage(unit, target, weaponName, out var damagePercentage) || damagePercentage == 0)
            return Cancel();

        var action = new UnitBrainAction(this, UnitActionType.Attack, unit, new[] { unit.NonNullPosition }, target, weaponName: weaponName);

        // unit is already in the attack position, just attack
        if (attackPositions.Contains(unit.NonNullPosition))
            return action;

        var pathFinder = new PathFinder(unit);
        if (!pathFinder.TryFindPath(out var shortPath, out _, targets: attackPositions))
            return DoNothing();

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

public class UnitMoveState : UnitState {

    public const int defaultLifetimeInDays = 3;
    public Vector2Int position;

    public override UnitBrainAction GetNextAction() {
        if (unit.Moved)
            return DoNothing();

        if (Rules.IsIndirect(unit)) {
            if (unit.Hp <= 2) {
                unit.states2.Add(new UnitHealState {
                    unit = unit,
                    createdOnDay = unit.Player.level.Day()
                });
                return requestOneMoreIteration;
            }

            var weaponName = unit.type switch {
                UnitType.Artillery => WeaponName.Cannon,
                UnitType.Rockets => WeaponName.RocketLauncher,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (Rules.TryGetAttackRange(unit, out var range)) {
                var level = unit.Player.level;
                var attackPositions = level.PositionsInRange(unit.NonNullPosition, range);
                foreach (var position in attackPositions) {
                    if (level.TryGetUnit(position, out var other) &&
                        Rules.CanAttack(unit, unit.NonNullPosition, false, other, other.NonNullPosition, weaponName))
                        return new UnitBrainAction(this, UnitActionType.Attack, unit, new[] { unit.NonNullPosition }, other, weaponName: weaponName);
                }
            }

            var enemyPlayer = unit.Player.level.players.Single(p => Rules.AreEnemies(unit.Player, p));
            var influenceMap = InfluenceMapDrawer.UnitInfluence(unit.Player);
            var enemyInfluenceMap = InfluenceMapDrawer.UnitInfluence(enemyPlayer);
            var artilleryPreferenceMap = InfluenceMapDrawer.ArtilleryPreference(unit.Player);
            if (influenceMap.TryGetValue(unit.NonNullPosition, out var influence) &&
                enemyInfluenceMap.TryGetValue(unit.NonNullPosition, out var enemyInfluence) &&
                artilleryPreferenceMap.TryGetValue(unit.NonNullPosition, out var artilleryPreference) &&
                enemyInfluence > influence && artilleryPreference < .1f) {
                return Cancel();
            }
            if (enemyInfluenceMap.TryGetValue(unit.NonNullPosition, out enemyInfluence) &&
                enemyInfluence == 0 && unit.NonNullPosition == position)
                return Cancel();
        }
        else {
            if (unit.NonNullPosition == position)
                return Cancel();

            if (TryActualizeKillStateForTheClosestEnemy())
                return requestOneMoreIteration;
        }
        if (!new PathFinder(unit).TryFindPath(out var shortPath, out _, position))
            return DoNothing();

        return new UnitBrainAction(this, UnitActionType.Stay, unit, shortPath);
    }
}

public class UnitBrainController {
    public Player player;

    public void MakeMove() {
        if (DamageTable.Loaded == null)
            DamageTable.Load();

        var task = Task.Run(() => {
            var game = Game.Instance;
            var level = game.stateMachine.Find<LevelSessionState>().level;
            var player = level.CurrentPlayer;
            Assert.IsTrue(player == this.player);

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
                
                // add capture orders
                foreach (var building in buildingsToCapture)
                    if (Rules.CanCapture(unit, building))
                        orders.Add(new Order {
                            type = Order.Type.Capture,
                            unit = unit,
                            targetBuilding = building
                        });

                // add kill orders
                if (unit.type != UnitType.Apc && !Rules.IsIndirect(unit)) {
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
                        if (Rules.TryGetDamage(unit.type, enemy.type, weaponName, out var damagePercentage) && damagePercentage > 0)
                            orders.Add(new Order {
                                type = Order.Type.Kill,
                                unit = unit,
                                targetUnit = enemy
                            });
                }

                // add move orders
                foreach (var position in level.tiles.Keys)
                    if (Rules.CanStay(unit.type, level.tiles[position]))
                        orders.Add(new Order {
                            type = Order.Type.Move,
                            unit = unit,
                            targetPosition = position
                        });
            }

            // score orders by their "value"

            var artilleryPreference = InfluenceMapDrawer.ArtilleryPreference(player);

            for (var i = 0; i < orders.Count; i++) {
                var order = orders[i];
                switch (order.type) {
                    case Order.Type.Capture: {
                        var manhattanDistance = (order.unit.NonNullPosition - order.targetBuilding.position).ManhattanLength();
                        order.score = 1.25f / (manhattanDistance + 1);
                        break;
                    }
                    case Order.Type.Kill: {
                        var manhattanDistance = (order.unit.NonNullPosition - order.targetUnit.NonNullPosition).ManhattanLength();
                        order.score = 1.5f / (manhattanDistance + 1);
                        break;
                    }
                    case Order.Type.Move: {
                        if (Rules.IsIndirect(order.unit) &&
                            artilleryPreference.TryGetValue(order.unit.NonNullPosition, out var currentPreference) &&
                            artilleryPreference.TryGetValue(order.targetPosition, out var targetPositionPreference)) {
                            order.score = targetPositionPreference - currentPreference;
                        }
                        else
                            order.score = 0;
                        
                        var manhattanDistance = (order.unit.NonNullPosition - order.targetPosition).ManhattanLength();
                        order.score -= manhattanDistance * .00001f; // break ties by discouraging moving
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                orders[i] = order;
            }

            // assign order to according units

            // order descending
            orders.Sort((a, b) => -a.score.CompareTo(b.score));

            var assignedUnits = new HashSet<Unit>();

            bool TryAssignBestOrderTo(Unit unit) {
                Assert.IsTrue(unit.states2.Count == 0);
                if (orders.Count == 0)
                    return false;
                var order = orders.FirstOrDefault(o => o.unit == unit);
                if (order == null)
                    return false;
                unit.states2.Add(order.type switch {
                    Order.Type.Capture => new UnitCaptureState {
                        unit = unit,
                        sourceOrder = order,
                        createdOnDay = unit.Player.level.Day(),
                        lifetimeInDays = UnitCaptureState.defaultLifetimeInDays,
                        building = order.targetBuilding,
                    },
                    Order.Type.Kill => new UnitKillState {
                        unit = unit,
                        sourceOrder = order,
                        createdOnDay = unit.Player.level.Day(),
                        lifetimeInDays = UnitKillState.defaultLifetimeInDays,
                        target = order.targetUnit,
                    },
                    Order.Type.Move => new UnitMoveState {
                        unit = unit,
                        sourceOrder = order,
                        createdOnDay = unit.Player.level.Day(),
                        lifetimeInDays = Rules.IsIndirect(unit) ? 1 : UnitMoveState.defaultLifetimeInDays,
                        position = order.targetPosition,
                    },
                    _ => throw new ArgumentOutOfRangeException()
                });
                assignedUnits.Add(unit);
                return true;
            }

            //
            //
            // populate all the immediate unit actions from their "brain states"
            //
            //

            var actions = new List<UnitBrainAction>();
            var unitsToProcess = new List<Unit>(units);
            var unitsToProcessOneMoreTime = new List<Unit>();
            for (var i = 0; unitsToProcess.Count > 0; i++) {
                if (i >= 5) {
                    Debug.Log("Too many iterations for units' brain activities, units ignored: " + string.Join(", ", unitsToProcess));
                    break;
                }
                unitsToProcessOneMoreTime.Clear();
                foreach (var unit in unitsToProcess) {
                    unit.states2.RemoveAll(state => state.HasExpired);
                    if (unit.states2.Count > 0) {
                        var action = unit.states2[^1].GetNextAction();
                        if (action == UnitState.requestOneMoreIteration)
                            unitsToProcessOneMoreTime.Add(unit);
                        else if (!action.unit.Moved)
                            actions.Add(action);
                    }
                    else if (TryAssignBestOrderTo(unit))
                        unitsToProcessOneMoreTime.Add(unit);
                }
                (unitsToProcess, unitsToProcessOneMoreTime) = (unitsToProcessOneMoreTime, unitsToProcess);
            }

            if (actions.Count == 0)
                return null;

            const float last = -99999;

            foreach (var action in actions) {
                action.ResetPrecedence();
                action.precedence[0] = action.type switch {
                    UnitActionType.LaunchMissile => 0,
                    UnitActionType.Capture => -1,
                    UnitActionType.Attack when Rules.IsIndirect(action.unit) => -2,
                    UnitActionType.Attack => -3,
                    UnitActionType.GetIn => -4,
                    UnitActionType.Drop => -5,
                    UnitActionType.Stay when action.sourceState.sourceOrder?.type == Order.Type.Capture => -6,
                    UnitActionType.Stay when action.unit.type == UnitType.Apc => -7,
                    UnitActionType.Stay when !Rules.IsIndirect(action.unit) => -8,
                    UnitActionType.Stay => -9,
                    UnitActionType.Join => -10,
                    UnitActionType.Supply => -11,
                    _ => last
                };
                if (action.type == UnitActionType.Attack) {
                    if (Rules.TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damagePercentage)) {
                        var targetCost = Rules.Cost(action.targetUnit);
                        var damageCost = targetCost * damagePercentage;
                        action.precedence[1] = damageCost;
                    }
                    else
                        action.precedence[1] = last;
                }
            }
            actions.Sort((a, b) => UnitBrainAction.CompareLexicographically(a.precedence, b.precedence));

            var selectedAction = actions[0];
            foreach (var unit in assignedUnits)
                if (unit != selectedAction.unit)
                    unit.states2.Clear();

            return selectedAction;
        });

        Game.Instance.StartCoroutine(WaitForMove(task));
    }

    public IEnumerator WaitForMove(Task<UnitBrainAction> task) {

        while (!task.IsCompleted)
            yield return null;

        var action = task.Result;
        if (action == null) {
            Game.Instance.EnqueueCommand(SelectionState.Command.EndTurn);
            yield break;
        }

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