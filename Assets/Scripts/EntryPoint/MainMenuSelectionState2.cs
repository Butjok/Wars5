using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuState2 : StateMachineState {

    public const string sceneName = "MainMenuTest";

    public MainMenuView2 view;
    public bool showSplash, showWelcome;
    public static float pressAnyKeyTimeout = 3;

    public MainMenuState2(StateMachine stateMachine, bool showSplash, bool showWelcome) : base(stateMachine) {
        this.showSplash = showSplash;
        this.showWelcome = showWelcome;
    }

    public override IEnumerator<StateChange> Enter {
        get {
            if (SceneManager.GetActiveScene().name != sceneName)
                SceneManager.LoadScene(sceneName);

            view = Object.FindObjectOfType<MainMenuView2>();
            while (!view) {
                yield return StateChange.none;
                view = Object.FindObjectOfType<MainMenuView2>();
            }
            view.enabled = false;

            if (GitInfoEntry.TryLoad(out var gitInfo))
                view.gitInfo = gitInfo;
            
            Time.timeScale = 0;

            if (showSplash)
                yield return StateChange.Push(new SplashState(stateMachine));

            if (CameraFader.IsBlack == true)
                CameraFader.FadeToWhite();
            
            Time.timeScale = 1;

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

            view.enabled = true;

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
            var gameSession = stateMachine.Find<GameSessionState>();
            var game = gameSession.game;
            var view = stateMachine.Find<MainMenuState2>().view;

            view.enqueueCommand = command => game.EnqueueCommand(command);

            foreach (var button in view.Buttons) {
                button.Interactable = button != view.loadGameButton || gameSession.persistentData.campaign.Missions.Any(mission => mission.saves.Any());
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
                            view.enabled = false;
                            yield return StateChange.Push(new MinaMenuQuitConfirmationState(stateMachine));
                            view.enabled = true;
                            break;

                        case (Command.GoToCampaignOverview, _):
                            var hasFadedToBlack = CameraFader.FadeToBlack();
                            while (!hasFadedToBlack())
                                yield return StateChange.none;
                            yield return StateChange.PopThenPush(2, new CampaignOverviewState2(stateMachine));
                            break;

                        case (Command.OpenLoadGameMenu, _):
                            view.enabled = false;
                            yield return StateChange.Push(new MainMenuLoadGameState(stateMachine));
                            view.enabled = true;
                            break;

                        case (Command.OpenGameSettingsMenu, _):
                            view.enabled = false;
                            yield return StateChange.Push(new MainMenuGameSettingsState(stateMachine));
                            view.enabled = true;
                            break;

                        case (Command.OpenAboutMenu, _):
                            view.enabled = false;
                            yield return StateChange.Push(new MainMenuAboutState(stateMachine));
                            view.enabled = true;
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