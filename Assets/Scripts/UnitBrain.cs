using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drawing;
using FullscreenEditor;
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
    public float[] priorities = new float[10];
    public UnitState sourceState;
    public UnitBrainAction(UnitState sourceState, UnitActionType type, Unit unit, IEnumerable<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null, WeaponName weaponName = default, Vector2Int targetPosition = default) : base(type, unit, path, targetUnit, targetBuilding, weaponName, targetPosition) {
        this.sourceState = sourceState;
    }
    public static int ComparePriorities(UnitBrainAction a, UnitBrainAction b) {
        Assert.IsTrue(a.priorities.Length == b.priorities.Length);
        for (var i = 0; i < a.priorities.Length; i++) {
            if (a.priorities[i] > b.priorities[i])
                return -1;
            if (a.priorities[i] < b.priorities[i])
                return 1;
        }
        return 0;
    }
}

public abstract class UnitState {
    public const UnitBrainAction requestOneMoreIteration = null;
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
        var index = unit.states2.IndexOf(existingKillState);
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
            if (existingKillState == unit.states2[^1])
                return false;
            // move the state to the end 
            unit.states2.Remove(existingKillState);
            unit.states2.Add(existingKillState);
            var newIndex = unit.states2.IndexOf(existingKillState);
            Assert.IsTrue(newIndex != index);
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
        if (pickUpUnit is not { IsSpawned: true } || pickUpUnit.Position is { } pickUpUnitPosition && pickUpUnitPosition == dropPosition)
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

    public override string ToString() {
        return $"Transfer {pickUpUnit} -> {dropPosition} ";
    }
}

public class UnitCaptureState : UnitState {

    public const int defaultLifetimeInDays = 6;
    public Building building;

    public override UnitBrainAction GetNextAction() {
        if (unit.Moved)
            return DoNothing();

        if (building is not { IsSpawned: true } || building.Player == unit.Player)
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

    public override string ToString() {
        return $"Capture {building}";
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

    public override string ToString() {
        return $"Heal";
    }
}

public class UnitKillState : UnitState {

    public const int defaultLifetimeInDays = 3;
    public Unit target;

    public override UnitBrainAction GetNextAction() {
        if (unit.Moved)
            return DoNothing();

        if (target is not { IsSpawned: true })
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

    public override string ToString() {
        return $"Kill {target}";
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

            var playerInfluenceMap = unit.Player.unitBrainController.playerInfluenceMap;
            var enemyInfluenceMap = unit.Player.unitBrainController.enemyInfluenceMap;
            var artilleryPreferenceMap = unit.Player.unitBrainController.artilleryPreferenceMap;

            if (playerInfluenceMap.TryGetValue(unit.NonNullPosition, out var influence) &&
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

    public override string ToString() {
        return $"Move to {position}";
    }
}

public class UnitBrainController {
    public Player player;
    public Dictionary<Vector2Int, float> playerInfluenceMap, enemyInfluenceMap, artilleryPreferenceMap;

    public void MakeMove() {
        if (!DamageTable.IsLoaded)
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
            // update the artillery preference map
            //
            //

            playerInfluenceMap = InfluenceMapDrawer.UnitInfluence(player);
            enemyInfluenceMap = InfluenceMapDrawer.UnitInfluence(level.players.Single(p => Rules.AreEnemies(player, p)));
            artilleryPreferenceMap = InfluenceMapDrawer.ArtilleryPreference(player.level, playerInfluenceMap, enemyInfluenceMap);

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
                if (!Rules.IsIndirect(unit) && Rules.TryGetAttackRange(unit, out _)) {
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
                        if (Rules.TryGetDamage(unit, enemy, weaponName, out var damagePercentage) && damagePercentage > 0)
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
                            artilleryPreferenceMap.TryGetValue(order.unit.NonNullPosition, out var currentPreference) &&
                            artilleryPreferenceMap.TryGetValue(order.targetPosition, out var targetPositionPreference)) {
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

            var assignedUnits = new List<Unit>();

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

            var unitStates = new Dictionary<Unit, List<UnitState>>();

            var actions = new List<UnitBrainAction>();
            var unitsToProcess = new List<Unit>(units);
            var unitsToProcessOneMoreTime = new List<Unit>();
            for (var i = 0; unitsToProcess.Count > 0; i++) {
                if (i >= 5) {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append("Too many iterations for units' brain activities, units ignored:\n");
                    foreach (var unit in unitsToProcess) {
                        stringBuilder.Append($"{unit}:\n");
                        if (unitStates.TryGetValue(unit, out var history))
                            foreach (var state in history)
                                stringBuilder.Append($"  {state}\n");
                    }
                    Debug.Log(stringBuilder.ToString());
                    break;
                }
                unitsToProcessOneMoreTime.Clear();
                foreach (var unit in unitsToProcess) {
                    unit.states2.RemoveAll(state => state.HasExpired);
                    if (unit.states2.Count > 0) {
                        var state = unit.states2[^1];
                        var action = state.GetNextAction();
                        if (action == UnitState.requestOneMoreIteration) {
                            if (!unitStates.ContainsKey(unit))
                                unitStates.Add(unit, new List<UnitState>());
                            unitStates[unit].Add(state);
                            unitsToProcessOneMoreTime.Add(unit);
                        }
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
                action.priorities[0] = action.type switch {
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
                        action.priorities[1] = damageCost;
                    }
                    else
                        action.priorities[1] = last;
                }
            }
            var selectedAction = actions[0];
            foreach (var action in actions)
                if (UnitBrainAction.ComparePriorities(action, selectedAction) < 0)
                    selectedAction = action;

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

        var game = Game.Instance;
        game.dontShowMoveUi = true;
        while (!game.stateMachine.IsInState<SelectionState>())
            yield return null;

        if (action == null) {
            game.EnqueueCommand(SelectionState.Command.EndTurn);
            yield break;
        }

        game.EnqueueCommand(SelectionState.Command.Select, action.unit.NonNullPosition);

        while (!game.stateMachine.IsInState<PathSelectionState>())
            yield return null;
        foreach (var position in action.path)
            game.EnqueueCommand(PathSelectionState.Command.AppendToPath, position);
        game.EnqueueCommand(PathSelectionState.Command.Move);

        while (action.unit.Hp > 0 && !game.stateMachine.IsInState<ActionSelectionState>())
            yield return null;

        if (action.unit.Hp > 0)
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