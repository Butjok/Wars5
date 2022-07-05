using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectionState : LevelState {

	public List<Unit> unitLoop;
	public int unitIndex = -1;
	public Unit cycledUnit;

	public SelectionState(Level level) : base(level) {
		unitLoop = level.units.Values
			.Where(unit => unit.player == CurrentPlayer && !unit.moved.v)
			.OrderBy(unit => Vector3.Distance(CameraRig.Instance.transform.position, unit.view.center.position))
			.ToList();
	}

	public override void Update() {
		if (cycledUnit != null && Camera.main) {
			var worldPosition = cycledUnit.view.center.position;
			var screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
			if (screenPosition.x is < 0 or > 1 || screenPosition.y is < 0 or > 1)
				cycledUnit = null;
		}
		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (unitLoop.Count > 0) {
				unitIndex = (unitIndex + 1) % unitLoop.Count;
				var next = unitLoop[unitIndex];
				CameraRig.Instance.Jump(next.view.center.position);
				cycledUnit = next;
			}
			else
				UiSound.NotAllowed();
		}
		if (Input.GetMouseButtonDown(Mouse.left) &&
		    Mouse.TryGetPosition(out var position) &&
		    level.TryGetUnit(position.RoundToInt(), out var unit)) {

			if (unit.moved.v)
				UiSound.NotAllowed();
			else {
				SelectUnit(unit);
				return;
			}
		}
		if (Input.GetKeyDown(KeyCode.Return))
			if (cycledUnit != null) {
				SelectUnit(cycledUnit);
				return;
			}
			else
				UiSound.NotAllowed();
	}

	public void SelectUnit(Unit unit) {
		unit.view.selected.v = true;
		level.state.v = new PathSelectionState(level, unit);
	}
}

public class PathSelectionState : LevelState {

	public static Traverser traverser = new();
	public Unit unit;
	public List<Vector2Int> path;

	public PathSelectionState(Level level, Unit unit) : base(level) {
		this.unit = unit;
		Assert.IsTrue(unit.position.v != null);
		var position = (Vector2Int)unit.position.v;
		Assert.IsTrue(level.tiles.ContainsKey(position));
		traverser.Traverse(level.tiles.Keys, position, Cost);
	}

	public int? Cost(Vector2Int position, int length) {
		if (length >= Rules.MoveDistance(unit) ||
		    !level.TryGetTile(position, out var tile) ||
		    level.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
			return null;
		
		return Rules.MoveCost(unit, tile);
	}

	public override void Update() {
		if (Input.GetMouseButtonDown(Mouse.right)) {
			level.state.v = new SelectionState(level);
			unit.view.selected.v = false;
			return;
		}
		if (Input.GetMouseButtonDown(Mouse.left)) {

			if (Mouse.TryGetPosition(out var position) && traverser.IsReachable(position.RoundToInt())) {
				path = traverser.ReconstructPath(position.RoundToInt());
				level.state.v = new UnitMovementAnimationState(level, unit, path);
				return;
			}
			else
				UiSound.NotAllowed();
		}
	}

	public override void DrawGizmos() {
		foreach (var position in level.tiles.Keys)
			Handles.Label(position.ToVector3Int(), traverser.GetDistance(position).ToString(), new GUIStyle { normal = new GUIStyleState { textColor = Color.black } });
	}
}

public class UnitMovementAnimationState : LevelState {

	public Unit unit;
	public List<Vector2Int> path;
	public List<MovePath.Move> moves;
	public Vector2Int startPosition, startForward;

	public UnitMovementAnimationState(Level level, Unit unit, List<Vector2Int> path) : base(level) {

		this.unit = unit;
		this.path = path;

		startPosition = (Vector2Int)unit.position.v;
		startForward = unit.view.transform.forward.ToVector2().RoundToInt();

		moves = MovePath.From(path.Select(p => (Vector2)p).ToList(), startPosition, startForward);
		if (moves != null) {
			unit.view.walker.onComplete += GoToNextState;
			unit.view.walker.moves = moves;
			unit.view.walker.speed = GameSettings.Instance.unitSpeed;
			unit.view.walker.enabled = true;
		}
	}

	public override void Update() {
		if (moves == null)
			GoToNextState();
		else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetMouseButtonDown(Mouse.right))
			unit.view.walker.enabled = false;
	}

	public void GoToNextState() {
		unit.view.walker.enabled = false;
		if (moves != null) {
			unit.view.Position = path.Last();
			unit.view.Forward = moves.Last().forward;
		}
		level.state.v = new ActionSelectionState(level, unit, startPosition, startForward, path, moves);
	}

	public override void Dispose() {
		if (moves != null)
			unit.view.walker.onComplete -= GoToNextState;
	}
}

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
			level.state.v = new PathSelectionState(level, unit);
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