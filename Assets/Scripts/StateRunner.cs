using System;
using System.Collections;
using System.Collections.Generic;
using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class StateRunner : MonoBehaviour {

    private Stack<IEnumerator<StateChange>> states = new();
    private Stack<string> stateNames = new();
    public IEnumerable<string> StateNames => stateNames;

    public void PushState(string stateName, IEnumerator<StateChange> state) {
        states.Push(state);
        stateNames.Push(stateName);
    }
    public void ClearStates() {
        states.Clear();
        stateNames.Clear();
    }

    protected virtual void Update() {
        if (states.TryPeek(out var state)) {
            if (state.MoveNext()) {

                var stateChange = state.Current;

                for (var i = 0; i < stateChange.popCount; i++) {
                    states.Pop();
                    stateNames.Pop();
                }

                if (stateChange.state != null) {
                    Assert.IsNotNull(stateChange.stateName);
                    states.Push(stateChange.state);
                    stateNames.Push(stateChange.stateName);
                }
            }
            else {
                states.Pop();
                stateNames.Pop();
            }
        }
    }
}

public struct StateChange {

    public static IEnumerator<StateChange> WaitForSeconds(float delay) {
        var startTime = Time.time;
        while (Time.time < startTime + delay)
            yield return none;
    }
    public static IEnumerator<StateChange> WaitForCompletion(IEnumerator iEnumerator) {
        while (iEnumerator.MoveNext())
            yield return none;
    }
    public static IEnumerator<StateChange> WaitForCompletion(Tween tween) {
        while (tween.IsActive() && !tween.IsComplete())
            yield return none;
    }
    
    public static StateChange none = default;
    public static StateChange PopThenPush(int popCount, string stateName, IEnumerator<StateChange> state) => new(popCount, stateName, state);
    public static StateChange Pop(int count=1) => PopThenPush(count, null, null);
    public static StateChange Push(string stateName, IEnumerator<StateChange> state) => PopThenPush(0, stateName, state);
    public static StateChange ReplaceWith(string stateName, IEnumerator<StateChange> state) => PopThenPush(1, stateName, state);

    public readonly int popCount;
    public IEnumerator<StateChange> state;
    public readonly string stateName;

    private StateChange(int popCount, string stateName, IEnumerator<StateChange> state = null) {
        this.stateName = stateName;
        this.state = state;
        this.popCount = popCount;
    }
}