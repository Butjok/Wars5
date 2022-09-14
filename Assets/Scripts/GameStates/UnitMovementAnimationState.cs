using System.Collections;
using System.Linq;
using UnityEngine;

public static class UnitMovementAnimationState {

	public static IEnumerator New(Game2 game, Unit unit, MovePath path) {

		var startForward = unit.view.transform.forward.ToVector2().RoundToInt();
		var play = true;

		if (path.moves.Count > 0) {
			unit.view.walker.onComplete = () => play = false;
			unit.view.walker.moves = path.moves;
			unit.view.walker.speed = game.settings.unitSpeed;
			unit.view.walker.enabled = true;
		}
		else
			play = false;

		while (play) {
			yield return null;

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

		unit.view.walker.enabled = false;
		if (path.moves.Count != 0) {
			unit.view.Position = path.positions.Last();
			unit.view.Forward = path.moves.Last().forward;
		}
		yield return ActionSelectionState.New(game, unit, startForward, path);
	}
}