using System.Collections.Generic;
using System.Data;
using Butjok.CommandLine;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuState2 : StateMachineState {

    public const string sceneName = "MainMenuNew";

    public MainMenuView2 view;
    public bool showSplash, showWelcome;
    public static float pressAnyKeyTimeout = 3;

    public MainMenuState2(StateMachine stateMachine, bool showSplash, bool showWelcome) : base(stateMachine) {
        this.showSplash = showSplash;
        this.showWelcome = showWelcome;
    }

    public override IEnumerator<StateChange> Enter {
        get {
            /*if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                yield return StateChange.none;
            }*/

            view = FindObject<MainMenuView2>();
            if (GitInfoEntry.TryLoad(out var gitInfo))
                view.gitInfo = gitInfo;

            if (showSplash)
                yield return StateChange.Push(new SplashState(stateMachine));

            if (CameraFader.IsBlack == true)
                CameraFader.FadeToWhite();
            
            var zoomFadeAnimation = CameraAnimation.ZoomFadeAnimation(view.mainCamera, 2, startFovFactor: .9f);
            while (zoomFadeAnimation.MoveNext())
                yield return StateChange.none;

            if (showWelcome) {

                foreach (var button in view.Buttons)
                    button.Interactable = false;

                var startTime = Time.time;
                while (!Input.anyKeyDown) {
                    if (!view.pressAnyKey.activeSelf && Time.time > startTime + pressAnyKeyTimeout)
                        view.pressAnyKey.SetActive(true);
                    yield return StateChange.none;
                }

                if (view.pressAnyKey.activeSelf)
                    view.pressAnyKey.SetActive(false);
            }

            yield return StateChange.Push(new MainMenuSelectionState2(stateMachine));
        }
    }
}

public class MainMenuSelectionState2 : StateMachineState {

    public enum Command { GoToCampaignOverview, OpenLoadGameMenu, OpenGameSettingsMenu, OpenAboutMenu, OpenQuitDialog }

    [Command] public static float quitHoldTime = .5f;

    public MainMenuSelectionState2(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.Find<GameSessionState>().game;
            var view = stateMachine.Find<MainMenuState2>().view;

            view.enqueueCommand = command => game.EnqueueCommand(command);

            foreach (var button in view.Buttons) {
                button.Interactable = button != view.loadGameButton || PersistentData.Read().savedGames.Count > 0;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(_ => game.EnqueueCommand(button.command));
            }

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

                        case (Command.OpenQuitDialog, _):
                            yield return StateChange.Push(new MinaMenuQuitConfirmationState(stateMachine));
                            break;

                        case (Command.GoToCampaignOverview, _):
                            var hasFadedToBlack = CameraFader.FadeToBlack();
                            while (!hasFadedToBlack())
                                yield return StateChange.none;
                            yield return StateChange.PopThenPush(2, new CampaignOverviewState2(stateMachine));
                            break;

                        case (Command.OpenLoadGameMenu, _):
                            yield return StateChange.Push(new MainMenuLoadGameState(stateMachine));
                            yield return StateChange.none;
                            break;

                        case (Command.OpenGameSettingsMenu, _):
                            //HideButtons();
                            yield return StateChange.Push(new MainMenuGameSettingsState(stateMachine));
                            yield return StateChange.none;
                            //ShowButtons();
                            break;

                        case (Command.OpenAboutMenu, _):
                            //HideButtons();
                            yield return StateChange.Push(new MainMenuAboutState(stateMachine));
                            yield return StateChange.none;
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
                            game.EnqueueCommand(Command.OpenQuitDialog);
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