using System.Collections.Generic;
using Object = UnityEngine.Object;

public class GameSettingsState : StateMachineState {

    public enum Command { Close }

    public GameSettingsState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var gameSessionState = stateMachine.Find<GameSessionState>();
            var (game, menu) = (gameSessionState.game, Object.FindObjectOfType<GameSettingsMenu>());

            menu.Show(gameSessionState.persistentData.settings, () => game.EnqueueCommand(Command.Close));
            while (true) {
                yield return StateChange.none;
                while (game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (Command.Close, _):
                            menu.Hide();
                            yield return StateChange.Pop();
                            break;
                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }
}