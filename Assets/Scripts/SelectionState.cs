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
	}
	public override void DrawGUI() {
		GUILayout.Label("Nothing is selected");
	}
}