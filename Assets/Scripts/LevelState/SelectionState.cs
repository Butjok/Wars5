using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionState : State2<Game2> {

	public List<Unit> unitLoop;
	public int unitIndex = -1;
	public Unit cycledUnit;

	public SelectionState( Game2 parent) : base(parent) {
		unitLoop = parent.units.Values
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
			if (unitLoop.Count > 0) {
				unitIndex = (unitIndex + 1) % unitLoop.Count;
				var next = unitLoop[unitIndex];
				CameraRig.Instance.Jump(next.view.center.position);
				cycledUnit = next;
			}
			else
				Sounds.NotAllowed.Play();
		}
		if (Input.GetMouseButtonDown(Mouse.left) &&
		    Mouse.TryGetPosition(out Vector2Int position) &&
		    parent.TryGetUnit(position, out var unit)) {

			if (unit.moved.v)
				Sounds.NotAllowed.Play();
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
				Sounds.NotAllowed.Play();
	}

	public void SelectUnit(Unit unit) {
		unit.view.selected.v = true;
		ChangeTo(new PathSelectionState(parent, unit));
	}
}