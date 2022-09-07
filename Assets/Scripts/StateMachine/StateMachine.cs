using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

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
	public StateMachineRunner Runner => runner;
	public bool Running {
		get => runner.enabled;
		set => runner.enabled = value;
	}

	private Stack<IState> stack = new();

	public StateMachine(string name = null) {
		var go = new GameObject(name);
		Object.DontDestroyOnLoad(go);
		runner = go.AddComponent<StateMachineRunner>();
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