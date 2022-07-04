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
		unitLoop = level.unitMap
			.Where(unit => unit.player == CurrentPlayer && !unit.moved.v)
			.OrderBy(unit => Vector3.Distance(CameraRig.Instance.transform.position, unit.view.transform.position))
			.ToList();
	}

	public override void Update() {
		if(cycledUnit != null && Camera.main) {
			var worldPosition = cycledUnit.view.transform.position;
			var screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
			if(screenPosition.x < 0 || screenPosition.x > 1 || screenPosition.y < 0 || screenPosition.y > 1)
				cycledUnit = null;
		}
		if(Input.GetKeyDown(KeyCode.Tab)) {
			if(unitLoop.Count > 0) {
				unitIndex = (unitIndex + 1) % unitLoop.Count;
				var unit = unitLoop[unitIndex];
				CameraRig.Instance.Jump(unit.view.transform.position);
				cycledUnit = unit;
			}
			else
				UiSound.NotAllowed();
		}
		if(Input.GetMouseButtonDown(Mouse.left)) {
			var unit = Mouse.Unit;
			if(unit != null) {
				if(unit.moved.v)
					UiSound.NotAllowed();
				else {
					level.state.v = new PathSelectionState(level, unit);
					return;
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Return))
			if(cycledUnit != null) {
				level.state.v = new PathSelectionState(level, cycledUnit);
				return;
			}
			else
				UiSound.NotAllowed();
	}
}

public class PathSelectionState : LevelState {

	static public Traverser traverser = new();
	public Unit unit;
	public List<Vector2Int> path;

	public PathSelectionState(Level level, Unit unit) : base(level) {
		this.unit = unit;
		unit.selected.v = true;
		Assert.IsTrue(unit.position.v is { } position);
		Assert.IsTrue(level.tileAt.ContainsKey((Vector2Int)unit.position.v));
		traverser.Traverse(level.tileAt.Keys, (Vector2Int)unit.position.v, (position, length) => length < 3 ? 1 : null);
	}

	public override void Dispose() {
		unit.selected.v = false;
	}

	public override void Update() {
		if(Input.GetMouseButtonDown(Mouse.right)) {
			level.state.v = new SelectionState(level);
			return;
		}
		if(Input.GetMouseButtonDown(Mouse.left)) {
			var tile = Mouse.Tile(level);
			if(tile != null && traverser.infos.TryGetValue(tile.position, out var info) && info.distance < int.MaxValue) {
				path = traverser.ReconstructPath(tile.position);
				level.state.v = new UnitMovementAnimationState(level, unit, path);
				return;
			}
			else
				UiSound.NotAllowed();
		}
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
		startForward = unit.forward.v;

		moves = MovePath.From(path.Select(p => (Vector2)p).ToList(), startPosition, startForward);
		if(moves != null) {
			unit.view.walker.onCompleted += GoToNextState;
			unit.view.walker.moves = moves;
			unit.view.walker.enabled = true;
		}
	}

	public override void Update() {
		if(moves == null)
			GoToNextState();
	}

	public void GoToNextState() {
		if(moves != null) {
			unit.view.position.v = path.Last();
			unit.view.forward.v = moves.Last().forward;
		}
		level.state.v = new ActionSelectionState(level, unit, startPosition, startForward, path, moves);
	}

	public override void Dispose() {
		if(moves != null)
			unit.view.walker.onCompleted -= GoToNextState;
	}
}

public class ActionSelectionState : LevelState {

	public Unit unit;
	public List<Vector2Int> path;
	public List<MovePath.Move> moves;
	public Vector2Int startPosition, startForward;

	public ActionSelectionState(Level level, Unit unit, Vector2Int startPosition, Vector2Int startForward, List<Vector2Int> path, List<MovePath.Move> moves) : base(level) {
		this.unit = unit;
		this.startPosition = startPosition;
		this.startForward = startForward;
		this.path = path;
		this.moves = moves;
	}

	public override void Update() {

		if(Input.GetMouseButtonDown(Mouse.right)) {
			unit.view.position.v = startPosition;
			unit.view.forward.v = startForward;
			level.state.v = new PathSelectionState(level, unit);
			return;
		}

		if(unit.view.turret) {
			var target = Mouse.Unit;
			if(target != null && unit.CanAttack(target)) {
				unit.view.turret.ballisticComputer.target = target.view.transform;
				unit.view.turret.aim = true;
			}
			else
				unit.view.turret.aim = false;
		}
	}

	public override void Dispose() {
		if(unit.view.turret)
			unit.view.turret.aim = false;
	}
}

public static class UnitSettings {
	public static bool CanAttack(this Unit attacker, Unit target) {
		return (attacker.player.team & target.player.team) == 0;
	}
}