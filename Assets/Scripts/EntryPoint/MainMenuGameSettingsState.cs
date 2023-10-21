using System.Collections.Generic;

public class MainMenuGameSettingsState : StateMachineState {

    public enum Command { Close }

    public MainMenuGameSettingsState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var gameSessionState = stateMachine.Find<GameSessionState>();
            var game = gameSessionState.game;
            var view = stateMachine.Find<MainMenuState2>().view;
            view.gameSettingsMenu.Show(gameSessionState.persistentData.settings, () => game.EnqueueCommand(Command.Close));

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
        stateMachine.Find<MainMenuState2>().view.gameSettingsMenu.Hide();
    }
}