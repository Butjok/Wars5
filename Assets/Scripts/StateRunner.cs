using System;
using System.Collections.Generic;
using UnityEngine;

public class StateRunner : MonoBehaviour {

    public Stack<IEnumerator<StateChange>> states = new();

    protected virtual void Update() {
        if (states.TryPeek(out var state)) {
            if (state.MoveNext()) {

                var stateChange = state.Current;
                switch (stateChange.type) {

                    case StateChange.Type.Ignore:
                        break;

                    case StateChange.Type.Pop:
                        states.Pop();
                        break;

                    case StateChange.Type.Push:
                        states.Push(stateChange.state);
                        break;

                    case StateChange.Type.Replace:
                        states.Pop();
                        states.Push(stateChange.state);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(stateChange.type.ToString());
                }
            }
            else
                states.Pop();
        }
    }
}

public struct StateChange {

    public enum Type { Ignore, Pop, Push, Replace }

    public static StateChange none = default;
    public static StateChange pop = new(Type.Pop);
    public static StateChange Push(IEnumerator<StateChange> state) => new(Type.Push, state);
    public static StateChange Replace(IEnumerator<StateChange> state) => new(Type.Replace, state);

    public readonly Type type;
    public IEnumerator<StateChange> state;

    public StateChange(Type type, IEnumerator<StateChange> state = null) {
        this.type = type;
        this.state = state;
    }
}