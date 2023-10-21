using System.Collections.Generic;
using UnityEngine;

public class GameSessionState : StateMachineState {

    public enum Command { LaunchMainMenu, PlayMission, OpenLevelEditor, StartCampaignOverview, }

    public Game game;
    public readonly PersistentData persistentData;

    public GameSessionState(Game game) : base(game.stateMachine) {
        this.game = game;
        persistentData = PersistentData.Read(this);
    }

    public override IEnumerator<StateChange> Enter {
        get {

            Player.undisposed.Clear();
            Building.undisposed.Clear();
            Unit.undisposed.Clear();
            UnitAction.undisposed.Clear();

            while (true) {
                yield return StateChange.none;
                while (game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (Command.LaunchMainMenu, (bool showSplashScreen, bool showWelcome)):
                            yield return StateChange.Push(new MainMenuState2(stateMachine, showSplashScreen, showWelcome));
                            break;
                        case (Command.StartCampaignOverview, _): {
                            Debug.Log(Command.StartCampaignOverview);
                            yield return StateChange.Push(new CampaignOverviewState2(stateMachine));
                            break;
                        }
                        case (Command.PlayMission, Mission mission):
                            yield return StateChange.Push(new LevelSessionState(stateMachine, new SavedMission{mission = mission, }));
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