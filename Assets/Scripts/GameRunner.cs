using UnityEngine;

public class GameRunner : StateMachineRunner {
	public override void Update() {
		base.Update();
		if (Input.GetKeyDown(KeyCode.Escape))
			if (sm.State is Level)
				sm.Push(new InGameOverlayMenu());
			else
				sm.Pop();
	}
}