using System.Collections.Generic;

public class MainMenuGameSettingsState : StateMachineState {

    public enum Command { Close }

    public MainMenuGameSettingsState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.Find<GameSessionState>().game;
            var view = stateMachine.Find<MainMenuState2>().view;
            view.gameSettingsMenu.Show(() => game.EnqueueCommand(Command.Close));

            while (true) {
                while (game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (Command.Close, _):
                            yield return StateChange.Pop();
                            break;
                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
                
                yield return StateChange.none;
            }
        }
    }
    public override void Exit() {
        var view = stateMachine.Find<MainMenuState2>().view;
        view.gameSettingsMenu.Hide();
    }
}