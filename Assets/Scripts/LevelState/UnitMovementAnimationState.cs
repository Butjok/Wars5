using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

	public override void Start() {
		base.Start();
		
		if (moves == null)
			GoToNextState();
	}

	public override void Update() {
		base.Update();
		
		if (Input.GetMouseButtonDown(Mouse.left) || Input.GetMouseButtonDown(Mouse.right)) {
			unit.view.walker.enabled = false;

			// manually update wheels and body
			foreach (var wheel in unit.view.wheels)
				wheel.Update();
			/*foreach (var piston in unit.view.wheelPistons)
				piston.Clear();*/
			if (unit.view.body)
				unit.view.body.Update();
		}
	}

	public void GoToNextState() {
		unit.view.walker.enabled = false;
		if (moves != null) {
			unit.view.Position = path.Last();
			unit.view.Forward = moves.Last().forward;
		}
		level.State = new ActionSelectionState(level, unit, startPosition, startForward, path, moves);
	}

	public override void Dispose() {
		base.Dispose();
		
		if (moves != null)
			unit.view.walker.onComplete -= GoToNextState;
	}
}