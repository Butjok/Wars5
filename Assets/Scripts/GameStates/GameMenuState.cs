using System.Collections.Generic;

public class GameMenuState : StateMachineState {

    public enum Command { Close, OpenSettingsMenu, OpenLoadGameMenu }

    public GameMenuState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var (game, level) = (FindState<GameSessionState>().game, FindState<LevelSessionState>().level);
            var inGameMenu = level.view.inGameMenu;

            PlayerView.globalVisibility = false;
            yield return StateChange.none;
            //level.view.cameraRig.enabled = false;

            inGameMenu.Show(() => game.EnqueueCommand(Command.Close));
            level.view.tilemapCursor.Hide();

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
        var level = FindState<LevelSessionState>().level;
        var inGameMenu = level.view.inGameMenu;
        inGameMenu.Hide();
        //level.view.cameraRig.enabled = true;
        PlayerView.globalVisibility = true;
    }
}