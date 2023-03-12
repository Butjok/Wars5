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

    public static Sprite TryGetMissionSplashSprite(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial or MissionName.FirstMission or MissionName.SecondMission => Resources.Load<Sprite>(missionName.ToString()),
            _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
        };
    }

    public MissionName MissionName {
        set {
            splashImage.sprite = TryGetMissionSplashSprite(value);
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
        LoadingState.allowSceneActivation = true;
    }
}

public class LoadingState : IDisposableState {

    [Command]
    public static float minimumLoadingTime = 2;
    [Command]
    public static string sceneName = "Loading";

    public static bool allowSceneActivation;

    public MissionName missionName;
    public string saveData;

    public LoadingState(MissionName missionName, string saveData) {
        this.missionName = missionName;
        this.saveData = saveData;
    }

    public static string GetSceneName(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial or MissionName.FirstMission or MissionName.SecondMission => "LevelEditor",
            _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
        };
    }

    public IEnumerator<StateChange> Run {
        get {

            allowSceneActivation = false;
            
            if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                yield return StateChange.none;
            }

            var view = Object.FindObjectOfType<LoadingView>();
            Assert.IsTrue(view);

            var missionSceneName = GetSceneName(missionName);
            var loadingOperation = SceneManager.LoadSceneAsync(missionSceneName);
            loadingOperation.allowSceneActivation = false;

            var startTime = Time.time;
            while (Time.time < startTime + minimumLoadingTime || loadingOperation.progress < .9f) {
                view.Progress = loadingOperation.progress;
                yield return StateChange.none;
            }

            view.Ready = true;
            while (true) {
                
                allowSceneActivation = InputState.TryConsumeKeyDown(KeyCode.Return) || allowSceneActivation;
                allowSceneActivation = InputState.TryConsumeKeyDown(KeyCode.Space) || allowSceneActivation;

                if (allowSceneActivation)
                    break;
                yield return StateChange.none;
            }

            loadingOperation.allowSceneActivation = true;
            yield return  StateChange.none;

            var level = Object.FindObjectOfType<Level>();
            Assert.IsTrue(level);
            
            GameReader.ReadInto(level, saveData, true);
            if (level is LevelEditor levelEditor)
                levelEditor.RebuildTilemapMesh();
            yield return StateChange.ReplaceWith(new PlayerTurnState(level));
        }
    }

    public void Dispose() {
        
    }
}