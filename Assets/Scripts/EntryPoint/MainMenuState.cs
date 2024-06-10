using System.Collections.Generic;
using UnityEngine;

public class MainMenuState : StateMachineState {

    public MainMenuState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var entryPointState = stateMachine.TryFind<EntryPointState>();
            var view = entryPointState.view;

            PostProcessing.ColorFilter = Color.black;
            view.mainCamera.enabled = true;

            PostProcessing.Fade(Color.white, view.fadeDuration, view.fadeEasing);

            if (entryPointState.showWelcome)
                yield return StateChange.Push(new MainMenuWelcomeState(stateMachine));
            else
                yield return StateChange.Push(new MainMenuSelectionState(stateMachine));
        }
    }

    public override void Exit() {
        var view = stateMachine.TryFind<EntryPointState>().view;
        PostProcessing.ColorFilter = Color.white;
        view.mainCamera.enabled = false;
    }
}