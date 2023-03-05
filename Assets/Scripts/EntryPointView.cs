using System;
using System.Collections;
using System.Collections.Generic;
using Butjok.CommandLine;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

public class EntryPointView : MonoBehaviour {

    private void Awake() {

        Assert.AreEqual(1, FindObjectsOfType<EntryPointView>(true).Length);

        Assert.IsTrue(videoPlayer);
        Assert.IsTrue(videoPlayer.targetCamera);
        Assert.IsTrue(bulkaGamesIntro);
        Assert.IsTrue(mainCamera);
        Assert.IsTrue(pressAnyKeyText);

        Assert.IsTrue(startVirtualCamera);
        Assert.IsTrue(logoVirtualCamera);
        Assert.IsTrue(mainMenuVirtualCamera);
    }

    public bool showSplash = true;
    public bool showWelcome = true;

    private void Start() {
        if (StateRunner.Instance.IsEmpty)
            StateRunner.Instance.PushState(new EntryPointState(showSplash, showWelcome));
    }

    public VideoPlayer videoPlayer;
    public VideoClip bulkaGamesIntro;
    public Camera mainCamera;

    public CinemachineVirtualCamera startVirtualCamera;
    public CinemachineVirtualCamera logoVirtualCamera;
    public CinemachineVirtualCamera mainMenuVirtualCamera;

    public float fadeDuration = 2;
    public Ease fadeEasing = Ease.Unset;
    public TMP_Text pressAnyKeyText;
    public float delay = 1;
    public Image holdImage;
}

public class EntryPointState : IDisposableState {

    [Command]
    public static string sceneName = "EntryPoint";

    public bool showSplash, showWelcome;
    public EntryPointState(bool showSplash, bool showWelcome) {
        this.showSplash = showSplash;
        this.showWelcome = showWelcome;
    }

    public IEnumerator<StateChange> Run {
        get {
            if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                yield return StateChange.none;
            }

            var view = Object.FindObjectOfType<EntryPointView>();
            Assert.IsTrue(view);

            if (showSplash)
                yield return StateChange.Push(new SplashState(view, showWelcome));
            else
                yield return StateChange.Push(new MainMenuState(view, showWelcome));
        }
    }

    public void Dispose() {
        // none
    }
}

public class SplashState : IDisposableState {

    public EntryPointView view;
    public bool showWelcome;
    public SplashState(EntryPointView view, bool showWelcome) {
        this.view = view;
        this.showWelcome = showWelcome;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.videoPlayer.enabled = true;
            view.videoPlayer.targetCamera.enabled = true;

            view.videoPlayer.clip = view.bulkaGamesIntro;
            view.videoPlayer.Play();
            var splashCompleted = false;
            view.videoPlayer.loopPointReached += _ => splashCompleted = true;

            while (!splashCompleted && !Input.anyKeyDown)
                yield return StateChange.none;
            yield return StateChange.none;

            yield return StateChange.ReplaceWith(new MainMenuState(view, showWelcome));
        }
    }

    public void Dispose() {
        view.videoPlayer.enabled = false;
        view.videoPlayer.targetCamera.enabled = false;
    }
}

public class MainMenuState : IDisposableState {

    public EntryPointView view;
    public bool showWelcome;
    public MainMenuState(EntryPointView view, bool showWelcome) {
        this.view = view;
        this.showWelcome = showWelcome;
    }

    public IEnumerator<StateChange> Run {
        get {
            PostProcessing.ColorFilter = Color.black;
            view.mainCamera.enabled = true;

            PostProcessing.Fade(Color.white, view.fadeDuration, view.fadeEasing);

            if (showWelcome)
                yield return StateChange.Push(new MainMenuWelcomeState(view));
            else
                yield return StateChange.Push(new MainMenuSelectionState(view));
        }
    }

    public void Dispose() {
        PostProcessing.ColorFilter = Color.white;
        view.mainCamera.enabled = false;
    }
}

public class MainMenuWelcomeState : IDisposableState {

    public EntryPointView view;
    public MainMenuWelcomeState(EntryPointView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.logoVirtualCamera.enabled = true;

            var pressAnyKeySequence = DOTween.Sequence();
            pressAnyKeySequence
                .SetDelay(view.delay)
                .AppendCallback(() => view.pressAnyKeyText.enabled = true);

            while (!Input.anyKeyDown)
                yield return StateChange.none;
            yield return StateChange.none;

            pressAnyKeySequence.Kill();
            view.pressAnyKeyText.enabled = false;

            yield return StateChange.ReplaceWith(new MainMenuSelectionState(view));
        }
    }

    public void Dispose() {
        view.logoVirtualCamera.enabled = false;
    }
}

public class MainMenuSelectionState : IDisposableState {

    [Command]
    public static float fadeDuration = .25f;
    [Command]
    public static Ease fadeEasing = Ease.Unset;
    [Command]
    public static float quitHoldTime = 1;

    public EntryPointView view;
    public MainMenuSelectionState(EntryPointView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.mainMenuVirtualCamera.enabled = true;

            while (true) {

                if (InputState.TryConsumeKeyDown(KeyCode.Return)) {
                    PostProcessing.ColorFilter = Color.white;
                    var tween = PostProcessing.Fade(Color.black, fadeDuration, fadeEasing);
                    while (tween.IsActive() && !tween.IsComplete())
                        yield return StateChange.none;
                    yield return StateChange.PopThenPush(3, new CampaignOverviewState2());
                }

                if (InputState.TryConsumeKeyDown(KeyCode.Escape)) {

                    var startTime = Time.time;
                    var quit = false;
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
                    if (quit) {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    }

                    view.holdImage.enabled = false;
                }

                yield return StateChange.none;
            }
        }
    }

    public void Dispose() {
        view.mainMenuVirtualCamera.enabled = false;
    }
}