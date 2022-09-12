using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitMovementAnimationState : State2<Game2> {

	public Unit unit;
	public MovePath path;
	public Vector2Int  startForward;

	public UnitMovementAnimationState(Game2 parent, Unit unit, MovePath path) : base(parent) {

		this.unit = unit;
		this.path = path;

		startForward = unit.view.transform.forward.ToVector2().RoundToInt();

		if (path.moves.Count > 0) {
			unit.view.walker.onComplete += GoToNextState;
			unit.view.walker.moves = path.moves;
			unit.view.walker.speed = GameSettings.Instance.unitSpeed;
			unit.view.walker.enabled = true;
		}
	}

	public override void Start() {
		base.Start();
		
		if (path.moves.Count == 0)
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
		if (path.moves.Count != 0) {
			unit.view.Position = path.positions.Last();
			unit.view.Forward = path.moves.Last().forward;
		}
		ChangeTo(new ActionSelectionState(parent, unit,  startForward, path));
	}

	public override void Dispose() {
		base.Dispose();
		
		if (path.moves.Count >0)
			unit.view.walker.onComplete -= GoToNextState;
	}
}