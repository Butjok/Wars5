using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class StateRunner : MonoBehaviour {

    public Stack<IEnumerator<StateChange>> states = new();
    public Stack<string> stateNames = new();
    public HashSet<IEnumerator<StateChange>> readyForInputStates = new();

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

            var ended = !state.MoveNext();

            if (ended) {
                readyForInputStates.Remove(states.Pop()); 
                stateNames.Pop();
            }
            else {
                var stateChange = state.Current;

                for (var i = 0; i < stateChange.popCount; i++) {
                    readyForInputStates.Remove(states.Pop()); 
                    stateNames.Pop();
                }

                if (stateChange.state != null)
                    PushState(stateChange.stateName, stateChange.state);
            }
        }
    }

    public void MarkReadyForInput() {
        var nonEmpty = states.TryPeek(out var state);
        Assert.IsTrue(nonEmpty);
        readyForInputStates.Add(state);
    }
    public bool IsInState(string stateName) {
        return stateNames.TryPeek(out var topStateName) && topStateName == stateName;
    }
    public bool IsReadyForInput() {
        return states.TryPeek(out var state) && readyForInputStates.Contains(state);
    }
}

abstract public class State {
    public abstract IEnumerator<StateChange> Run();
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
}

public struct StateChange {

    public static StateChange none = default;
    public static StateChange PopThenPush(int popCount, string stateName, IEnumerator<StateChange> state) => new(popCount, stateName, state);
    public static StateChange Pop(int count = 1) => PopThenPush(count, default, null);
    public static StateChange Push(string stateName, IEnumerator<StateChange> state) => PopThenPush(0, stateName, state);
    public static StateChange ReplaceWith(string stateName, IEnumerator<StateChange> state) => PopThenPush(1, stateName, state);

    public readonly int popCount;
    public IEnumerator<StateChange> state;
    public readonly string stateName;

    private StateChange(int popCount, string stateName, IEnumerator<StateChange> state = null) {
        if (state != null)
            Assert.IsNotNull(stateName);
        this.stateName = stateName;
        this.state = state;
        this.popCount = popCount;
    }
}