using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class GameMenuState : StateMachineState {

    public enum Command { Close, OpenSettingsMenu, OpenLoadGameMenu }

    public GameMenuState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var game = stateMachine.TryFind<GameSessionState>()?.game;
            var level = stateMachine.TryFind<LevelSessionState>()?.level;
            var menu = Object.FindObjectOfType<GameMenuView>(true);
            Assert.IsNotNull(game);
            Assert.IsNotNull(level);
            Assert.IsTrue(menu);

            PlayerView.globalVisibility = false;
            yield return StateChange.none;

            var cursor = level.view.cursorView;
            var cameraRig = level.view.cameraRig;

            if (cursor)
                cursor.Visible = false;
            if (cameraRig)
                cameraRig.enabled = false;

            menu.enqueueCloseCommand = () => game.EnqueueCommand(Command.Close);
            menu.Show();

            while (true) {
                yield return StateChange.none;

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Close, _):
                            menu.Hide();
                            if (cursor)
                                cursor.Visible = true;
                            if (cameraRig)
                                cameraRig.enabled = true;
                            PlayerView.globalVisibility = true;
                            yield return StateChange.Pop();
                            break;

                        case (Command.OpenSettingsMenu, _):
                            menu.Hide();
                            yield return StateChange.Push(new GameSettingsState(stateMachine));
                            menu.Show();
                            break;

                        case (Command.OpenLoadGameMenu, _):
                            menu.Hide();
                            yield return StateChange.Push(new LoadGameState(stateMachine));
                            menu.Show();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
        }
    }
}