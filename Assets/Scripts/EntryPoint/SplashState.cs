using System.Collections.Generic;
using UnityEngine;

public class SplashState : StateMachineState {

    public SplashState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Entry {
        get {
            var view = stateMachine.TryFind<EntryPointState>().view;
            
            view.videoPlayer.enabled = true;
            view.videoPlayer.targetCamera.enabled = true;

            view.videoPlayer.clip = view.bulkaGamesIntro;
            view.videoPlayer.Play();
            var splashCompleted = false;
            view.videoPlayer.loopPointReached += _ => splashCompleted = true;

            while (!splashCompleted && !Input.anyKeyDown)
                yield return StateChange.none;
            yield return StateChange.none;

            yield return StateChange.ReplaceWith(new MainMenuState(stateMachine));
        }
    }

    public override void Exit() {
        var view = stateMachine.TryFind<EntryPointState>().view;
        
        view.videoPlayer.enabled = false;
        view.videoPlayer.targetCamera.enabled = false;
    }
}