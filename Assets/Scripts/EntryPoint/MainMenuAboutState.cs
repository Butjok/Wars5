using System.Collections.Generic;
using UnityEngine;

public class MainMenuAboutState : StateMachineState {

    public MainMenuAboutState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var view = stateMachine.TryFind<MainMenuState2>().view;

            // foreach (var button in view.Buttons)
            //     button.Visible = false;

            view.aboutRoot.SetActive(true);

            view.aboutScrollRect.verticalNormalizedPosition = 1;

            while (true) {
                var shouldStop = InputState.TryConsumeKeyDown(KeyCode.Escape);
                if (shouldStop)
                    break;
                yield return StateChange.none;
            }

            yield return StateChange.Pop();
        }
    }

    public override void Exit() {
        var view = stateMachine.TryFind<MainMenuState2>().view;
        view.aboutRoot.SetActive(false);
        // foreach (var button in view.Buttons)
        //     button.Visible = true;
    }
}