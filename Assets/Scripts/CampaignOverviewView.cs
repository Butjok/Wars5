using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Cinemachine;
using DG.Tweening;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static Mouse;

public class CampaignOverviewView : MonoBehaviour {

    public Camera mainCamera;
    public CinemachineVirtualCamera defaultVirtualCamera;
    public LayerMask mouseRaycastLayerMask;
    public GameObject loadingSpinner;
    public GameObject missionPanel;
    public TMP_Text missionName;
    public TMP_Text missionDescription;
    public Button startMissionButton;
    public Button backButton;

    public MissionView[] MissionViews => GetComponentsInChildren<MissionView>();

    public bool start = true;

    private void Start() {
        if (start && StateRunner.Instance.IsEmpty)
            StateRunner.Instance.PushState(new CampaignOverviewState2());
    }

    public bool ShowLoadingSpinner {
        set => loadingSpinner.SetActive(value);
    }

    public void StartMission() {
        CampaignOverviewMissionCloseUpState.shouldStart = true;
    }

    public void CycleMission(int offset) {
        var missionNames = MissionViews.Select(mv => mv.MissionName).ToList();
        var index = missionNames.IndexOf(CampaignOverviewMissionCloseUpState.missionName);
        Assert.AreNotEqual(-1, index);
        var nextIndex = (index + offset).PositiveModulo(missionNames.Count);
        CampaignOverviewSelectionState.targetMissionName = missionNames[nextIndex];
    }

    public void GoToMainMenu() {
        CampaignOverviewSelectionState.shouldGoBackToMainMenu = true;
    }
}

public class CampaignOverviewState2 : IDisposableState {

    [Command]
    public static string sceneName = "Campaign";

    [Command]
    public static float fadeDuration = 1;
    [Command]
    public static Ease fadeEasing = Ease.Unset;

    public IEnumerator<StateChange> Run {
        get {

            if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                yield return StateChange.none;
            }

            var view = Object.FindObjectOfType<CampaignOverviewView>();
            Assert.IsTrue(view);

            PostProcessing.ColorFilter = Color.black;
            PostProcessing.Fade(Color.white, fadeDuration, fadeEasing);

            yield return StateChange.Push(new CampaignOverviewSelectionState(view));
        }
    }

    public void Dispose() {
        //PostProcessing.ColorFilter = Color.white;
    }
}

public class CampaignOverviewSelectionState : IDisposableState {

    [Command]
    public static float fadeDuration = .5f;
    [Command]
    public static Ease fadeEasing = Ease.Unset;
    [Command]
    public static bool drawDebugHit = false;
    [Command]
    public static bool shouldGoBackToMainMenu = false;

    public static MissionName? targetMissionName;

    public CampaignOverviewView view;
    public MissionView hoveredMissionView;

    public CampaignOverviewSelectionState(CampaignOverviewView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {

            view.defaultVirtualCamera.enabled = true;

            var campaign = PersistentData.Read().campaign;
            foreach (var missionView in view.MissionViews) {
                var missionName = missionView.MissionName;
                var isAvailable = campaign.IsAvailable(missionName);
                missionView.IsAvailable = isAvailable;
            }

            while (true) {

                if (targetMissionName is { } actualMissionName) {
                    targetMissionName = null;
                    var missionView = view.MissionViews.Single(mv => mv.MissionName == actualMissionName);
                    yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(view, missionView, actualMissionName));
                    continue;
                }

                if (InputState.IsKeyDown(KeyCode.Escape)) {
                    InputState.ConsumeKeyDown(KeyCode.Escape);
                    shouldGoBackToMainMenu = true;
                }

                if (shouldGoBackToMainMenu) {
                    shouldGoBackToMainMenu = false;

                    PostProcessing.ColorFilter = Color.white;
                    var tween = PostProcessing.Fade(Color.black, fadeDuration, fadeEasing);
                    while (tween.IsActive() && !tween.IsComplete())
                        yield return StateChange.none;
                    yield return StateChange.PopThenPush(2, new EntryPointState(false, false));
                }

                var ray = view.mainCamera.ScreenPointToRay(Input.mousePosition);
                var hasHit = Physics.Raycast(ray, out var hit, float.MaxValue, view.mouseRaycastLayerMask);

                if (hasHit) {

                    // highlight mission as hovered by mouse
                    var missionView = hit.collider.GetComponentInParent<MissionView>();
                    Assert.IsTrue(missionView);
                    if (hoveredMissionView != missionView) {
                        if (hoveredMissionView)
                            hoveredMissionView.Hovered = false;
                        hoveredMissionView = missionView;
                        hoveredMissionView.Hovered = true;
                    }

                    if (InputState.IsMouseButtonDown(left)) {
                        InputState.ConsumeMouseButtonDown(left);

                        if (hoveredMissionView) {
                            hoveredMissionView.Hovered = false;
                            hoveredMissionView = null;
                        }

                        if (drawDebugHit)
                            using (Draw.ingame.WithDuration(1))
                                Draw.ingame.Cross(hit.point);

                        var name = hit.collider.name;
                        var parsed = Enum.TryParse(name, out MissionName missionName);
                        Assert.IsTrue(parsed, name);

                        yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(view, missionView, missionName));
                        continue;
                    }
                }

                else if (hoveredMissionView) {
                    hoveredMissionView.Hovered = false;
                    hoveredMissionView = null;
                }

                yield return StateChange.none;
            }
        }
    }

    public void Dispose() {
        view.defaultVirtualCamera.enabled = false;
    }
}

