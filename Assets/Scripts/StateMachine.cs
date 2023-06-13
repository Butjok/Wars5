using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class StateMachine {

    private Stack<(StateMachineState state, IEnumerator<StateChange> enumerator)> states = new();
    public int Count => states.Count;
    private readonly Stack<string> stateNames = new();
    public IEnumerable<string> StateNames => stateNames;

    public void Pop(int count = 1, bool all = false) {
        if (all)
            count = states.Count;
        for (var i = 0; i < count; i++) {
            states.Pop().state.Exit();
            stateNames.Pop();
        }
    }
    public void Push(StateMachineState state) {
        states.Push((state, state.Enter));
        var name = state.GetType().Name;
        stateNames.Push(name.EndsWith("State") ? name[..^5] : name);
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
    public T TryFind<T>() where T : StateMachineState {
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

public abstract class StateMachineState {

    public readonly StateMachine stateMachine;
    protected StateMachineState(StateMachine stateMachine) {
        this.stateMachine = stateMachine;
    }

    protected T FindState<T>() where T : StateMachineState {
        var state = stateMachine.TryFind<T>();
        Assert.IsNotNull(state);
        return state;
    }
    protected T FindObject<T>() where T : Object {
        var obj = Object.FindObjectOfType<T>();
        Assert.IsTrue(obj);
        return obj;
    }

    public virtual void Exit() { }

    public abstract IEnumerator<StateChange> Enter { get; }

    protected StateChange MoveCursor((object name, object argument) command) {

        var levelView = (stateMachine.TryFind<LevelSessionState>()?.level ?? stateMachine.TryFind<LevelEditorSessionState>().level).view;
        var cursorView = levelView.cursorView;

        switch (command) {
            case (CursorInteractor.Command.MouseEnter, _):
                if (levelView.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition))
                    cursorView.Position = mousePosition;
                break;

            case (CursorInteractor.Command.MouseExit, _):
                cursorView.Position = null;
                break;

            case (CursorInteractor.Command.MouseOver, _):
                if (levelView.cameraRig.camera.TryGetMousePosition(out mousePosition))
                    cursorView.Position = mousePosition;
                break;
        }
        
        return StateChange.none;
    }

    protected static StateChange HandleUnexpectedCommand(object command) {
        return StateChange.none;
    }
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