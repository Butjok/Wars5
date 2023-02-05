using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using static Rules;

// ReSharper disable LoopCanBePartlyConvertedToQuery

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
    [Command]
    public Color textColor = Color.white;
    [Command]
    public Color restPathColor = new Color(1, 1, 1, .5f);

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

    public IEnumerator DrawPotentialUnitActionsCoroutine() {

        if (!Mouse.TryGetPosition(out Vector2Int position) || !main.TryGetUnit(position, out var unit))
            yield break;

        //
        // generate actions
        //

        var actions = new List<PotentialUnitAction>();

        //
        // find buildings to capture
        //
        
        var buildingsToCapture = main.buildings.Values.Where(building => CanCapture(unit, building));
        foreach (var building in buildingsToCapture)
	        if (PathFinding.TryFindMove(unit, building.position))
		        actions.Add(new PotentialUnitAction {
			        unit = unit,
			        type = UnitActionType.Capture,
			        targetBuilding = building,
			        path = PathFinding.Path,
			        restPath = PathFinding.RestPath
		        });

        //
        // find missile silos to use
        //
        
        var missileSilos = main.buildings.Values.Where(building => building.type == TileType.MissileSilo && CanLaunchMissile(unit, building));
        foreach (var missileSilo in missileSilos)
	        if (PathFinding.TryFindMove(unit, missileSilo.position))
		        actions.Add(new PotentialUnitAction {
			        unit = unit,
			        type = UnitActionType.LaunchMissile,
			        targetBuilding = missileSilo,
			        path = PathFinding.Path,
			        restPath = PathFinding.RestPath
		        });

        //
        // find units to attack
        //
        
        if (TryGetAttackRange(unit, out var attackRange)) {

            if (!IsArtillery(unit))
                foreach (var target in main.units.Values) {
                    List<Vector2Int> path = null;
                    List<Vector2Int> restPath = null;
                    foreach (var weaponName in GetWeaponNames(unit))
                        if (CanAttack(unit, target, weaponName)) {
                            if (path == null && PathFinding.TryFindMove(unit, target.NonNullPosition)) {
                                path = PathFinding.Path;
                                restPath = PathFinding.RestPath;
                            }
                            if (path != null)
                                actions.Add(new PotentialUnitAction {
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

        foreach (var action in actions) {
	        //action.priority = ;
        }

        actions.Sort((a, b) => (a.priority - b.priority) switch {
            > 0 => 1,
            < 0 => -1,
            _ => 0
        });

        foreach (var action in actions) {
            while (!Input.GetKeyDown(KeyCode.Alpha0)) {
                yield return null;

                using (Draw.ingame.WithLineWidth(thickness)) {
                    Draw.ingame.Label2D((Vector3)action.restPath[^1].ToVector3Int(), action.ToString(), textSize, LabelAlignment.Center, textColor);
                    var color = GetUnitActionTypeColor(action.type);
                    for (var i = 1; i < action.path.Count; i++)
                        Draw.ingame.Arrow((Vector3)action.path[i - 1].ToVector3Int(), (Vector3)action.path[i].ToVector3Int(), Vector3.up, arrowHeadSize, color);
                    for (var i = 1; i < action.restPath.Count; i++)
                        Draw.ingame.Arrow((Vector3)action.restPath[i - 1].ToVector3Int(), (Vector3)action.restPath[i].ToVector3Int(), Vector3.up, arrowHeadSize, color * new Color(1,1,1,.5f));
                }
            }

            yield return null;

            if (Input.GetKey(KeyCode.RightShift))
                yield break;
        }
    }

    public class PotentialUnitAction {
        public Unit unit;
        public UnitActionType type;
        public Unit targetUnit;
        public Building targetBuilding;
        public Vector2Int targetPosition;
        public WeaponName weaponName;
        public List<Vector2Int> path;
        public List<Vector2Int> restPath;
        public float priority;
        public override string ToString() {
	        var text = "";
	        text += $"{priority}\n";
            text += type.ToString();
            if (targetUnit != null)
                text += $" {targetUnit}";
            if (type == UnitActionType.Drop)
                text += $" to {targetPosition}";
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