public class CampaignOverviewMissionCloseUpState : IDisposableState {

    public static MissionName missionName;
    public static bool shouldStart;

    public CampaignOverviewView view;
    public MissionView missionView;

    public CampaignOverviewMissionCloseUpState(CampaignOverviewView view, MissionView missionView, MissionName missionName) {
        this.view = view;
        this.missionView = missionView;
        CampaignOverviewMissionCloseUpState.missionName = missionName;
    }

    public IEnumerator<StateChange> Run {
        get {

            view.missionPanel.SetActive(true);
            view.missionName.text = Strings.GetName(missionName);
            view.missionDescription.text = Strings.GetDescription(missionName);

            view.startMissionButton.interactable = PersistentData.Read().campaign.IsAvailable(missionName);

            if (missionView.TryGetVirtualCamera)
                missionView.TryGetVirtualCamera.enabled = true;

            if (missionView.text)
                missionView.text.enabled = false;

            view.backButton.gameObject.SetActive(false);

            while (true) {

                if (InputState.ScrollWheel != 0) {
                    view.CycleMission(InputState.ScrollWheel);
                    InputState.ConsumeScrollWheel();
                }

                // if we should switch to other mission
                if (CampaignOverviewSelectionState.targetMissionName is { })
                    yield return StateChange.Pop();

                var shouldGoBack = false;
                if (InputState.IsKeyDown(KeyCode.Escape)) {
                    InputState.ConsumeKeyDown(KeyCode.Escape);
                    shouldGoBack = true;
                }
                if (InputState.IsMouseButtonDown(right)) {
                    InputState.ConsumeMouseButtonDown(right);
                    shouldGoBack = true;
                }
                if (shouldGoBack)
                    yield return StateChange.Pop();

                if (InputState.IsKeyDown(KeyCode.Return)) {
                    InputState.ConsumeKeyDown(KeyCode.Return);
                    shouldStart = true;
                }

                if (shouldStart) {
                    shouldStart = false;
                    yield return StateChange.PopThenPush(1, new CampaignOverviewMissionLoadingState(view));
                }

                yield return StateChange.none;
            }
        }
    }

    public void Dispose() {
        if (missionView.TryGetVirtualCamera)
            missionView.TryGetVirtualCamera.enabled = false;
        if (missionView.text)
            missionView.text.enabled = true;
        view.missionPanel.SetActive(false);
        view.backButton.gameObject.SetActive(true);
    }
}

public static class InputState {

    public static int lastScrollWheelConsumptionFrame = -1;
    public static bool IsScrollWheelConsumed => lastScrollWheelConsumptionFrame == Time.frameCount;
    public static void ConsumeScrollWheel() {
        lastScrollWheelConsumptionFrame = Time.frameCount;
    }
    public static int ScrollWheel {
        get {
            if (IsScrollWheelConsumed)
                return 0;
            var value = Input.GetAxisRaw("Mouse ScrollWheel");
            return Mathf.Approximately(0, value) ? 0 : Mathf.RoundToInt(Mathf.Sign(value));
        }
    }

    public static Dictionary<KeyCode, int> lastKeyDownConsumptionFrame = new();
    public static bool IsKeyDownConsumed(KeyCode keyCode) {
        return lastKeyDownConsumptionFrame.TryGetValue(keyCode, out var frame) && frame == Time.frameCount;
    }
    public static void ConsumeKeyDown(KeyCode keyCode) {
        lastKeyDownConsumptionFrame[keyCode] = Time.frameCount;
    }
    public static bool IsKeyDown(KeyCode keyCode) {
        if (IsKeyDownConsumed(keyCode) || !Input.GetKeyDown(keyCode))
            return false;
        // if (consume)
        // ConsumeKeyDown(keyCode);
        return true;
    }

    public static int[] lastMouseDownConsumptionFrame = new int[10];
    public static bool IsMouseButtonDownConsumed(int button) {
        return lastMouseDownConsumptionFrame[button] == Time.frameCount;
    }
    public static void ConsumeMouseButtonDown(int button) {
        lastMouseDownConsumptionFrame[button] = Time.frameCount;
    }
    public static bool IsMouseButtonDown(int button) {
        if (IsMouseButtonDownConsumed(button) || !Input.GetMouseButtonDown(button))
            return false;
        return true;
    }
}

public class CampaignOverviewMissionLoadingState : IDisposableState {

    [Command]
    public static float fadeDuration = .25f;

    public CampaignOverviewView view;
    public CampaignOverviewMissionLoadingState(CampaignOverviewView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            PostProcessing.ColorFilter = Color.white;
            var fade = PostProcessing.Fade(Color.black, fadeDuration);
            while (fade.IsActive() && !fade.IsComplete())
                yield return StateChange.none;

            view.ShowLoadingSpinner = true;

            while (true) {
                if (InputState.IsKeyDown(KeyCode.Escape)) {
                    InputState.ConsumeKeyDown(KeyCode.Escape);
                    break;
                }
                yield return StateChange.none;
            }

            yield return StateChange.Pop();
        }
    }

    public void Dispose() {
        PostProcessing.ColorFilter = Color.white;
        view.ShowLoadingSpinner = false;
    }
}