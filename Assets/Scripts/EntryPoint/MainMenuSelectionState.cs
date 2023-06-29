using System.Collections.Generic;
using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;

public class MainMenuSelectionState : StateMachineState {

    public enum Command { GoToCampaignOverview, OpenLoadGameMenu, OpenGameSettingsMenu, OpenAboutMenu, Quit }

    [Command]
    public static float fadeDuration = .25f;
    [Command]
    public static Ease fadeEasing = Ease.Unset;
    [Command]
    public static float quitHoldTime = 1;
    [Command]
    public static bool simulateNoSavedGames = false;

    public Color defaultColor;
    public MainMenuSelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var game = stateMachine.Find<GameSessionState>().game;
            var view = stateMachine.TryFind<EntryPointState>().view;
            view.mainMenuVirtualCamera.enabled = true;
            view.textFrame3d.gameObject.SetActive(true);

            defaultColor = view.loadGameText.color;
            if (PersistentData.Loaded.savedGames.Count == 0 || simulateNoSavedGames)
                view.loadGameText.color = view.inactiveColor;

            while (true) {

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.Quit, _):
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                            break;

                        case (Command.OpenLoadGameMenu, _):
                            if (view.loadGameText.color != view.inactiveColor)
                                yield return StateChange.Push(new MainMenuLoadGameState(stateMachine));
                            else
                                UiSound.Instance.notAllowed.PlayOneShot();
                            break;
                        
                        case (Command.OpenGameSettingsMenu, _):
                            break;
                        
                        case (Command.OpenAboutMenu, _):
                            yield return StateChange.Push(new MainMenuAboutState(stateMachine));
                            break;
                        
                        case (Command.GoToCampaignOverview, _):
                            PostProcessing.ColorFilter = Color.white;
                            var tween = PostProcessing.Fade(Color.black, fadeDuration, fadeEasing);
                            while (tween.IsActive() && !tween.IsComplete())
                                yield return StateChange.none;
                            yield return StateChange.PopThenPush(3, new CampaignOverviewState2(stateMachine));
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

                yield return StateChange.none;
            }
        }
    }

    public override void Exit() {
        var view = stateMachine.TryFind<EntryPointState>().view;
        view.mainMenuVirtualCamera.enabled = false;
        view.loadGameText.color = defaultColor;
        view.textFrame3d.gameObject.SetActive(false);
    }
}