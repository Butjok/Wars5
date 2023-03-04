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
using UnityEngine.UI;

public class CampaignOverviewView : StateRunner {

    public Camera mainCamera;
    public CinemachineVirtualCamera defaultVirtualCamera;
    public LayerMask mouseRaycastLayerMask;
    public GameObject loadingSpinner;
    public GameObject missionPanel;
    public TMP_Text missionName;
    public TMP_Text missionDescription;
    public Button startMissionButton;

    public MissionView[] MissionViews => GetComponentsInChildren<MissionView>();

    private void Start() {
        PushState(new CampaignOverviewDefaultState(this));
    }

    public bool ShowLoadingSpinner {
        set => loadingSpinner.SetActive(value);
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label(string.Join(" > ", stateNames.Reverse()));
    }
    
    public void StartMission() {
        CampaignOverviewMissionCloseUpState.shouldStart = true;
    }
    
    public void CycleMission(int offset) {
        var missionNames = MissionViews.Select(mv => mv.MissionName).ToList();
        var index = missionNames.IndexOf(CampaignOverviewMissionCloseUpState.missionName);
        Assert.AreNotEqual(-1,index);
        var nextIndex = (index + offset).PositiveModulo(missionNames.Count);
        CampaignOverviewDefaultState.targetMissionName = missionNames[nextIndex];
    }
}

public static class DefaultGuiSkin {
    public static GUISkin TryGet => Resources.Load<GUISkin>("CommandLine");
}

public class CampaignOverviewDefaultState : IDisposableState {

    [Command]
    public static bool drawDebugHit = false;
    
     public static MissionName? targetMissionName;

    public CampaignOverviewView view;
    public MissionView hoveredMissionView;

    public CampaignOverviewDefaultState(CampaignOverviewView view) {
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
                yield return StateChange.none;

                if (targetMissionName is {} actualMissionName) {
                    targetMissionName = null;
                    var missionView = view.MissionViews.Single(mv => mv.MissionName == actualMissionName);
                    yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(view, missionView, actualMissionName));
                }

                else {
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

                        if (Input.GetMouseButtonDown(Mouse.left)) {

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

                            //if (campaign.IsAvailable(missionName))
                                yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(view, missionView, missionName));
                            //else
                                //UiSound.Instance.notAllowed.PlayOneShot();
                        }
                    }

                    else if (hoveredMissionView) {
                        hoveredMissionView.Hovered = false;
                        hoveredMissionView = null;
                    }
                }
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

            while (true) {
                yield return StateChange.none;

                var scrollWheel = Input.GetAxisRaw("Mouse ScrollWheel");
                
                // if we should switch to other mission
                if (CampaignOverviewDefaultState.targetMissionName is { })
                    yield return StateChange.Pop();
                
                else if (scrollWheel != 0)
                    view.CycleMission(Mathf.RoundToInt(Mathf.Sign(scrollWheel)));

                else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right))
                    yield return StateChange.Pop();
                    
                else if (shouldStart || Input.GetKeyDown(KeyCode.Return)) {
                    shouldStart = false;
                    yield return StateChange.PopThenPush(2, new CampaignOverviewMissionLoadingState(view));
                }
            }
        }
    }

    public void Dispose() {
        if (missionView.TryGetVirtualCamera)
            missionView.TryGetVirtualCamera.enabled = false;
        if (missionView.text)
            missionView.text.enabled = true;
        view.missionPanel.SetActive(false);
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

            while (!Input.GetKeyDown(KeyCode.Escape))
                yield return StateChange.none;

            yield return StateChange.Pop();
        }
    }

    public void Dispose() {
        PostProcessing.ColorFilter = Color.white;
        view.ShowLoadingSpinner = false;
    }
}