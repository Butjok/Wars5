using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
		level.State = new PathSelectionState(level, unit);
	}
}