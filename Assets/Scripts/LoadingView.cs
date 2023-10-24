using System;
using System.Collections.Generic;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class LoadingView : MonoBehaviour {

    public LoadingSpinner spinner;
    public UsefulTip usefulTip;
    public Button startButton;
    public TMP_Text startButtonText;
    public Image splashImage;
    public TMP_Text progressText;
    public string progressTextFormat = "{}%";
    public Action launch;

    public float Progress {
        set {
            if (progressText) {
                if (progressText.enabled)
                    progressText.enabled = true;
                progressText.text = string.Format(progressTextFormat, Mathf.RoundToInt(value * 100));
            }
        }
    }

    private void Start() {

        startButton.onClick.AddListener(Launch);

        startButtonText = startButton.GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(startButtonText);
        startButtonText.enabled = false;
        startButton.interactable = false;

        spinner.gameObject.SetActive(true);
        usefulTip.gameObject.SetActive(true);
    }

    public Mission Mission {
        set {
            splashImage.sprite = value.LoadingScreen;
            if (!splashImage.sprite)
                splashImage.sprite = null;
        }
    }

    [ContextMenu(nameof(SetReady))]
    public void SetReady() {
        Ready = true;
    }

    public bool Ready {
        set {
            if (value) {
                startButton.interactable = true;
                startButtonText.enabled = true;

                spinner.gameObject.SetActive(false);
                usefulTip.gameObject.SetActive(false);

                if (progressText)
                    progressText.enabled = false;
            }
        }
    }

    public void Launch() {
        launch?.Invoke();
    }
}

public class LoadingState : StateMachineState {

    [Command]
    public static float minimumLoadingTime = 0;
    [Command]
    public static string sceneName = "Loading";

    public SavedMission savedMission;

    public LoadingState(StateMachine stateMachine, SavedMission savedMission) : base(stateMachine) {
        this.savedMission = savedMission;
    }

    public override IEnumerator<StateChange> Enter {
        get {

            LoadingView view = null;

            if (SceneManager.GetActiveScene().name != sceneName)
                SceneManager.LoadScene(sceneName);
            
            view = Object.FindObjectOfType<LoadingView>();
            while (!view) {
                yield return StateChange.none;
                view = Object.FindObjectOfType<LoadingView>();
            }
            
            var loadingOperation = SceneManager.LoadSceneAsync(savedMission.mission.SceneName);
            loadingOperation.allowSceneActivation = false;
            view.launch = () => loadingOperation.allowSceneActivation = true;

            var startTime = Time.time;
            while (Time.time < startTime + minimumLoadingTime || loadingOperation.progress < .9f) {
                view.Progress = loadingOperation.progress;
                yield return StateChange.none;
            }

            view.Ready = true;
            while (true) {

                loadingOperation.allowSceneActivation = InputState.TryConsumeKeyDown(KeyCode.Return) || loadingOperation.allowSceneActivation;
                loadingOperation.allowSceneActivation = InputState.TryConsumeKeyDown(KeyCode.Space) || loadingOperation.allowSceneActivation;

                if (loadingOperation.allowSceneActivation)
                    break;
                yield return StateChange.none;
            }

            while (SceneManager.GetActiveScene().name != savedMission.mission.SceneName)
                yield return StateChange.none;

            yield return StateChange.ReplaceWith(new LevelSessionState(stateMachine, savedMission));
        }
    }

    public override void Exit() { }
}