using System;

public abstract class SubstateMachine : StateMachine, IState {

	public bool Started { get; set; }
	public bool Disposed { get; set; }

	public virtual void Start() { }
	public virtual void Update() { }
	public virtual void DrawGUI() { }
	public virtual void DrawGizmos() { }
	public virtual void DrawGizmosSelected() { }
	public virtual void OnBeforePush() {
		Running = false;
	}
	public virtual void OnAfterPop() {
		Running = true;
	}

	protected SubstateMachine(Type runnerType = null, string name = null) : base(runnerType, name) { }
}