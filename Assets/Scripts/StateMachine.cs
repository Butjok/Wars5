using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class StateMachine {

    public abstract class State : IDisposable {
        protected readonly StateMachine stateMachine;
        protected State(StateMachine stateMachine) {
            this.stateMachine = stateMachine;
        }
        public virtual void Dispose() { }
        public abstract IEnumerator<StateChange> Sequence { get; }
    }

    private Stack<(State state, IEnumerator<StateChange> enumerator)> states = new();
    public int Count => states.Count;

    public void Pop(int count = 1, bool all = false) {
        if (all)
            count = states.Count;
        for (var i = 0; i < count; i++)
            states.Pop().state.Dispose();
    }
    public void Push(State state) {
        states.Push((state, state.Sequence));
    }
    public T TryPeek<T>() where T : State {
        if (states.TryPeek(out var state) && state.state is T castedState)
            return castedState;
        return null;
    }
    public bool TryPeek(out State result) {
        result = TryPeek<State>();
        return result != null;
    }
    public bool IsInState<T>() where T : State {
        return TryPeek<T>() != null;
    }
    public T TryFind<T>() where T: State {
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

public struct StateChange {

    public int popCount;
    public StateMachine.State state;

    public static StateChange none = default;
    public static StateChange Pop(int count = 1) => new() { popCount = count };

    public static StateChange PopThenPush(int popCount, StateMachine.State state) => new() {
        popCount = popCount,
        state = state
    };
    public static StateChange Push(StateMachine.State state) => PopThenPush(0, state);
    public static StateChange ReplaceWith(StateMachine.State state) => PopThenPush(1, state);
}