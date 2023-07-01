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
    public Button backButton, previousButton, nextButton;

    public IEnumerable<MissionView> MissionViews => GetComponentsInChildren<MissionView>();

    public bool start = true;

    private void Start() {
        var game = Game.Instance;
        if (start && game.stateMachine.Count == 0)
            game.stateMachine.Push(new CampaignOverviewState2(game.stateMachine));
    }

    public bool ShowLoadingSpinner {
        set => loadingSpinner.SetActive(value);
    }

    public void StartMission() {
        CampaignOverviewMissionCloseUpState.shouldStart = true;
    }

    public void CycleMission(int offset) {
        var campaign = PersistentData.Read().campaign;
        var missionNames = MissionViews.Select(mv => mv.MissionName);
        var availableMissionNames = missionNames.Where(campaign.IsAvailable).ToList();
        var index = availableMissionNames.IndexOf(CampaignOverviewMissionCloseUpState.missionName);
        var nextIndex = (index + offset).PositiveModulo(availableMissionNames.Count);
        CampaignOverviewSelectionState.targetMissionName = availableMissionNames[nextIndex];
    }

    public void GoToMainMenu() {
        CampaignOverviewSelectionState.shouldGoBackToMainMenu = true;
    }
}

public class CampaignOverviewState2 : StateMachineState {

    [Command] public static string sceneName = "Campaign";
    [Command] public static float fadeDuration = .25f;
    [Command] public static Ease fadeEasing = Ease.Unset;

    public CampaignOverviewState2(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {

            if (CameraFader.IsBlack == false) {
                var completed = CameraFader.FadeToBlack();
                while (!completed())
                    yield return StateChange.none;
            }

            if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                yield return StateChange.none;
            }

            CameraFader.FadeToWhite();
            
            var view = Object.FindObjectOfType<CampaignOverviewView>();
            Assert.IsTrue(view);

            //PostProcessing.ColorFilter = Color.black;
            //PostProcessing.Fade(Color.white, fadeDuration, fadeEasing);

            yield return StateChange.Push(new CampaignOverviewSelectionState(stateMachine, view));
        }
    }
}

public class CampaignOverviewSelectionState : StateMachineState {

    [Command] public static float fadeDuration = .25f;
    [Command] public static Ease fadeEasing = Ease.Unset;
    [Command] public static bool drawDebugHit = false;
    [Command] public static bool shouldGoBackToMainMenu = false;

    public static MissionName? targetMissionName;

    public CampaignOverviewView view;
    public MissionView hoveredMissionView;

    public CampaignOverviewSelectionState(StateMachine stateMachine, CampaignOverviewView view) : base(stateMachine) {
        this.view = view;
    }

    public override IEnumerator<StateChange> Enter {
        get {

            view.backButton.onClick.RemoveAllListeners();
            view.backButton.onClick.AddListener(view.GoToMainMenu);

            view.defaultVirtualCamera.enabled = true;

            var campaign = PersistentData.Read().campaign;
            foreach (var missionView in view.MissionViews) {
                var missionName = missionView.MissionName;
                var isAvailable = campaign.IsAvailable(missionName);
                missionView.IsAvailable = isAvailable;
            }

            while (true) {

                if (InputState.TryConsumeKeyDown(KeyCode.Tab))
                    view.CycleMission(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                if (InputState.TryConsumeScrollWheel(out var scrollWheel))
                    view.CycleMission(scrollWheel);

                if (targetMissionName is { } actualMissionName) {
                    targetMissionName = null;
                    var missionView = view.MissionViews.Single(mv => mv.MissionName == actualMissionName);
                    yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(stateMachine, view, missionView, actualMissionName));
                    continue;
                }

                shouldGoBackToMainMenu = InputState.TryConsumeKeyDown(KeyCode.Escape) || shouldGoBackToMainMenu;

                if (shouldGoBackToMainMenu) {
                    shouldGoBackToMainMenu = false;

                    var completed = CameraFader.FadeToBlack();
                    while(!completed())
                        yield return StateChange.none;
                    yield return StateChange.PopThenPush(2, new MainMenuState2(stateMachine, false, false));
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
                        InputState.TryConsumeMouseButtonDown(left);

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

                        if (campaign.IsAvailable(missionName)) {
                            yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(stateMachine, view, missionView, missionName));
                            continue;
                        }
                        else
                            UiSound.Instance.notAllowed.PlayOneShot();
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

    public override void Exit() {
        view.defaultVirtualCamera.enabled = false;
    }
}

public class CampaignOverviewMissionCloseUpState : StateMachineState {

    public static MissionName missionName;
    public static bool shouldStart;

    public CampaignOverviewView view;
    public MissionView missionView;

    public CampaignOverviewMissionCloseUpState(StateMachine stateMachine, CampaignOverviewView view, MissionView missionView, MissionName missionName) : base(stateMachine) {
        this.view = view;
        this.missionView = missionView;
        CampaignOverviewMissionCloseUpState.missionName = missionName;
    }

    public override IEnumerator<StateChange> Enter {
        get {

            var campaign = PersistentData.Read().campaign;
            var availableMissionsCount = view.MissionViews.Count(mv => campaign.IsAvailable(mv.MissionName));
            view.previousButton.interactable = view.nextButton.interactable = availableMissionsCount > 1;

            view.missionPanel.SetActive(true);
            view.missionName.text = Strings.GetName(missionName);
            view.missionDescription.text = Strings.GetDescription(missionName);

            var isAvailable = campaign.IsAvailable(missionName);
            view.startMissionButton.interactable = isAvailable;

            if (missionView.TryGetVirtualCamera)
                missionView.TryGetVirtualCamera.enabled = true;

            if (missionView.text)
                missionView.text.enabled = false;

            var shouldGoBack = false;
            view.backButton.onClick.RemoveAllListeners();
            view.backButton.onClick.AddListener(() => shouldGoBack = true);

            while (true) {

                if (InputState.TryConsumeScrollWheel(out var scrollWheel))
                    view.CycleMission(scrollWheel);

                if (InputState.TryConsumeKeyDown(KeyCode.Tab))
                    view.CycleMission(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                // if we should switch to other mission
                if (CampaignOverviewSelectionState.targetMissionName is { })
                    yield return StateChange.Pop();

                shouldGoBack = InputState.TryConsumeKeyDown(KeyCode.Escape) || shouldGoBack;
                shouldGoBack = InputState.TryConsumeMouseButtonDown(right) || shouldGoBack;

                if (shouldGoBack)
                    yield return StateChange.Pop();

                shouldStart = InputState.TryConsumeKeyDown(KeyCode.Return) || shouldStart;
                shouldStart = InputState.TryConsumeKeyDown(KeyCode.Space) || shouldStart;

                if (shouldStart) {
                    shouldStart = false;
                    if (isAvailable) {
                        var completed = CameraFader.FadeToBlack();
                        while(!completed())
                            yield return StateChange.none;
                        yield return StateChange.PopThenPush(3, new LoadingState(stateMachine, missionName, Campaign.Mission.GetInputCode(missionName), true));
                        continue;
                    }
                    UiSound.Instance.notAllowed.PlayOneShot();
                }

                yield return StateChange.none;
            }
        }
    }

    public override void Exit() {
        if (missionView.TryGetVirtualCamera)
            missionView.TryGetVirtualCamera.enabled = false;
        if (missionView.text)
            missionView.text.enabled = true;
        view.missionPanel.SetActive(false);

        view.backButton.onClick.RemoveAllListeners();
        view.backButton.onClick.AddListener(view.GoToMainMenu);

        missionName = MissionName.None;
    }
}