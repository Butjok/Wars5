using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SelectionState {

	public static IEnumerator New(Game2 game, bool turnStart = false) {

		var unmovedUnits = game.units.Values
			.Where(unit => unit.player == game.CurrentPlayer && !unit.moved.v)
			.OrderBy(unit => Vector3.Distance(CameraRig.Instance.transform.position, unit.view.center.position))
			.ToList();

		var unitIndex = -1;
		Unit cycledUnit = null;

		if (turnStart) {

			var (controlFlow, nextState) = game.levelLogic.OnTurnStart(game);
			if (nextState != null)
				yield return nextState;
			if (controlFlow == ControlFlow.Replace)
				yield break;

			yield return TurnStartAnimationState.New(game);
		}

		CursorView.Instance.Visible = true;
		
		while (true) {
			yield return null;

			if (game.CurrentPlayer.IsAi || Input.GetKeyDown(KeyCode.KeypadEnter)) {

				if (game.CurrentPlayer.IsAi)
					game.CurrentPlayer.bestAction = game.CurrentPlayer.FindAction();

				var aiAction = game.CurrentPlayer.IsAi ? game.CurrentPlayer.bestAction : null;
				if (game.CurrentPlayer.IsAi && aiAction != null) {
					
					CursorView.Instance.Visible = false;
					yield return UnitMovementAnimationState.New(game, aiAction.unit, aiAction.path);
					yield break;
				}

				if (!game.CurrentPlayer.IsAi || aiAction == null) {
					foreach (var unit in game.units.Values)
						unit.moved.v = false;
					if (game.Turn is not { } integer)
						throw new Exception();
					game.Turn = integer + 1;
					
					CursorView.Instance.Visible = false;

					var (controlFlow, nextState) = game.levelLogic.OnTurnEnd(game);
					if (nextState != null)
						yield return nextState;
					if (controlFlow == ControlFlow.Replace)
						yield break;
					
					yield return New(game, true);
					yield break;
				}
			}
			
			else {
				if (cycledUnit != null && Camera.main) {
					var worldPosition = cycledUnit.view.center.position;
					var screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
					if (screenPosition.x is < 0 or > 1 || screenPosition.y is < 0 or > 1)
						cycledUnit = null;
				}
				if (Input.GetKeyDown(KeyCode.Tab)) {
					if (unmovedUnits.Count > 0) {
						unitIndex = (unitIndex + 1) % unmovedUnits.Count;
						var next = unmovedUnits[unitIndex];
						CameraRig.Instance.Jump(next.view.center.position.ToVector2());
						cycledUnit = next;
					}
					else
						UiSound.Instance.notAllowed.Play();
				}
				else if (Input.GetMouseButtonDown(Mouse.left) &&
				         Mouse.TryGetPosition(out Vector2Int position) &&
				         game.TryGetUnit(position, out var unit)) {

					if (unit.player != game.CurrentPlayer || unit.moved.v)
						UiSound.Instance.notAllowed.Play();
					else {
						unit.view.Selected = true;
						yield return PathSelectionState.New(game, unit);
						yield break;
					}
				}
				else if (Input.GetKeyDown(KeyCode.Return))
					if (cycledUnit != null) {
						cycledUnit.view.Selected = true;
						yield return PathSelectionState.New(game, cycledUnit);
						yield break;
					}
					else
						UiSound.Instance.notAllowed.Play();

				else if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftShift)) {
					CursorView.Instance.Visible = false;
					yield return VictoryState.New(game);
					yield break;
				}

				else if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift)) {
					CursorView.Instance.Visible = false;
					yield return DefeatState.New(game);
					yield break;
				}
			}
		}
	}
}