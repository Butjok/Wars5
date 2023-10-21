using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuQuitDialog : MonoBehaviour {

    public Action onConfirm;
    public Action onCancel;

    public bool Visible {
        set => gameObject.SetActive(value);
    }

    public void Confirm() {
        onConfirm?.Invoke();
    }
    public void Cancel() {
        onCancel?.Invoke();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            Confirm();
    }
}

public class MinaMenuQuitConfirmationState : StateMachineState {

    public enum Command { Quit, Cancel }

    public MainMenuQuitDialog dialog;

    public MinaMenuQuitConfirmationState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.Find<GameSessionState>().game;
            var view = stateMachine.Find<MainMenuState2>().view;

            dialog = view.quitDialog;
            dialog.onConfirm = () => game.EnqueueCommand(Command.Quit);
            dialog.onCancel = () => game.EnqueueCommand(Command.Cancel);

            dialog.Visible = true;

            while (true) {
                yield return StateChange.none;

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Cancel, _):
                            yield return StateChange.Pop();
                            break;

                        case (Command.Quit, _):
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }

    public override void Exit() {
        dialog.Visible = false;
    }
}