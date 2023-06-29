using System.Collections.Generic;

public class MainMenuState2 : StateMachineState {

    public MainMenuView2 view;
    
    public MainMenuState2(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            view = FindObject<MainMenuView2>();
            yield return StateChange.Push(new MainMenuSelectionState2(stateMachine));
        }
    }
}

public class MainMenuSelectionState2 : StateMachineState {

    public enum Command { GoToCampaignOverview, OpenLoadGameMenu, OpenGameSettingsMenu, OpenAboutMenu, Quit }

    public MainMenuSelectionState2(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.Find<GameSessionState>().game;
            var view = stateMachine.Find<MainMenuState2>().view;

            foreach (var button in view.Buttons) {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(_ => game.EnqueueCommand(button.command));
            }
            view.loadGameButton.Interactable = PersistentData.Loaded.savedGames.Count > 0;

            while (true) {
                yield return StateChange.none;

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Quit, _):
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif
                            break;

                        case (Command.GoToCampaignOverview, _):
                            yield return StateChange.PopThenPush(3, new CampaignOverviewState2(stateMachine));
                            break;

                        case (Command.OpenLoadGameMenu, _):
                            yield return StateChange.Push(new MainMenuLoadGameState(stateMachine));
                            break;

                        case (Command.OpenGameSettingsMenu, _):
                            yield return StateChange.Push(new MainMenuGameSettingsState(stateMachine));
                            break;

                        case (Command.OpenAboutMenu, _):
                            yield return StateChange.Push(new MainMenuAboutState(stateMachine));
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }
}