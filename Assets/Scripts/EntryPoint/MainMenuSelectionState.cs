using System.Collections.Generic;
using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;

public class MainMenuSelectionState : StateMachineState {

    public static bool quit, goToCampaign, goToAbout, goToSettings, goToLoadGame;

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

    public override IEnumerator<StateChange> Sequence {
        get {
            var view = stateMachine.TryFind<EntryPointState>().view;
            view.mainMenuVirtualCamera.enabled = true;
            view.textFrame3d.gameObject.SetActive(true);

            defaultColor = view.loadGameText.color;
            if (PersistentData.Loaded.savedGames.Count == 0 || simulateNoSavedGames)
                view.loadGameText.color = view.inactiveColor;

            while (true) {

                if (quit) {
                    quit = false;
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    yield break;
                }

                if (goToCampaign) {
                    goToCampaign = false;
                    PostProcessing.ColorFilter = Color.white;
                    var tween = PostProcessing.Fade(Color.black, fadeDuration, fadeEasing);
                    while (tween.IsActive() && !tween.IsComplete())
                        yield return StateChange.none;
                    yield return StateChange.PopThenPush(3, new CampaignOverviewState2(stateMachine));
                }

                if (goToLoadGame) {
                    goToLoadGame = false;
                    if (view.loadGameText.color != view.inactiveColor) {
                        yield return StateChange.Push(new MainMenuLoadGameState(stateMachine));
                        continue;
                    }
                    else
                        UiSound.Instance.notAllowed.PlayOneShot();
                }

                if (goToSettings) {
                    goToSettings = false;
                }

                if (goToAbout) {
                    goToAbout = false;
                    yield return StateChange.Push(new MainMenuAboutState(stateMachine));
                    continue;
                }

                if (InputState.TryConsumeKeyDown(KeyCode.Escape)) {

                    var startTime = Time.time;
                    view.holdImage.enabled = true;

                    while (!InputState.TryConsumeKeyUp(KeyCode.Escape)) {

                        var holdTime = Time.time - startTime;
                        if (holdTime > quitHoldTime) {
                            quit = true;
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

    public override void Dispose() {
        var view = stateMachine.TryFind<EntryPointState>().view;
        view.mainMenuVirtualCamera.enabled = false;
        view.loadGameText.color = defaultColor;
        view.textFrame3d.gameObject.SetActive(false);
    }
}