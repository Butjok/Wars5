using System.Collections.Generic;
using UnityEngine;

public class SplashState : StateMachineState {

    public SplashState(StateMachine stateMachine) : base(stateMachine) { }

    public MainMenuView2 view;

    public override IEnumerator<StateChange> Enter {
        get {
            view = stateMachine.Find<MainMenuState2>().view;

            view.mainCamera.enabled = false;
            view.splashScreenVideoPlayer.enabled = true;
            view.splashScreenVideoPlayer.targetCamera.enabled = true;

            view.splashScreenVideoPlayer.clip = view.splashScreenVideoClip;
            view.splashScreenVideoPlayer.Play();
            var splashCompleted = false;
            view.splashScreenVideoPlayer.loopPointReached += _ => splashCompleted = true;

            while (!splashCompleted && !Input.anyKeyDown)
                yield return StateChange.none;
            yield return StateChange.none;
        }
    }

    public override void Exit() {
        view.splashScreenVideoPlayer.enabled = false;
        view.splashScreenVideoPlayer.targetCamera.enabled = false;
        view.mainCamera.enabled = true;
    }
}