using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionState : StateMachineState {

    public enum Command { LaunchEntryPoint, PlayLevel, OpenLevelEditor }

    public Game game;
    public GameSessionState(Game game) : base(game.stateMachine) {
        this.game = game;
    }

    public override IEnumerator<StateChange> Sequence {
        get {

            Player.undisposed.Clear();
            Building.undisposed.Clear();
            Unit.undisposed.Clear();
            UnitAction.undisposed.Clear();

            while (true) {
                yield return StateChange.none;
                while (game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (Command.LaunchEntryPoint, (bool showSplashScreen, bool showWelcome)):
                            yield return StateChange.Push(new EntryPointState(stateMachine, showSplashScreen, showWelcome));
                            break;
                        case (Command.PlayLevel, string input):
                            yield return StateChange.Push(new PlayState(stateMachine, input));
                            break;
                        case (Command.OpenLevelEditor, string input):
                            yield return StateChange.Push(new LevelEditorState(stateMachine, input));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
        }
    }
    public override void Dispose() {
        if (Player.undisposed.Count > 0)
            Debug.LogError($"undisposed players: {Player.undisposed.Count}");
        if (Building.undisposed.Count > 0)
            Debug.LogError($"undisposed buildings: {Building.undisposed.Count}");
        if (Unit.undisposed.Count > 0)
            Debug.LogError($"undisposed units: {Unit.undisposed.Count}");
        if (UnitAction.undisposed.Count > 0)
            Debug.LogError($"undisposed unit actions: {UnitAction.undisposed.Count}");
    }
}