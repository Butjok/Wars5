using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class StateMachine {

    private Stack<(StateMachineState state, IEnumerator<StateChange> enumerator)> states = new();
    public int Count => states.Count;

    public void Pop(int count = 1, bool all = false) {
        if (all)
            count = states.Count;
        for (var i = 0; i < count; i++)
            states.Pop().state.Dispose();
    }
    public void Push(StateMachineState state) {
        states.Push((state, state.Sequence));
    }
    public T TryPeek<T>() where T : StateMachineState {
        if (states.TryPeek(out var state) && state.state is T castedState)
            return castedState;
        return null;
    }
    public bool TryPeek(out StateMachineState result) {
        result = TryPeek<StateMachineState>();
        return result != null;
    }
    public bool IsInState<T>() where T : StateMachineState {
        return TryPeek<T>() != null;
    }
    public T TryFind<T>() where T: StateMachineState {
        foreach (var state in states)
            if (state.state is T castedState)
                return castedState;
        return null;
    }

    public const int maxDepth = 100;
    public void Tick() {

        var depth = 0;

        while (true) {
            Assert.IsTrue(depth < maxDepth);

            if (!states.TryPeek(out var state))
                return;

            if (state.enumerator.MoveNext()) {
                var stateChange = state.enumerator.Current;
                for (var i = 0; i < stateChange.popCount; i++)
                    Pop();
                if (stateChange.state != null)
                    Push(stateChange.state);

                if (stateChange.popCount != 0 || stateChange.state != null) {
                    depth++;
                    continue;
                }
            }
            else {
                Pop();
                depth++;
                continue;
            }
            break;
        }
    }
}

public abstract class StateMachineState : IDisposable {
    protected readonly StateMachine stateMachine;
    protected StateMachineState(StateMachine stateMachine) {
        this.stateMachine = stateMachine;
    }
    public virtual void Dispose() { }
    public abstract IEnumerator<StateChange> Sequence { get; }
}

public struct StateChange {

    public int popCount;
    public StateMachineState state;

    public static StateChange none = default;
    public static StateChange Pop(int count = 1) => new() { popCount = count };

    public static StateChange PopThenPush(int popCount, StateMachineState state) => new() {
        popCount = popCount,
        state = state
    };
    public static StateChange Push(StateMachineState state) => PopThenPush(0, state);
    public static StateChange ReplaceWith(StateMachineState state) => PopThenPush(1, state);
}