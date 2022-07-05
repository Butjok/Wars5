using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSelectionState : LevelState {

	public Unit unit;
	public List<Vector2Int> path;
	public List<MovePath.Move> moves;
	public Vector2Int startPosition, startForward;
	public List<UnitAction> actions = new();

	public ActionSelectionState(Level level, Unit unit, Vector2Int startPosition, Vector2Int startForward, List<Vector2Int> path, List<MovePath.Move> moves) : base(level) {

		this.unit = unit;
		this.startPosition = startPosition;
		this.startForward = startForward;
		this.path = path;
		this.moves = moves;

		UnitAction newAction(UnitActionType type, Unit unitTarget = null, Building buildingTarget = null, int weapon = -1) {
			return new UnitAction(type, level, unit, path, unitTarget, buildingTarget, weapon);
		}

		var position = path.Last();
		level.TryGetUnit(position, out var other);
		if (other == null || other == unit) {
			if (level.TryGetBuilding(position, out var building) && Rules.CanCapture(unit, building))
				actions.Add(newAction(UnitActionType.Capture, buildingTarget: building));
			else
				actions.Add(newAction(UnitActionType.Stay));
		}
		if (other != null && other.hp.v != Rules.MaxHp(other))
			actions.Add(newAction(UnitActionType.Join, unit));
		if (other != null && Rules.CanTake(other, unit))
			actions.Add(newAction(UnitActionType.GetIn, other));
		if (!Rules.IsArtillery(unit) || path.Count == 1)
			foreach (var otherPosition in level.AttackRange(position, Rules.AttackDistance(unit)))
				if (level.TryGetUnit(otherPosition, out other))
					for (var weapon = 0; weapon < Rules.WeaponsCount(unit); weapon++)
						if (Rules.CanAttack(unit, other, weapon))
							actions.Add(newAction(UnitActionType.Attack, other, weapon: weapon));
		foreach (var offset in Rules.offsets)
			if (level.TryGetUnit(position + offset, out other) && Rules.CanSupply(unit, other))
				actions.Add(newAction(UnitActionType.Supply, other));
	}

	public override void Dispose() {
		foreach (var action in actions)
			action.Dispose();
	}

	public override void Update() {

		if (Input.GetMouseButtonDown(Mouse.right)) {
			unit.view.Position = startPosition;
			unit.view.Forward = startForward;
			level.State = new PathSelectionState(level, unit);
			return;
		}

		/*if (unit.view.turret) {

			if (Mouse.TryGetPosition(out var position) &&
			    level.TryGetUnit(position.RoundToInt(), out var target) &&	
			     Rules.CanAttack(unit,target)) {

				unit.view.turret.ballisticComputer.target = target.view.center;
				unit.view.turret.aim = true;
			}
			else
				unit.view.turret.aim = false;
		}*/
	}
}

public class InGameOverlayMenuState : LevelState {
	public InGameOverlayMenuState(Level level) : base(level) { }
	public override void Update() {
		
	}
}