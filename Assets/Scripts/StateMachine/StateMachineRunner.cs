using UnityEngine;

public class StateMachineRunner : MonoBehaviour {
	public StateMachine sm;
	public virtual void Update() {
		if (enabled)
			sm?.State?.Update();
	}
	public virtual void OnGUI() {
		if (enabled)
			sm?.State?.DrawGUI();
	}
	public virtual void OnDrawGizmos() {
		if (enabled)
			sm?.State?.DrawGizmos();
	}
	public virtual void OnDrawGizmosSelected() {
		if (enabled)
			sm?.State?.DrawGizmosSelected();
	}
}