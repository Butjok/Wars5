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
    
    public StateMachineState Peek() => states.Count == 0? null: states.Peek().state;
    public T Peek<T>() where T: StateMachineState {
        return (T)states.Peek().state;
    }

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
        return states.Peek().state.GetType() == typeof(T);
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

    private Game game;

    public Game Game {
        get {
            if (game == null) {
                game = stateMachine.Find<GameSessionState>().game;
                Assert.IsNotNull(game);
            }

            return game;
        }
    }

    private Level level;

    public Level Level {
        get {
            if (level == null) {
                level = stateMachine.Find<LevelSessionState>().level;
                Assert.IsNotNull(level);
            }

            return level;
        }
    }

    public void UpdateTilemapCursor() {
        Level.SetGui("tilemap-cursor", () => {
            if (Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition)) {
                Level.TryGetBuilding(mousePosition, out var building);
                Level.TryGetTile(mousePosition, out var tileType);
                Level.TryGetUnit(mousePosition, out var unit);
                Level.view.tilemapCursor.Set(mousePosition, tileType, building, unit);

                if (mousePosition.TryRaycast(out var hit)) {
                    var text = "";

                    if (unit != null) {
                        string FormatUnit(Unit unit) {
                            return $"<b><color=#{ColorUtility.ToHtmlStringRGB(unit.Player.UiColor)}>{unit.type}</color></b> ({unit.Hp})";
                        }

                        text = $"{FormatUnit(unit)}";
                        if (unit.Cargo.Count > 0)
                            text += ':';
                        text += '\n';
                        foreach (var cargo in unit.Cargo)
                            text += $"  {FormatUnit(cargo)}\n";
                    }

                    if (building != null)
                        text += $"<b><color=#{ColorUtility.ToHtmlStringRGB(building.Player?.UiColor ?? Color.white)}>{building.type}</color></b>\n";
                    //    else if (tileType != 0)
                    //        text += $"{tileType}\n";

                    if (text.Length > 0)
                        WarsGui.CenteredLabel(Level, hit.point, text.TrimEnd(), new Vector2(0, 75));
                }
            }

            else
                Level.view.tilemapCursor.Hide();
        });
    }

    public virtual void Exit() { }

    public abstract IEnumerator<StateChange> Enter { get; }

    protected static StateChange HandleUnexpectedCommand(object command) {
        return StateChange.none;
    }

    protected bool TryEnqueueModeSelectionCommand() {
        var game = stateMachine.Find<GameSessionState>().game;
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (Input.GetKeyDown(KeyCode.T)) {
                game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectTilesMode);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.U)) {
                game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectUnitsMode);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectTriggersMode);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Z)) {
                game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectAreasMode);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.P)) {
                game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectPropsMode);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.W)) {
                game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.SelectPathsMode);
                return true;
            }
        }

        if (Input.GetKeyDown(KeyCode.F5)) {
            game.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.Play);
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
            case (LevelEditorSessionState.SelectModeCommand.SelectPropsMode, _):
                return StateChange.ReplaceWith(new LevelEditorPropsModeState(stateMachine));
            case  (LevelEditorSessionState.SelectModeCommand.SelectPathsMode, _):
                return StateChange.ReplaceWith(new LevelEditorPathsModeState(stateMachine));
            case (LevelEditorSessionState.SelectModeCommand.Play, _):
                return StateChange.Push(new LevelEditorPlayState(stateMachine));
        }

        return StateChange.none;
    }
}

public class LevelEditorPropsModeState : StateMachineState {
    public PropPlacement propPlacement;
    public LevelEditorPropsModeState(StateMachine sm) : base(sm) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.Find<GameSessionState>().game;

            yield return StateChange.none;

            propPlacement = Object.FindObjectOfType<PropPlacement>();
            Assert.IsTrue(propPlacement);
            propPlacement.enabled = true;

            while (true) {
                yield return StateChange.none;

                if (TryEnqueueModeSelectionCommand()) { }

                while (game.TryDequeueCommand(out var command)) {
                    switch (command) {
                        case (LevelEditorSessionState.SelectModeCommand, _):
                            yield return HandleModeSelectionCommand(command);
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
                }
            }
        }
    }

    public override void Exit() {
        propPlacement.enabled = false;
        Debug.Log("LevelEditorPropsModeState.Exit");
        base.Exit();
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