using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionState : GameState {

	public List<Unit> unitLoop;
	public int unitIndex = -1;

	public SelectionState(Level level) : base(level) {
		unitLoop = level.unitMap
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
						level.state.v = new PathSelectionState(level, view.unit);
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

public static class Mouse {
	public const int left = 0;
	public const int right = 1;
	public const int middle = 2;
}