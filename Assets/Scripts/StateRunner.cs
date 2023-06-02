using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class StateRunner : MonoBehaviour {

    private static StateRunner instance;
    public static StateRunner Instance {
        get {
            if (!instance) {
                var go = new GameObject(nameof(StateRunner));
                DontDestroyOnLoad(go);
                instance = go.AddComponent<StateRunner>();

                var commandLineGui = go.AddComponent<CommandLineGUI>();
                commandLineGui.assemblies = new List<string> { "CommandLine", "Wars", "Stable" };
                commandLineGui.guiSkin = DefaultGuiSkin.TryGet;
                commandLineGui.Theme = "Default";
                commandLineGui.depth = -2000;
                commandLineGui.FetchCommands();
            }
            return instance;
        }
    }

    public Stack<IEnumerator<StateChange>> states = new();
    public Stack<string> stateNames = new();
    public Dictionary<IEnumerator<StateChange>, IDisposableState> disposableStates = new();

    public bool IsEmpty => states.Count == 0;

    public void Pop(int count = 1, bool all =false) {
        if (all)
            count = states.Count;
        for (var i = 0; i < count; i++) {
            var state = states.Pop();
            stateNames.Pop();
            if (disposableStates.TryGetValue(state, out var disposableState)) {
                disposableState.Dispose();
                disposableStates.Remove(state);
            }
        }
    }

    public bool Is<T>(IEnumerator<StateChange> state, out T result) where T : IDisposableState {
        if (disposableStates.TryGetValue(state, out var disposableState) && disposableState is T) {
            result = (T)disposableState;
            return true;
        }
        result = default;
        return false;
    }
    
    public void Push(string stateName, IEnumerator<StateChange> state) {
        states.Push(state);
        stateNames.Push(stateName);
    }
    public void Push(IDisposableState disposableState) {
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
        Tick();
    }

    public const int maxDepth = 100;

    private void Tick(int depth = 0) {

        Assert.IsTrue(depth < maxDepth);

        if (!states.TryPeek(out var state))
            return;

        if (state.MoveNext()) {
            var stateChange = state.Current;
            for (var i = 0; i < stateChange.popCount; i++)
                Pop();
            if (stateChange.state != null)
                Push(stateChange.stateName, stateChange.state);
            if (stateChange.disposableState != null)
                Push(stateChange.disposableState);

            if (stateChange.popCount != 0 ||
                stateChange.state != null ||
                stateChange.disposableState != null) {

                Tick(depth + 1);
            }
        }
        else {
            Pop();
            Tick(depth + 1);
        }
    }

    [Command]
    public static int guiDepth = -1000;

    private void OnGUI() {
        if (Debug.isDebugBuild) {
            GUI.skin = DefaultGuiSkin.TryGet;
            GUI.depth = guiDepth;
            GUILayout.Label(string.Join(" / ", stateNames.Reverse().Select(name => name.EndsWith("State") ? name[..^"State".Length] : name)));
        }
    }
}

public static class DefaultGuiSkin {
    public static GUISkin TryGet => Resources.Load<GUISkin>("CommandLine");
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