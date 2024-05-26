using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using SaveGame;
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
    public List<Vector2Int> shortPath, restPath;
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
    public static readonly UnitBrainAction requestOneMoreIteration = null;
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
    [DontSave] public IEnumerable<Unit> UnitsAlreadyTargeted {
        get {
            foreach (var state in unit.states)
                if (state is UnitKillState killState)
                    yield return killState.target;
        }
    }
    public bool TryStartAttackingNearbyEnemy() {
        if (!Rules.TryGetAttackRange(unit, out _))
            return false;

        if (Rules.IsIndirect(unit))
            return false;

        var maxDistance = Rules.MoveCapacity(unit) + 1;
        var enemy = FindEnemiesNearby(maxDistance).Except(UnitsAlreadyTargeted).FirstOrDefault();
        if (enemy == null)
            return false;

        var result = new UnitKillState {
            unit = unit,
            createdOnDay = unit.Player.level.Day(),
            lifetimeInDays = UnitKillState.defaultLifetimeInDays,
            target = enemy
        };
        unit.states.Push(result);
        return true;
    }
    public UnitBrainAction Pop(int popCount = 1) {
        Assert.IsTrue(unit.states.Peek() == this);
        Assert.IsTrue(popCount <= unit.states.Count);
        for (var i = 0; i < popCount; i++)
            unit.states.Pop();
        return requestOneMoreIteration;
    }
    public UnitBrainAction DoNothing() {
        return new UnitBrainAction(this, UnitActionType.Stay, unit, new[] { unit.NonNullPosition });
    }
}

public class UnitTransferState : UnitState {

    public Unit pickUpUnit;
    public Vector2Int dropPosition;

