using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public interface IState : IDisposable {
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

public class StateMachine : IDisposable {

	private IState state;
	public IState State {
		get => state;
		set {
			if (state != null) {
				Assert.IsFalse(state.Disposed);
				state.Dispose();
				state.Disposed = true;
			}
			state = value;
			if (!state.Started) {
				state.Start();
				state.Started = true;
			}
		}
	}

	private StateMachineRunner runner;
	public bool Running {
		get => runner.enabled;
		set => runner.enabled = value;
	}

	private Stack<IState> stack = new();

	public StateMachine(Type runnerType = null, string name = null) {
		runnerType ??= typeof(StateMachineRunner);
		var go = new GameObject(name ?? runnerType.Name);
		Object.DontDestroyOnLoad(go);
		runner = (StateMachineRunner)go.AddComponent(runnerType);
		runner.sm = this;
	}

	public virtual void Dispose() {
		if (state != null) {
			Assert.IsFalse(state.Disposed);
			state.Dispose();
			state.Disposed = true;
		}
		if (runner && runner.gameObject) {
			Object.Destroy(runner.gameObject);
			runner = null;
		}
	}

	public void Push(IState newState) {
		if (state != null) {
			state.OnBeforePush();
			stack.Push(state);
		}
		state = newState;
		if (!state.Started) {
			state.Start();
			state.Started = true;
		}
	}
	public void Pop() {
		Assert.IsFalse(state.Disposed);
		state.Dispose();
		state.Disposed = true;
		state = stack.Pop();
		state.OnAfterPop();
	}
}