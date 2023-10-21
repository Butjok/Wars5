using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        name = Regex.Replace(name, @"State(\d*)$", "$1");
        stateNames.Push(name);
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
    public T Find<T>() where T : StateMachineState {
        var result = TryFind<T>();
        Assert.IsNotNull(result);
        return result;
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

    public virtual void Exit() { }

    public abstract IEnumerator<StateChange> Enter { get; }

    protected static StateChange HandleUnexpectedCommand(object command) {
        return StateChange.none;
    }

    protected bool TryEnqueueModeSelectionCommand() {
        var game = stateMachine.Find<GameSessionState>().game;
        if (Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.LeftShift)) {
            game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectTilesMode);
            return true;
        }
        if (Input.GetKeyDown(KeyCode.U) && Input.GetKey(KeyCode.LeftShift)) {
            game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectUnitsMode);
            return true;
        }
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift)) {
            game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectTriggersMode);
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftShift)) {
            game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectAreasMode);
            return true;
        }
        return false;
    }

    protected StateChange HandleModeSelectionCommand(object command) {
        switch (command) {
            case (LevelEditorSessionState.SelectModeCommand.SelectTilesMode, _):
                return StateChange.ReplaceWith(new LevelEditorTilesModeState(stateMachine));
            case (LevelEditorSessionState.SelectModeCommand.SelectUnitsMode, _):
                return StateChange.ReplaceWith(new LevelEditorUnitsModeState(stateMachine));
            case (LevelEditorSessionState.SelectModeCommand.SelectTriggersMode, _):
                return StateChange.ReplaceWith(new LevelEditorTriggersModeState(stateMachine));
            case (LevelEditorSessionState.SelectModeCommand.SelectAreasMode, _):
                return StateChange.ReplaceWith(new LevelEditorZoneModeState(stateMachine));
            case (LevelEditorSessionState.SelectModeCommand.Play, _):
                return StateChange.Push(new LevelEditorPlayState(stateMachine));
        }
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

public static class StateMachineStateExtensions {
    
    public static void UpdateTilemapCursor(this Level level) {
        
        if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition2)) {
            level.TryGetBuilding(mousePosition2, out var building);
            level.TryGetTile(mousePosition2, out var tileType);
            level.TryGetUnit(mousePosition2, out var unit);
            level.view.tilemapCursor.Set(mousePosition2, tileType, building, unit);
        }
                
        else
            level.view.tilemapCursor.Hide();
    }
}