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

public class CampaignOverviewView : StateRunner {

    public Camera mainCamera;
    public CinemachineVirtualCamera defaultVirtualCamera;
    public CinemachineVirtualCamera missionVirtualCamera;
    public LayerMask mouseRaycastLayerMask;
    public GameObject loadingSpinner;
    public GameObject missionPanel;
    public TMP_Text missionName;
    public TMP_Text missionDescription;

    public IEnumerable<MissionView> MissionViews => GetComponentsInChildren<MissionView>();

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
}

public static class DefaultGuiSkin {
    public static GUISkin TryGet => Resources.Load<GUISkin>("CommandLine");
}

public class CampaignOverviewDefaultState : IDisposableState {

    [Command]
    public static bool drawDebugHit = false;
    
    public CampaignOverviewView view;
    public MissionView hoveredMissionView;

    public CampaignOverviewDefaultState(CampaignOverviewView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.defaultVirtualCamera.enabled = true;
            view.missionVirtualCamera.enabled = false;

            var campaign = PersistentData.Read().campaign;
            foreach (var missionView in view.MissionViews) {
                var missionName = missionView.MissionName;
                var isAvailable = campaign.IsAvailable(missionName);
                missionView.IsAvailable = isAvailable;
            }

            while (true) {
                yield return StateChange.none;

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

                        if (drawDebugHit)
                        using (Draw.ingame.WithDuration(1))
                            Draw.ingame.Cross(hit.point);

                        var name = hit.collider.name;
                        var parsed = Enum.TryParse(name, out MissionName missionName);
                        Assert.IsTrue(parsed, name);

                        if (campaign.IsAvailable(missionName))
                            yield return StateChange.Push(new CampaignOverviewMissionCloseUpState(view, hit.transform, missionName));
                        else
                            UiSound.Instance.notAllowed.PlayOneShot();
                    }
                }

                else if (hoveredMissionView) {
                    hoveredMissionView.Hovered = false;
                    hoveredMissionView = null;
                }
            }
        }
    }

    public void Dispose() {
        view.defaultVirtualCamera.enabled = false;
        view.missionVirtualCamera.enabled = false;
    }
}

public class CampaignOverviewMissionCloseUpState : IDisposableState {

    public CampaignOverviewView view;
    public Transform lookAtTarget;
    public MissionName missionName;

    public CampaignOverviewMissionCloseUpState(CampaignOverviewView view, Transform lookAtTarget, MissionName missionName) {
        this.view = view;
        this.lookAtTarget = lookAtTarget;
        this.missionName = missionName;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.missionVirtualCamera.enabled = true;
            view.missionVirtualCamera.LookAt = lookAtTarget;
            view.missionVirtualCamera.Follow = lookAtTarget;

            view.missionPanel.SetActive(true);
            view.missionName.text = Missions.GetName(missionName);
            view.missionDescription.text = Missions.GetDescription(missionName);

            while (true) {
                yield return StateChange.none;
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right))
                    yield return StateChange.Pop();
                else if (Input.GetKeyDown(KeyCode.Return))
                    yield return StateChange.PopThenPush(2, new CampaignOverviewMissionLoadingState(view));
            }
        }
    }

    public void Dispose() {
        view.missionVirtualCamera.enabled = false;
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