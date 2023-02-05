using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static Rules;
using Random = UnityEngine.Random;

// ReSharper disable LoopCanBePartlyConvertedToQuery

public class AiPlayerCommander : MonoBehaviour {

    public GradientMap priorityGradientMap = new();

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
        StopAllCoroutines();
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
    public float pathThickness = 2;
    [Command]
    public float restPathThickness = 1;
    [Command]
    public float actionLineThickness = 1;
    [Command]
    public float arrowHeadSize = .1f;
    [Command]
    public Color restPathAlpha = new Color(1, 1, 1, .5f);
    [Command]
    public Vector2 actionLineLerp = new Vector2(.1f, 1);

    public Color GetUnitActionTypeColor(UnitActionType type) {
        return type switch {
            UnitActionType.Stay => colorStay,
            UnitActionType.Join => colorJoin,
            UnitActionType.Capture => colorCapture,
            UnitActionType.Attack => colorAttack,
            UnitActionType.GetIn => colorGetIn,
            UnitActionType.Drop => colorDrop,
            UnitActionType.Supply => colorSupply,
            UnitActionType.LaunchMissile => colorLaunchMissile,
            _ => throw new ArgumentOutOfRangeException(type.ToString())
        };
    }

    public Stack<(string text, DateTime dateTime)> priorityFormulaHistory = new();
    [Command]
    public string PriorityFormula {
        get => priorityFormulaHistory.TryPeek(out var top) ? top.text : "0";
        set => priorityFormulaHistory.Push((value, DateTime.Now));
    }
    [Command]
    public bool TryPopPriorityFormula() {
        return priorityFormulaHistory.TryPop(out _);
    }

    public static string PriorityFormulasHistoryPath => Path.Combine(Application.dataPath, "PriorityFormula.txt");
    
    private void Awake() {
        if (File.Exists(PriorityFormulasHistoryPath)) {
            var list = File.ReadAllText(PriorityFormulasHistoryPath).FromJson<List<(string, DateTime)>>();
            list.Reverse();
            foreach (var item in list)
                priorityFormulaHistory.Push(item);
        }
    }
    private void OnApplicationQuit() {
        var list = new List<(string, DateTime)>();
        foreach (var item in priorityFormulaHistory)
            list.Add(item);
        File.WriteAllText(PriorityFormulasHistoryPath, list.ToJson());
    }

