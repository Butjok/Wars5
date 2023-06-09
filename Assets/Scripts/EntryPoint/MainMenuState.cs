using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class MainMenuState : StateMachine.State {

    public MainMenuState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var entryPointState = stateMachine.TryFind<EntryPointState>();
            var view = entryPointState.view;

            SceneManager.LoadScene("EntryPoint");
            view = Object.FindObjectOfType<EntryPointView>();
            Assert.IsTrue(view);

            PostProcessing.ColorFilter = Color.black;
            view.mainCamera.enabled = true;

            PostProcessing.Fade(Color.white, view.fadeDuration, view.fadeEasing);

            if (entryPointState.showWelcome)
                yield return StateChange.Push(new MainMenuWelcomeState(stateMachine));
            else
                yield return StateChange.Push(new MainMenuSelectionState(stateMachine));
        }
    }

    public override void Dispose() {
        var view = stateMachine.TryFind<EntryPointState>().view;
        PostProcessing.ColorFilter = Color.white;
        view.mainCamera.enabled = false;
    }
}