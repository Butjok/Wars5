using System.Collections.Generic;
using UnityEngine;

public class MainMenuAboutState : StateMachineState {

    public MainMenuAboutState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var view = stateMachine.TryFind<MainMenuState2>().view;

            // foreach (var button in view.Buttons)
            //     button.Visible = false;

            view.aboutRoot.gameObject.SetActive(true);
            view.TranslateShowPanel(view.aboutRoot);

            view.aboutScrollRect.verticalNormalizedPosition = 1;

            while (true) {
                var shouldStop = Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right);
                yield return StateChange.none;
                if (shouldStop)
                    break;
                if (Game.TryDequeueCommand(out var command)) {
                    if (command.name is MainMenuSelectionState2.Command.OpenAboutMenu) continue;
                    Game.EnqueueCommand(command.name);
                    yield return StateChange.Pop();
                }
            }

            yield return StateChange.Pop();
        }
    }

    public override void Exit() {
        var view = stateMachine.TryFind<MainMenuState2>().view;
        view.TranslateHidePanel(view.aboutRoot);
        //view.aboutRoot.gameObject.SetActive(false);
        // foreach (var button in view.Buttons)
        //     button.Visible = true;
    }
}