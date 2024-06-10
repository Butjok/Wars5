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

            Player.toDematerialize.Clear();
            Building.toDematerialize.Clear();
            Unit.toDematerialize.Clear();

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
                        case (Command.OpenLevelEditor, (null, bool showLevelEditorTileMesh)):
                            yield return StateChange.Push(new LevelEditorSessionState(stateMachine, null, showLevelEditorTileMesh));
                            break;
                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }
    public override void Exit() {

        var collectionsToExamine = new(string, IReadOnlyCollection<object>)[] {
            (nameof(Player), Player.toDematerialize),
            (nameof(Building), Building.toDematerialize),
            (nameof(Unit), Unit.toDematerialize),
            (nameof(MineField), MineField.toDematerialize),
            (nameof(Crate), Crate.toDispose),
            (nameof(TunnelEntrance), TunnelEntrance.toDematerialize),
            (nameof(PipeSection), PipeSection.toDematerialize),
            (nameof(Bridge2), Bridge2.toDematerialize),
        };
        
        foreach (var (name, collection) in collectionsToExamine)
            if (collection.Count > 0)
                Debug.LogError($"Undisposed {name}: {collection.Count}");
    }
}