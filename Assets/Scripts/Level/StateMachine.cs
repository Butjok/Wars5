using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public interface IState : IDisposable {
	bool Started { get; }
	bool Disposed { get; }
	void Start();
	void Update();
	void DrawGUI();
	void DrawGizmos();
	void DrawGizmosSelected();
	void OnBeforePush();
	void OnAfterPop();
}

public abstract class State : IState {

	public bool Started { get; set; } = false;
	public bool Disposed { get; } = false;

	public virtual void Start() {
		Started = true;
	}
	public virtual void Update() { }
	public virtual void DrawGUI() { }
	public virtual void Dispose() { }
	public virtual void DrawGizmos() { }
	public virtual void DrawGizmosSelected() { }
	public virtual void OnBeforePush() { }
	public virtual void OnAfterPop() { }
}

public abstract class SubstateMachine : StateMachine, IState {

	public bool Started { get; set; } = false;
	public bool Disposed { get; } = false;

	public void Start() {
		Started = true;
	}
	public void Update() { }
	public void DrawGUI() { }
	public void DrawGizmos() { }
	public void DrawGizmosSelected() { }
	public void OnBeforePush() {
		Running = false;
	}
	public void OnAfterPop() {
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
			}
			state = value;
			if (!state.Started)
				state.Start();
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
		if (!state.Started)
			state.Start();
	}
	public void Pop() {
		Assert.IsFalse(state.Disposed);
		state.Dispose();
		state = stack.Pop();
		state.OnAfterPop();
	}
}