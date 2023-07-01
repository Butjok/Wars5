using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuState2 : StateMachineState {

    public const string sceneName = "MainMenuNew";

    public MainMenuView2 view;
    public bool showSplash, showWelcome;

    public MainMenuState2(StateMachine stateMachine, bool showSplash, bool showWelcome) : base(stateMachine) {
        this.showSplash = showSplash;
        this.showWelcome = showWelcome;
    }

    public override IEnumerator<StateChange> Enter {
        get {
            if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                yield return StateChange.none;
            }

            if (CameraFader.IsBlack == true)
                CameraFader.FadeToWhite();

            view = FindObject<MainMenuView2>();
            yield return StateChange.Push(new MainMenuSelectionState2(stateMachine));
        }
    }
}

public class MainMenuSelectionState2 : StateMachineState {

    public enum Command { GoToCampaignOverview, OpenLoadGameMenu, OpenGameSettingsMenu, OpenAboutMenu, Quit }

    [Command] public static float quitHoldTime = 1;

    public MainMenuSelectionState2(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.Find<GameSessionState>().game;
            var view = stateMachine.Find<MainMenuState2>().view;

            foreach (var button in view.Buttons) {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(_ => game.EnqueueCommand(button.command));
            }
            view.loadGameButton.Interactable = PersistentData.Read().savedGames.Count > 0;

            void HideButtons() {
                foreach (var button in view.Buttons)
                    button.Visible = false;
            }
            void ShowButtons() {
                foreach (var button in view.Buttons)
                    button.Visible = true;
            }

            while (true) {
                yield return StateChange.none;

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Quit, _):
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif
                            break;

                        case (Command.GoToCampaignOverview, _):
                            var hasFadedToBlack = CameraFader.FadeToBlack();
                            while (!hasFadedToBlack())
                                yield return StateChange.none;
                            yield return StateChange.PopThenPush(2, new CampaignOverviewState2(stateMachine));
                            break;

                        case (Command.OpenLoadGameMenu, _):
                            yield return StateChange.Push(new MainMenuLoadGameState(stateMachine));
                            break;

                        case (Command.OpenGameSettingsMenu, _):
                            //HideButtons();
                            yield return StateChange.Push(new MainMenuGameSettingsState(stateMachine));
                            //ShowButtons();
                            break;

                        case (Command.OpenAboutMenu, _):
                            //HideButtons();
                            yield return StateChange.Push(new MainMenuAboutState(stateMachine));
                            //ShowButtons();
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }

                if (Input.GetKeyDown(KeyCode.Escape)) {

                    var startTime = Time.time;
                    view.holdImage.enabled = true;

                    while (!Input.GetKeyUp(KeyCode.Escape)) {

                        var holdTime = Time.time - startTime;
                        if (holdTime > quitHoldTime) {
                            game.EnqueueCommand(Command.Quit);
                            break;
                        }

                        view.holdImage.fillAmount = holdTime / quitHoldTime;
                        yield return StateChange.none;
                    }

                    view.holdImage.enabled = false;
                    continue;
                }
            }
        }
    }
}