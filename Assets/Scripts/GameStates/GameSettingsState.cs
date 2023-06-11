using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class GameSettingsState : StateMachineState {

    public enum Command { Close }

    public GameSettingsState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Entry {
        get {
            var (game, menu) = (GetState<GameSessionState>().game, GetObject<GameSettingsMenu>());

            menu.Show(() => game.EnqueueCommand(Command.Close));
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