    public IEnumerator DrawPotentialUnitActionsCoroutine() {

        if (!Mouse.TryGetPosition(out Vector2Int position) || !main.TryGetUnit(position, out var unit))
            yield break;

        //
        // generate actions
        //

        var actions = new List<FutureUnitAction>();

        //
        // find buildings to capture
        //

        var buildingsToCapture = main.buildings.Values.Where(building => CanCapture(unit, building));
        foreach (var building in buildingsToCapture)
            if (MoveFinder.TryFindMove(unit, building.position))
                actions.Add(new FutureUnitAction {
                    unit = unit,
                    type = UnitActionType.Capture,
                    targetBuilding = building,
                    path = MoveFinder.Path,
                    restPath = MoveFinder.RestPath
                });

        //
        // find missile silos to use
        //

        var missileSilos = main.buildings.Values.Where(building => building.type == TileType.MissileSilo && CanLaunchMissile(unit, building));
        foreach (var missileSilo in missileSilos)
            if (MoveFinder.TryFindMove(unit, missileSilo.position))
                actions.Add(new FutureUnitAction {
                    unit = unit,
                    type = UnitActionType.LaunchMissile,
                    targetBuilding = missileSilo,
                    path = MoveFinder.Path,
                    restPath = MoveFinder.RestPath
                });

        //
        // find units to attack
        //

        if (TryGetAttackRange(unit, out var attackRange)) {

            foreach (var target in main.units.Values) {

                List<Vector2Int> path = null;
                List<Vector2Int> restPath = null;

                foreach (var weaponName in GetWeaponNames(unit))
                    if (CanAttack(unit, target, weaponName)) {

                        if (path == null) {
                            if (MoveFinder.TryFindMove(unit, goals: main.PositionsInRange(target.NonNullPosition, attackRange))) {
                                path = MoveFinder.Path;
                                restPath = MoveFinder.RestPath;
                            }
                            else
                                break;
                        }

                        actions.Add(new FutureUnitAction {
                            unit = unit,
                            type = UnitActionType.Attack,
                            targetUnit = target,
                            weaponName = weaponName,
                            path = path,
                            restPath = restPath
                        });
                    }
            }
        }

        //
        // rank actions
        //

        var minPriority = 0f;
        var maxPriority = 0f;
        var priorityRange = 0f;

        int CalculatePathCost(IEnumerable<Vector2Int> path) {
            return path.Skip(1).Sum(position => {
                var isValid = TryGetMoveCost(unit, position, out var cost);
                Assert.IsTrue(isValid);
                return cost;
            });
        }

        void PrioritizeActions() {
            
            var mostExpensiveUnitCost = UnitTypeSettings.Loaded.Values.Max(item => item.cost);

            foreach (var action in actions) {

                float damageDealt = 0;
                float damageDealtInCredits = 0;
                float damageTaken = 0;
                float damageTakenInCredits = 0;

                if (action.type == UnitActionType.Attack) {

                    var isValid = TryGetDamage(action.unit, action.targetUnit, action.weaponName, out var damageDealtInteger);
                    Assert.IsTrue(isValid);

                    damageDealt = damageDealtInteger;
                    damageDealtInCredits = damageDealt / MaxHp(action.targetUnit) * mostExpensiveUnitCost;

                    // TODO: add damage taken
                }

                action.priority = ExpressionEvaluator.Evaluate(PriorityFormula,
                    ("hp", action.unit.Hp),
                    ("fuel", action.unit.Fuel),
                    ("hasCargo", action.unit.Cargo.Count > 0 ? 1 : 0),
                    ("pathLength", action.path.Count),
                    ("restPathLength", action.restPath.Count),
                    ("fullPathLength", action.path.Count + action.restPath.Count - 1),
                    ("pathCost", CalculatePathCost(action.path)),
                    ("restPathCost", CalculatePathCost(action.restPath)),
                    ("fullPathCost", CalculatePathCost(action.path.Concat(action.restPath.Skip(1)))),
                    (nameof(damageDealt), damageDealt),
                    (nameof(damageDealtInCredits), damageDealtInCredits),
                    (nameof(damageTaken), damageTaken),
                    (nameof(damageTakenInCredits), damageTakenInCredits));
            }

            minPriority = actions.Min(action => action.priority);
            maxPriority = actions.Max(action => action.priority);
            priorityRange = maxPriority - minPriority;
        }
        PrioritizeActions();

        var moveFinderDebugDrawer = FindObjectOfType<MoveFinderDebugDrawer>();
        if (moveFinderDebugDrawer)
            moveFinderDebugDrawer.Show = false;

        while (!Input.GetKeyDown(KeyCode.Alpha0)) {
            yield return null;

            PrioritizeActions();

            foreach (var action in actions) {

                var color = Mathf.Approximately(0, priorityRange) ? Color.blue : priorityGradientMap.Sample((action.priority - minPriority) / priorityRange);

                using (Draw.ingame.WithLineWidth(pathThickness))
                    for (var i = 1; i < action.path.Count; i++)
                        Draw.ingame.Arrow((Vector3)action.path[i - 1].ToVector3Int(), (Vector3)action.path[i].ToVector3Int(), Vector3.up, arrowHeadSize, color);

                using (Draw.ingame.WithLineWidth(restPathThickness))
                    for (var i = 1; i < action.restPath.Count; i++)
                        Draw.ingame.Arrow((Vector3)action.restPath[i - 1].ToVector3Int(), (Vector3)action.restPath[i].ToVector3Int(), Vector3.up, arrowHeadSize, color * restPathAlpha);

                using (Draw.ingame.WithLineWidth(actionLineThickness))
                using (Draw.ingame.WithColor(GetUnitActionTypeColor(action.type))) {

                    //Draw.ingame.Label2D((Vector3)action.restPath[^1].ToVector3Int(), $"{action.priority}: {action}\n", textSize, LabelAlignment.BottomCenter);

                    var from = (Vector3)action.restPath[^1].ToVector3Int();
                    if (action.targetUnit is { Position: { } unitPosition }) {
                        var to = (Vector3)unitPosition.ToVector3Int();
                        Draw.ingame.Line(Vector3.Lerp(from, to, actionLineLerp[0]), Vector3.Lerp(from, to, actionLineLerp[1]));
                    }
                    if (action.targetPosition is { } targetPosition) {
                        var to = (Vector3)targetPosition.ToVector3Int();
                        Draw.ingame.Line(Vector3.Lerp(from, to, actionLineLerp[0]), Vector3.Lerp(from, to, actionLineLerp[1]));
                    }
                    if (action.targetBuilding != null) {
                        var to = (Vector3)action.targetBuilding.position.ToVector3Int();
                        Draw.ingame.Line(Vector3.Lerp(from, to, actionLineLerp[0]), Vector3.Lerp(from, to, actionLineLerp[1]));
                    }
                }
            }

        }
        yield return null;
    }

