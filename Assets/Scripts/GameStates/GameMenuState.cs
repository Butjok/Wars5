using System.Collections.Generic;

public class GameMenuState : StateMachineState {

    public enum Command { Close, OpenSettingsMenu, OpenLoadGameMenu }

    public GameMenuState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var (game, level, menu) = (FindState<GameSessionState>().game, FindState<LevelSessionState>().level, FindObject<GameMenuView>());

            PlayerView.globalVisibility = false;
            yield return StateChange.none;
            level.view.cursorView.Position = null;
            level.view.cameraRig.enabled = false;

            menu.enqueueCloseCommand = () => game.EnqueueCommand(Command.Close);
            menu.Show();

            while (true) {
                yield return StateChange.none;

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Close, _):
                            yield return StateChange.Pop();
                            break;

                        case (Command.OpenSettingsMenu, _):
                            yield return StateChange.ReplaceWith(new GameSettingsState(stateMachine));
                            break;

                        case (Command.OpenLoadGameMenu, _):
                            yield return StateChange.ReplaceWith(new LoadGameState(stateMachine));
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }

    public override void Exit() {
        var (menu, level)= (FindObject<GameMenuView>(), FindState<LevelSessionState>().level);
        menu.Hide();
        level.view.cameraRig.enabled = true;
        PlayerView.globalVisibility = true;
    }
}