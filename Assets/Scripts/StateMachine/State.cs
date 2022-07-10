using System;

public interface IState : IDisposable {
	public StateMachine Sm { get; }
	bool Started { get; set; }
	bool Disposed { get; set; }
	void Start();
	void Update();
	void DrawGUI();
	void DrawGizmos();
	void DrawGizmosSelected();
	void OnBeforePush();
	void OnAfterPop();
}

public abstract class State : IState {
	
	protected State(StateMachine sm) {
		Sm = sm;
	}

	public StateMachine Sm { get; }
	public bool Started { get; set; }
	public bool Disposed { get; set; }

	public virtual void Start() { }
	public virtual void Update() { }
	public virtual void DrawGUI() { }
	public virtual void Dispose() { }
	public virtual void DrawGizmos() { }
	public virtual void DrawGizmosSelected() { }
	public virtual void OnBeforePush() { }
	public virtual void OnAfterPop() { }
}

public abstract class StateMachineState : StateMachine, IState {

	public StateMachine Sm { get; }
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

	protected StateMachineState(StateMachine sm, string name = null) : base( name) {
		Sm = sm;
	}
}