    public override UnitBrainAction NextAction {
        get {
            if (pickUpUnit is not { Initialized: true } || pickUpUnit.Position is { } pickUpUnitPosition && pickUpUnitPosition == dropPosition)
                return Pop();

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
}

public class UnitCaptureState : UnitState {

    public const int defaultLifetimeInDays = 6;
    public Building building;

    public override UnitBrainAction NextAction {
        get {
            if (unit.Moved)
                return DoNothing();

            if (building is not { Initialized: true } || building.Player == unit.Player)
                return Pop();

            if (TryStartAttackingNearbyEnemy())
                return requestOneMoreIteration;

            if (building.Player != unit.Player || FindEnemiesNearby(building.position, 5).Any()) {
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
}

public class UnitHealState : UnitState {
    public override UnitBrainAction NextAction {
        get {
            if (unit.Moved)
                return DoNothing();

            if (unit.Hp >= Rules.MaxHp(unit.type))
                return Pop();

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
                return Pop();

            // if the unit is ready standing on the building, and there is an enemy nearby, attack it
            if (unit.NonNullPosition == building.position && TryStartAttackingNearbyEnemy())
                return requestOneMoreIteration;

            return new UnitBrainAction(this, UnitActionType.Stay, unit, path);
        }
    }
}

public class UnitKillState : UnitState {

    public const int defaultLifetimeInDays = 2;
    public Unit target;

    public override UnitBrainAction NextAction {
        get {
            if (unit.Moved)
                return DoNothing();

            if (target is not { Initialized: true })
                return Pop();

            if (unit.Hp <= 2) {
                unit.states.Push(new UnitHealState {
                    unit = unit,
                    createdOnDay = unit.Player.level.Day()
                });
                return requestOneMoreIteration;
            }

            if (TryStartAttackingNearbyEnemy()) {
                var killState = (UnitKillState)unit.states.Peek();
                var killAction = killState.NextAction;
                if (killAction != requestOneMoreIteration && killAction.type == UnitActionType.Attack &&
                    (killAction.targetUnit.NonNullPosition - unit.NonNullPosition).ManhattanLength() <=
                    (target.NonNullPosition - unit.NonNullPosition).ManhattanLength()) {
                    return killAction;
                }
                unit.states.Pop();
            }

            var hasAttackRange = Rules.TryGetAttackRange(unit, out var attackRange);
            Assert.IsTrue(hasAttackRange);

            if (target.Position is not { } actualTargetPosition) {
                if (target.Carrier != null && !UnitsAlreadyTargeted.Contains(target.Carrier)) {
                    unit.states.Push(new UnitKillState {
                        unit = unit,
                        createdOnDay = unit.Player.level.Day(),
                        lifetimeInDays = UnitKillState.defaultLifetimeInDays,
                        target = target.Carrier
                    });
                    return requestOneMoreIteration;
                }
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
                return Pop();

            var action = new UnitBrainAction(this, UnitActionType.Attack, unit, new[] { unit.NonNullPosition }, target, weaponName: weaponName);

            // unit is already in the attack position, just attack
            if (attackPositions.Contains(unit.NonNullPosition))
                return action;

            var pathFinder = new PathFinder(unit);
            if (!pathFinder.TryFindPath(out var shortPath, out _, targets: attackPositions))
                return DoNothing();
            
            //pathFinder.DrawNodes(1.5f);

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

    public const int defaultLifetimeInDays = 3;
    public Vector2Int position;

    public override UnitBrainAction NextAction {
        get {
            if (unit.Moved)
                return DoNothing();

            if (Rules.IsIndirect(unit)) {
                
                if (unit.Hp <= 2) {
                    unit.states.Push(new UnitHealState {
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
                    return Pop();
                }
                if (enemyInfluenceMap.TryGetValue(unit.NonNullPosition, out enemyInfluence) &&
                    enemyInfluence == 0 && unit.NonNullPosition == position)
                    return Pop();
            }
            else {
                if (unit.NonNullPosition == position)
                    return Pop();

                if (TryStartAttackingNearbyEnemy())
                    return requestOneMoreIteration;
            }
            if (!new PathFinder(unit).TryFindPath(out var shortPath, out _, position))
                return Pop();

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

            // add capture orders
            foreach (var building in buildingsToCapture)
                if (Rules.CanCapture(unit, building) && pathFinder.TryFindPath(out var shortPath, out var restPath, building.position))
                    orders.Add(new Order {
                        type = Order.Type.Capture,
                        unit = unit,
                        targetBuilding = building,
                        pathCost = pathFinder.FullCost(building.position),
                        shortPath = shortPath,
                        restPath = restPath
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
                    if (Rules.TryGetDamage(unit.type, enemy.type, weaponName, out var damagePercentage) && damagePercentage > 0 &&
                        pathFinder.TryFindPath(out var shortPath, out var restPath, enemy.NonNullPosition))
                        orders.Add(new Order {
                            type = Order.Type.Kill,
                            unit = unit,
                            targetUnit = enemy,
                            pathCost = pathFinder.FullCost(enemy.NonNullPosition),
                            shortPath = shortPath,
                            restPath = restPath,
                        });
            }

            // add move orders
            foreach (var position in level.tiles.Keys)
                if (Rules.CanStay(unit, position) && pathFinder.TryFindPath(out var shortPath, out var restPath, position))
                    orders.Add(new Order {
                        type = Order.Type.Move,
                        unit = unit,
                        targetPosition = position,
                        pathCost = pathFinder.FullCost(position),
                        shortPath = shortPath,
                        restPath = restPath
                    });
        }

        // score orders by their "value"

        var artilleryPreference = InfluenceMapDrawer.ArtilleryPreference(player);

        for (var i = 0; i < orders.Count; i++) {
            var order = orders[i];
            switch (order.type) {
                case Order.Type.Capture:
                    order.score = 1.25f / (order.pathCost + 1);
                    break;
                case Order.Type.Kill:
                    order.score = 1.5f / (order.pathCost + 1);
                    break;
                case Order.Type.Move:
                    if (Rules.IsIndirect(order.unit) &&
                        artilleryPreference.TryGetValue(order.shortPath[^1], out var shortPathDestinationPreference) &&
                        artilleryPreference.TryGetValue(order.targetPosition, out var targetPositionPreference)) {
                        order.score = (shortPathDestinationPreference + targetPositionPreference)/2;
                        var moveCapacity = Rules.MoveCapacity(order.unit);
                        //order.score -= Mathf.Max(0f, order.pathCost - moveCapacity) / moveCapacity;
                    }
                    else {
                        order.score = 0;
                    }
                    order.score -= order.shortPath.Count * .001f; // break ties by discouraging moving
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            orders[i] = order;
        }

        // assign order to according units

        // order descending
        orders.Sort((a, b) => -a.score.CompareTo(b.score));

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
        // populate all the immediate unit actions from their "brain states"
        //
        //

        var actions = new List<UnitBrainAction>();
        var unitsToIterate = new HashSet<Unit>(units);
        var unitsToIterateOneMoreTime = new HashSet<Unit>();
        for (var i = 0; unitsToIterate.Count > 0; i++) {
            if (i >= 4) {
                Debug.Log("Too many iterations for units' brain activities");
                break;
            }
            unitsToIterateOneMoreTime.Clear();
            foreach (var unit in unitsToIterate) {
                var popCount = 0;
                var depth = 0;
                foreach (var state in unit.states) {
                    depth++;
                    if (state.Expired) {
                        if (state.lifetimeInDays <= 0)
                            Debug.LogError($"Unit {unit} has a state with invalid lifetime, state: {state}");
                        popCount = depth;
                    }
                }
                for (var j = 0; j < popCount; j++)
                    unit.states.Pop();

                if (unit.states.Count > 0) {
                    var action = unit.states.Peek().NextAction;
                    if (action == UnitState.requestOneMoreIteration)
                        unitsToIterateOneMoreTime.Add(unit);
                    else if (!action.unit.Moved)
                        actions.Add(action);
                }
                else
                    unitsToIterateOneMoreTime.Add(unit);
            }
            (unitsToIterate, unitsToIterateOneMoreTime) = (unitsToIterateOneMoreTime, unitsToIterate);
        }

        if (actions.Count == 0) {
            EndTurn();
            return;
        }

        const float last = -99999;

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
                _ => last
            };
            if (action.type == UnitActionType.Attack) {
                if (Rules.TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damagePercentage)) {
                    var targetCost = Rules.Cost(action.targetUnit);
                    var damageCost = targetCost * damagePercentage;
                    action.order[1] = damageCost;
                }
                else
                    action.order[1] = last;
            }
        }
        actions.Sort((a, b) => UnitBrainActionExtensions.CompareLexicographically(a.order, b.order));

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