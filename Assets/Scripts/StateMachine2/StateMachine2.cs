using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class StateMachine2<T> : MonoBehaviour where T : StateMachine2<T> {

    public State2<T> state;
    public string stateName;
    public Stack<State2<T>> pausedStates = new();

    public void Update() {
        state?.Update();
    }

    protected void RunWith(State2<T> initialState) {

        Assert.IsNull(state);

        Assert.IsFalse(initialState.started);
        Assert.IsFalse(initialState.disposed);
        Assert.IsFalse(initialState.paused);

        state = initialState;
        UpdateDebugName();

        state.Start();
        state.started = true;
    }

    public void UpdateDebugName() {
#if DEBUG
        name = GetType().Name + ": " + state;
#endif
    }
}

public abstract class State2<T> : IDisposable where T : StateMachine2<T> {

    public T parent;
    public bool started;
    public bool disposed;
    public bool paused;

    protected State2(T parent) {
        Assert.IsTrue(parent);
        this.parent = parent;
    }

    public virtual void Start() { }
    public virtual void Dispose() { }
    public virtual void Update() { }
    public virtual void OnPause() { }
    public virtual void OnUnpause() { }

    public void ChangeTo(State2<T> state) {

        Assert.IsTrue(started);
        Assert.IsFalse(disposed);
        Assert.IsFalse(paused);

        Assert.IsFalse(state.started);
        Assert.IsFalse(state.disposed);
        Assert.IsFalse(state.paused);

        Dispose();
        disposed = true;

        parent.state = state;
        parent.UpdateDebugName();

        state.Start();
        state.started = true;
    }

    public void Pause() {

        Assert.IsTrue(started);
        Assert.IsFalse(disposed);
        Assert.IsFalse(paused);

        OnPause();
        paused = true;

        parent.pausedStates.Push(this);
        parent.state = null;
        parent.UpdateDebugName();
    }

    public void PauseTo(State2<T> state) {

        Assert.IsFalse(state.started);
        Assert.IsFalse(state.disposed);
        Assert.IsFalse(state.paused);

        Pause();

        parent.state = state;
        parent.UpdateDebugName();

        state.Start();
        state.started = true;
    }

    public void UnpauseLastState() {

        Assert.IsTrue(started);
        Assert.IsFalse(disposed);
        Assert.IsFalse(paused);

        var state = parent.pausedStates.Pop();

        Assert.IsTrue(state.started);
        Assert.IsFalse(state.disposed);
        Assert.IsTrue(state.paused);

        parent.state = state;
        parent.UpdateDebugName();

        state.OnUnpause();
        state.paused = false;
    }
}