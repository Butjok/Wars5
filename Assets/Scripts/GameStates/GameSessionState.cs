using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionState : StateMachineState {

    public enum Command { LaunchEntryPoint, PlayLevel, OpenLevelEditor }

    public Game game;
    public PersistentData persistentData;
    
    public GameSessionState(Game game) : base(game.stateMachine) {
        this.game = game;
    }

    public override IEnumerator<StateChange> Enter {
        get {

            persistentData = PersistentData.Read();
            
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
                        case (Command.PlayLevel, (string input, MissionName missionName, bool isFreshStart)):
                            yield return StateChange.Push(new LevelSessionState(stateMachine, input, missionName,isFreshStart));
                            break;
                        case (Command.OpenLevelEditor, (string input, bool showLevelEditorTileMesh)):
                            yield return StateChange.Push(new LevelEditorSessionState(stateMachine, input, showLevelEditorTileMesh));
                            break;
                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }
    public override void Exit() {
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