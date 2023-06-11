using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class EntryPointState : StateMachineState {

    [Command]
    public static string sceneName = "EntryPoint";

    public EntryPointView view;
    public bool showSplash, showWelcome;
    public EntryPointState(StateMachine stateMachine, bool showSplash, bool showWelcome) : base(stateMachine) {
        this.showSplash = showSplash;
        this.showWelcome = showWelcome;
    }

    public override IEnumerator<StateChange> Entry {
        get {
            if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                LightProbes.Tetrahedralize();
                yield return StateChange.none;
            }

            view = Object.FindObjectOfType<EntryPointView>();
            Assert.IsTrue(view);

            if (showSplash)
                yield return StateChange.Push(new SplashState(stateMachine));
            else
                yield return StateChange.Push(new MainMenuState(stateMachine));
        }
    }
}