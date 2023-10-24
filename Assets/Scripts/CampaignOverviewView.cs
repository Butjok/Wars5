using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using Butjok.CommandLine;
using Cinemachine;
using DG.Tweening;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Mouse;
using Button = UnityEngine.UI.Button;

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
    public Campaign campaign;
    public Mission selected;
    public Action startMission, goToMainMenu;
    public Action<int> cycleMission;

    public IEnumerable<MissionView> MissionViews => GetComponentsInChildren<MissionView>();

    public bool start = true;

    private void Start() {
        var game = Game.Instance;
        if (start && game.stateMachine.Count == 0)
            game.EnqueueCommand(GameSessionState.Command.StartCampaignOverview);
    }

    public bool ShowLoadingSpinner {
        set => loadingSpinner.SetActive(value);
    }

    public void StartMission() {
        startMission?.Invoke();
    }
    public void CycleMission(int offset) {
        cycleMission?.Invoke(offset);
    }
    public void GoToMainMenu() {
        goToMainMenu?.Invoke();
    }
}

public class CampaignOverviewState2 : StateMachineState {

    [Command] public static string sceneName = "Campaign";

    public CampaignOverviewView view;

    public CampaignOverviewState2(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            if (SceneManager.GetActiveScene().name != sceneName)
                SceneManager.LoadScene(sceneName);

            view = Object.FindObjectOfType<CampaignOverviewView>();
            while (!view) {
                yield return StateChange.none;
                view = Object.FindObjectOfType<CampaignOverviewView>();
            }

            yield return StateChange.Push(new CampaignOverviewSelectionState(stateMachine));
        }
    }
}

public class CampaignOverviewSelectionState : StateMachineState {

    [Command] public static float fadeDuration = .25f;
    [Command] public static Ease fadeEasing = Ease.Unset;
    [Command] public static bool drawDebugHit = false;

    public MissionView targetMissionView;
    public MissionView hoveredMissionView;
    public bool shouldGoBackToMainMenu = false;

    public CampaignOverviewSelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public void CycleMission(int offset) {
        var campaign = stateMachine.Find<GameSessionState>().persistentData.campaign;
        var view = stateMachine.Find<CampaignOverviewState2>().view;
        var availableMissionViews = view.MissionViews.Where(missionView => campaign.GetMission(missionView.MissionType).IsAvailable).ToList();
        var index = availableMissionViews.IndexOf(stateMachine.TryFind<CampaignOverviewMissionCloseUpState>()?.missionView);
        var nextIndex = (index + offset).PositiveModulo(availableMissionViews.Count);
        targetMissionView = availableMissionViews[nextIndex];
    }

    public override IEnumerator<StateChange> Enter {
        get {

            var view = stateMachine.Find<CampaignOverviewState2>().view;

            view.backButton.onClick.RemoveAllListeners();
            view.backButton.onClick.AddListener(() => shouldGoBackToMainMenu = true);

            view.cycleMission = CycleMission;

            view.defaultVirtualCamera.enabled = true;

            var campaign = stateMachine.Find<GameSessionState>().persistentData.campaign;
            foreach (var missionView in view.MissionViews)
                missionView.IsAvailable = campaign.GetMission(missionView.MissionType).IsAvailable;

            while (true) {

                if (InputState.TryConsumeKeyDown(KeyCode.Tab))
                    CycleMission(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                if (InputState.TryConsumeScrollWheel(out var scrollWheel))
                    CycleMission(scrollWheel);

                if (targetMissionView) {
                    var target = targetMissionView;
                    targetMissionView = null;
                    yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(stateMachine, target));
                    continue;
                }

                shouldGoBackToMainMenu = InputState.TryConsumeKeyDown(KeyCode.Escape) || shouldGoBackToMainMenu;

                if (shouldGoBackToMainMenu) {
                    shouldGoBackToMainMenu = false;

                    var completed = CameraFader.FadeToBlack();
                    while (!completed())
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

                        var missionType = missionView.MissionType;
                        if (campaign.GetMission(missionType).IsAvailable) {
                            yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(stateMachine, missionView));
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
        stateMachine.Find<CampaignOverviewState2>().view.defaultVirtualCamera.enabled = false;
    }
}

public class CampaignOverviewMissionCloseUpState : StateMachineState {

    public bool shouldStart;

    public MissionView missionView;

    public CampaignOverviewMissionCloseUpState(StateMachine stateMachine, MissionView missionView) : base(stateMachine) {
        this.missionView = missionView;
    }

    public override IEnumerator<StateChange> Enter {
        get {

            var view = stateMachine.Find<CampaignOverviewState2>().view;
            var campaign = stateMachine.Find<GameSessionState>().persistentData.campaign;

            var availableMissionsCount = campaign.Missions.Count(mission => mission.IsAvailable);
            view.previousButton.interactable = view.nextButton.interactable = availableMissionsCount > 1;

            var mission = campaign.GetMission(missionView.MissionType);

            view.missionPanel.SetActive(true);
            view.missionName.text = mission.Name;
            view.missionDescription.text = mission.Description;

            view.startMission = () => shouldStart = true;
            view.startMissionButton.interactable = mission.IsAvailable;

            if (missionView.TryGetVirtualCamera)
                missionView.TryGetVirtualCamera.enabled = true;

            if (missionView.label)
                missionView.label.Alpha = 0;

            var shouldGoBack = false;
            view.backButton.onClick.RemoveAllListeners();
            view.backButton.onClick.AddListener(() => shouldGoBack = true);

            while (true) {

                if (InputState.TryConsumeScrollWheel(out var scrollWheel))
                    view.CycleMission(scrollWheel);

                if (InputState.TryConsumeKeyDown(KeyCode.Tab))
                    view.CycleMission(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                // if we should switch to other mission
                if (stateMachine.Find<CampaignOverviewSelectionState>().targetMissionView)
                    yield return StateChange.Pop();

                shouldGoBack = InputState.TryConsumeKeyDown(KeyCode.Escape) || shouldGoBack;
                shouldGoBack = InputState.TryConsumeMouseButtonDown(right) || shouldGoBack;

                if (shouldGoBack)
                    yield return StateChange.Pop();

                shouldStart = InputState.TryConsumeKeyDown(KeyCode.Return) || shouldStart;
                shouldStart = InputState.TryConsumeKeyDown(KeyCode.Space) || shouldStart;

                if (shouldStart) {
                    shouldStart = false;
                    if (mission.IsAvailable) {
                        var completed = CameraFader.FadeToBlack();
                        while (!completed())
                            yield return StateChange.none;
                        yield return StateChange.PopThenPush(3, new LoadingState(stateMachine, new SavedMission { mission = mission, input = LevelEditorFileSystem.TryReadLatest("autosave") }));
                        continue;
                    }
                    UiSound.Instance.notAllowed.PlayOneShot();
                }

                yield return StateChange.none;
            }
        }
    }

    public override void Exit() {
        
        var view = stateMachine.Find<CampaignOverviewState2>().view;
        
        if (missionView.TryGetVirtualCamera)
            missionView.TryGetVirtualCamera.enabled = false;
        if (missionView.label)
            missionView.label.Alpha = 1;
        view.missionPanel.SetActive(false);

        view.backButton.onClick.RemoveAllListeners();
        view.backButton.onClick.AddListener(view.GoToMainMenu);

    }
}