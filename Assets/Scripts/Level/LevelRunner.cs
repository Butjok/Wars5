using UnityEngine;

public class LevelRunner : StateMachineRunner {
	public override void OnGUI() {
		GUILayout.Label(sm.State?.ToString());
	}
}