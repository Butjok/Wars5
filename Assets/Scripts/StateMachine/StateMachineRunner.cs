using UnityEngine;

public class StateMachineRunner : MonoBehaviour {
	public StateMachine sm;
	public virtual void Update() {
		sm?.State?.Update();
	}
	public virtual void OnGUI() {
		sm?.State?.DrawGUI();
	}
	public virtual void OnDrawGizmos() {
		sm?.State?.DrawGizmos();
	}
	public virtual void OnDrawGizmosSelected() {
		sm?.State?.DrawGizmosSelected();
	}
}