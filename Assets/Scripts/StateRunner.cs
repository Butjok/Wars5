using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class StateRunner : MonoBehaviour {

    public Stack<IEnumerator<StateChange>> states = new();
    public Stack<string> stateNames = new();
    public Dictionary<IEnumerator<StateChange>, IDisposableState> disposableStates = new();

    public void PushState(string stateName, IEnumerator<StateChange> state) {
        states.Push(state);
        stateNames.Push(stateName);
    }
    public void PushState(IDisposableState disposableState) {
        var enumerator = disposableState.Run;
        states.Push(enumerator);
        stateNames.Push(disposableState.GetType().Name);
        disposableStates.Add(enumerator, disposableState);
    }
    public void ClearStates() {
        states.Clear();
        stateNames.Clear();
    }

    protected virtual void Update() {

        void Pop() {
            var state = states.Pop();
            stateNames.Pop();
            if (disposableStates.TryGetValue(state, out var disposableState)) {
                disposableState.Dispose();
                disposableStates.Remove(state);
            }
        }

        if (!states.TryPeek(out var state))
            return;

        if (state.MoveNext()) {
            var stateChange = state.Current;
            for (var i = 0; i < stateChange.popCount; i++)
                Pop();
            if (stateChange.state != null)
                PushState(stateChange.stateName, stateChange.state);
            if (stateChange.disposableState != null)
                PushState(stateChange.disposableState);
        }
        else
            Pop();
    }
}

public static class Wait {
    public static IEnumerator<StateChange> ForSeconds(float delay) {
        var startTime = Time.time;
        while (Time.time < startTime + delay)
            yield return StateChange.none;
    }
    public static IEnumerator<StateChange> ForCompletion(IEnumerator iEnumerator) {
        while (iEnumerator.MoveNext())
            yield return StateChange.none;
    }
    public static IEnumerator<StateChange> ForCompletion(Tween tween) {
        while (tween.IsActive() && !tween.IsComplete())
            yield return StateChange.none;
    }
    public class ForSpaceKeyDown : IDisposableState {
        public void Dispose() { }
        public IEnumerator<StateChange> Run {
            get {
                while (!Input.GetKeyDown(KeyCode.Space))
                    yield return StateChange.none;
                yield return StateChange.none;
            }
        }
    }
}

public interface IDisposableState : IDisposable {
    IEnumerator<StateChange> Run { get; }
}

public struct StateChange {

    public static StateChange none = default;
    public static StateChange Pop(int count = 1) => new(count, null);

    public static StateChange PopThenPush(int popCount, string stateName, IEnumerator<StateChange> state) => new(popCount, stateName, state);
    public static StateChange Push(string stateName, IEnumerator<StateChange> state) => PopThenPush(0, stateName, state);
    public static StateChange ReplaceWith(string stateName, IEnumerator<StateChange> state) => PopThenPush(1, stateName, state);

    public static StateChange PopThenPush(int popCount, IDisposableState state) => new(popCount, null, null, state);
    public static StateChange Push(IDisposableState state) => PopThenPush(0, state);
    public static StateChange ReplaceWith(IDisposableState state) => PopThenPush(1, state);

    public readonly int popCount;
    public IEnumerator<StateChange> state;
    public IDisposableState disposableState;
    public readonly string stateName;

    private StateChange(int popCount, string stateName, IEnumerator<StateChange> state = null, IDisposableState disposableState = null) {
        if (state != null)
            Assert.IsNotNull(stateName);
        this.stateName = stateName;
        this.state = state;
        this.popCount = popCount;
        this.disposableState = disposableState;
    }
}