    public class FutureUnitAction {
        public Unit unit;
        public UnitActionType type;
        public Unit targetUnit;
        public Building targetBuilding;
        public Vector2Int? targetPosition;
        public WeaponName weaponName;
        public List<Vector2Int> path;
        public List<Vector2Int> restPath;
        public float priority;
        public override string ToString() {
            var text = "";
            text += type.ToString();
            if (targetUnit != null)
                text += $" {targetUnit}";
            if (targetBuilding != null)
                text += $" {targetBuilding}";
            if (targetPosition is { } actualTargetPosition)
                text += $" to {actualTargetPosition}";
            if (type == UnitActionType.Attack)
                text += $" with {weaponName}";
            return text;
        }
    }

    public readonly Traverser2 traverser = new();

#if false
	public List<PotentialUnitAction> PotentialUnitActions(Unit unit) {

		var actions = new List<PotentialUnitAction>();
		
		// find buildings to capture 
		{
			var buildingToCapture = main.buildings.Values.Where(building => CanCapture(unit, building));
			foreach (var building in buildingToCapture)
				actions.Add( new PotentialUnitAction {
					unit = unit,
					type = UnitActionType.Capture,
					targetBuilding = building,
					path = traverser.FindPathToStay(unit, building.position)
				});
		}

		// find missile silos to use
		{
			var missileSilos = main.buildings.Values.Where(building => building.type == TileType.MissileSilo && CanLaunchMissile(unit, building));
			foreach (var missileSilo in missileSilos)
				actions.Add(new PotentialUnitAction {
					unit = unit,
					type = UnitActionType.LaunchMissile,
					targetBuilding = missileSilo,
					path = traverser.FindPathToStay(unit, missileSilo.position)
				});
		}

		// find units to attack
		if (TryGetAttackRange(unit, out var attackRange)) {

			if (!IsArtillery(unit))
				foreach (var target in main.units.Values) {
					List<Vector2Int> path = null;
					foreach (var weaponName in GetWeaponNames(unit))
						if (CanAttack(unit, target, weaponName)) {
							path ??= traverser.FindPathToStay(unit, target.NonNullPosition);
							actions.Add(new PotentialUnitAction {
								unit = unit,
								type = UnitActionType.Attack,
								targetUnit = target,
								weaponName = weaponName,
								path = path
							});
						}
				}

		}

		// find units to join / get in / supply
		/*{
			var allies = main.units.Values.Where(u => AreAllies(unit.Player, u.Player));
			foreach (var ally in allies) {
				var canJoin = CanJoin(unit, ally);
				var canGetIn = CanGetIn(unit, ally);
				var canSupply = CanSupply(unit, ally);

				if ((canJoin || canGetIn)) {
					var path = traverser.FindBestPathTo(unit, ally.NonNullPosition);
					if (canJoin)
						yield return new PotentialAction {
							unit = unit,
							type = UnitActionType.Join,
							targetUnit = ally,
							path = path
						};
					if (canGetIn)
						yield return new PotentialAction {
							unit = unit,
							type = UnitActionType.GetIn,
							targetUnit = ally,
							path = path
						};
				}
				if (canSupply)
					foreach (var destination in main.PositionsInRange(ally.NonNullPosition, Vector2Int.one))
						yield return new PotentialAction {
							unit = unit,
							type = UnitActionType.Supply,
							targetUnit = ally,
							path = traverser.FindBestPathTo(unit, destination)
						};
			}
		}*/

		// find buildings to capture by dropping infantry
		/*{
		    if (unit.Cargo.Count > 0) {
		        var cargo = unit.Cargo[0];
		        var buildingToCapture = main.buildings.Values.Where(building => CanCapture(cargo, building));
		        foreach (var building in buildingToCapture) {
		            foreach (var destination in main.PositionsInRange(building.position, Vector2Int.one))
		                yield return new PotentialAction {
		                    actionType = UnitActionType.Drop,
		                    targetUnit =  cargo,
		                    targetPosition = building.position
		                };
		        }
		    }
		}*/


	}
#endif
}