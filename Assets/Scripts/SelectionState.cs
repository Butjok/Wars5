using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionState : GameState {

	public List<Unit> unitLoop;
	public int unitIndex = -1;

	public SelectionState(Game game) : base(game) {
		unitLoop = game.unitMap
			.Where(unit => unit.player == CurrentPlayer)
			.OrderBy(unit => Vector3.Distance(CameraRig.Instance.transform.position, unit.view.transform.position))
			.ToList();
	}

	public override void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (unitLoop.Count > 0) {
				unitIndex = (unitIndex + 1) % unitLoop.Count;
				var unit = unitLoop[unitIndex];
				CameraRig.Instance.Jump(unit.view.transform.position);
			}
			else
				UiSound.Notallowed();
		}
		if (Input.GetMouseButtonDown(Mouse.left)) {
			if (Camera.main) {
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				var mask = Masks.selectable;
				var hits = Physics.RaycastAll(ray, float.MaxValue, mask);
				foreach (var hit in hits.OrderBy(hit => hit.distance)) {
					var view = hit.transform.GetComponentInParent<UnitView>();
					if (view) {
						game.state.v = new PathSelectionState(game, view.unit);
						break;
					}
				}
			}
		}
	}
	public override void DrawGUI() {
		GUILayout.Label("Nothing is selected");
	}
}
public static class Masks {
	public static int selectable = 1 << LayerMask.NameToLayer("Selectable");
}

public class PathSelectionState : GameState {
	public Unit unit;
	public PathSelectionState(Game game, Unit unit) : base(game) {
		this.unit = unit;
		unit.selected.v = true;
	}
	public override void Dispose() {
		unit.selected.v = false;
	}
	public override void Update() {
		if (Input.GetMouseButtonDown(Mouse.right)) {
			game.state.v = new SelectionState(game);
			return;
		}
	}
}

public static class Mouse {
	public const int left = 0;
	public const int right = 1;
	public const int middle = 2;
}