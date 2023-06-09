using System.Collections.Generic;
using UnityEngine;

public class MainMenuAboutState : StateMachineState {

    public MainMenuAboutState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var view = stateMachine.TryFind<EntryPointState>().view;
            view.about.SetActive(true);
            foreach (var go in view.hiddenInAbout)
                go.SetActive(false);

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

    public override void Dispose() {
        var view = stateMachine.TryFind<EntryPointState>().view;
        view.about.SetActive(false);
        foreach (var go in view.hiddenInAbout)
            go.SetActive(true);
    }
}