using System.Collections.Generic;
using UnityEngine;

public class MainMenuWelcomeState : StateMachineState {

    public MainMenuWelcomeState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var view = stateMachine.TryFind<EntryPointState>().view;
            view.logoVirtualCamera.enabled = true;

            foreach (var go in view.hiddenInWelcomeScreen)
                go.SetActive(false);

            var startTime = Time.time;

            while (true) {
                if (Input.anyKeyDown) {
                    yield return StateChange.none;
                    break;
                }
                if (Time.time > startTime + view.delay && !view.pressAnyKeyText.enabled)
                    view.pressAnyKeyText.enabled = true;
                yield return StateChange.none;
            }
            view.pressAnyKeyText.enabled = false;

            yield return StateChange.ReplaceWith(new MainMenuSelectionState(stateMachine));
        }
    }

    public override void Dispose() {
        var view = stateMachine.TryFind<EntryPointState>().view;
        view.logoVirtualCamera.enabled = false;
        foreach (var go in view.hiddenInWelcomeScreen)
            go.SetActive(true);
    }
}