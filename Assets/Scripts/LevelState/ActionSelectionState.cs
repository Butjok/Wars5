using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSelectionState : State2<Game2> {

	public Unit unit;
	public MovePath path;
	public Vector2Int startForward;
	public List<UnitAction> actions = new();
	public int index = -1;
	public UnitAction action;

	public ActionSelectionState(Game2 parent, Unit unit, Vector2Int startForward, MovePath path) : base(parent) {

		this.unit = unit;
		this.startForward = startForward;
		this.path = path;

		UnitAction newAction(UnitActionType type, Unit unitTarget = null, Building buildingTarget = null, int weapon = -1) {
			return new UnitAction(type, unit, path, unitTarget, buildingTarget, weapon);
		}

		var position = path.positions.Last();
		parent.TryGetUnit(position, out var other);
		if (other == null || other == unit) {
			if (parent.TryGetBuilding(position, out var building) && Rules.CanCapture(unit, building))
				actions.Add(newAction(UnitActionType.Capture, buildingTarget: building));
			else
				actions.Add(newAction(UnitActionType.Stay));
		}
		if (other != null && other.hp.v != Rules.MaxHp(other))
			actions.Add(newAction(UnitActionType.Join, unit));
		if (other != null && Rules.CanLoadAsCargo(other, unit))
			actions.Add(newAction(UnitActionType.GetIn, other));
		if (!Rules.IsArtillery(unit) || path.positions.Count == 1)
			foreach (var otherPosition in parent.AttackPositions(position, Rules.AttackRange(unit)))
				if (parent.TryGetUnit(otherPosition, out other))
					for (var weapon = 0; weapon < Rules.WeaponsCount(unit); weapon++)
						if (Rules.CanAttack(unit, other, weapon))
							actions.Add(newAction(UnitActionType.Attack, other, weapon: weapon));
		foreach (var offset in Rules.offsets)
			if (parent.TryGetUnit(position + offset, out other) && Rules.CanSupply(unit, other))
				actions.Add(newAction(UnitActionType.Supply, other));
	}

	public override void Dispose() {
		base.Dispose();
		foreach (var action in actions)
			action.Dispose();
	}

	public override void Update() {
		base.Update();

		void notAllowed() {
			UiSound.Instance.notAllowed.Play();
			Debug.Log("not actions");
		}

		if (Input.GetMouseButtonDown(Mouse.right)) {
			unit.view.Position = path.positions[0];
			unit.view.Forward = startForward;
			ChangeTo(new PathSelectionState(parent, unit));
		}

		else if (Input.GetKeyDown(KeyCode.Tab)) {
			if (actions.Count > 0) {
				index = (index + 1) % actions.Count;
				action = actions[index];
				Debug.Log(action);
			}
			else
				notAllowed();
		}

		else if (Input.GetKeyDown(KeyCode.Return)) {
			if (action != null) {
				action.Execute();
				unit.moved.v = true;
				if (Rules.Won(parent.realPlayer))
					ChangeTo(new VictoryState(parent));
				else if (Rules.Lost(parent.realPlayer))
					ChangeTo(new DefeatState(parent));
				else
					ChangeTo(parent.levelLogic.OnActionCompletion(action) ?? new SelectionState(parent));
			}
			else
				notAllowed();
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