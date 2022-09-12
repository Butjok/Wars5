using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionState : State2<Game2> {

	public List<Unit> unmovedUnits;
	public int unitIndex = -1;
	public Unit cycledUnit;

	public SelectionState(Game2 parent) : base(parent) {
		unmovedUnits = parent.units.Values
			.Where(unit => unit.player == parent.CurrentPlayer && !unit.moved.v)
			.OrderBy(unit => Vector3.Distance(CameraRig.Instance.transform.position, unit.view.center.position))
			.ToList();
	}

	public override void Update() {
		base.Update();

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
				CameraRig.Instance.Jump(next.view.center.position);
				cycledUnit = next;
			}
			else
				UiSound.Instance.notAllowed.Play();
		}
		else if (Input.GetMouseButtonDown(Mouse.left) &&
		         Mouse.TryGetPosition(out Vector2Int position) &&
		         parent.TryGetUnit(position, out var unit)) {

			if (unit.player != parent.CurrentPlayer || unit.moved.v)
				UiSound.Instance.notAllowed.Play();
			else {
				SelectUnit(unit);
				return;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Return))
			if (cycledUnit != null) {
				SelectUnit(cycledUnit);
				return;
			}
			else
				UiSound.Instance.notAllowed.Play();

		else if (Input.GetKeyDown(KeyCode.F12) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))) {
			var nextState = parent.levelLogic.OnTurnEnd();
			if (nextState != null)
				ChangeTo(nextState);
			else {
				foreach (var unit2 in parent.units.Values)
					unit2.moved.v = false;
				if (parent.Turn is not { } turn)
					throw new Exception();
				parent.Turn = turn + 1;
				ChangeTo(new SelectionState(parent));
			}
		}
	}

	public void SelectUnit(Unit unit) {
		unit.view.selected.v = true;
		ChangeTo(new PathSelectionState(parent, unit));
	}
}