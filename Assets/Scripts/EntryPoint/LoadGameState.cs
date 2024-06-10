using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class LoadGameState : StateMachineState {

    public enum Command { Close }

    public LoadGameState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            
            var game = stateMachine.TryFind<GameSessionState>()?.game;
            Assert.IsNotNull(game);

            var saves = SaveEntry.FileNames
                .Select(SaveEntry.Read)
                .OrderByDescending(saveData => saveData.dateTime);

            var menu = Object.FindObjectOfType<LoadGameMenu>(true);
            Assert.IsTrue(menu);
            menu.Show(() => game.EnqueueCommand(Command.Close), saves);

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