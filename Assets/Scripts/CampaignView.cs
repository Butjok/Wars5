using System;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CampaignView : MonoBehaviour {

    [Serializable]
    public class Mission {
        public string name;
        public string humanFriendlyName;
        public MeshRenderer[] renderers = { };
        public string availableUniformName = "_Available";
        public string completedUniformName = "_Completed";
        public BoxCollider collider;
        public TMP_Text text;
        public Color availableTextColor = Color.white;
        public Color unavailableTextColor = Color.grey;
        public Color completedTextColor = Color.green;
    }
    
    [Serializable]
    public class Link {
        public string from;
        public string to;
        public LineRenderer lineRenderer;
        public string lengthUniformName = "_Length";
        public string animationStartTimeUniformName = "_StartTime";
    }

    [FormerlySerializedAs("missionViews")] public Mission[] missions = { new() };
    public Link[] links = { };

    public UIFrame selectionFrame;

    public Mission Find(Collider collider) {
        return missions.SingleOrDefault(mv => mv.collider == collider);
    }

    public void Actualize(Campaign campaign) {
        
        var propertyBlock = new MaterialPropertyBlock();
        
        foreach (var missionView in missions) {

            var isCompleted = campaign[missionView.name].isCompleted;
            var isAvailable = campaign.IsAvailable(missionView.name);
            
            if (missionView.text) {
                missionView.text.text = missionView.humanFriendlyName;

                if (isCompleted)
                    missionView.text.color = missionView.completedTextColor;
                else if (isAvailable)
                    missionView.text.color = missionView.availableTextColor;
                else
                    missionView.text.color = missionView.unavailableTextColor;
            }
            
            propertyBlock.Clear();
            propertyBlock.SetFloat(missionView.completedUniformName, isCompleted ? 1 : 0);
            propertyBlock.SetFloat(missionView.availableUniformName, isAvailable ? 1 : 0);
            
            foreach (var renderer in missionView.renderers)
                renderer.SetPropertyBlock(propertyBlock);
        }
    }

    public bool TryAnimateLink(string from, string to) {
        
        var link = links.SingleOrDefault(l => l.from == from && l.to == to);
        if (link == null || !link.lineRenderer)
            return false;

        var lineRenderer = link.lineRenderer;
        var length = 0f;
        for (var i = 1; i < lineRenderer.positionCount; i++)
            length += Vector3.Distance(lineRenderer.GetPosition(i - 1), lineRenderer.GetPosition(i));

        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetFloat(link.lengthUniformName, length);
        propertyBlock.SetFloat(link.animationStartTimeUniformName, Time.timeSinceLevelLoad);

        return true;
    }

    public void Deselect() {
        selectionFrame.gameObject.SetActive(false);
    }
    public void Select(Mission mission) {
        Deselect();
        selectionFrame.gameObject.SetActive(true);
        selectionFrame.JumpTo(mission.collider,selectionFrameJumpDuration);
    }

    private void Start() {
        // Campaign.Clear();
        Actualize(Campaign.Load());
    }

    public float selectionFrameJumpDuration = .25f;
    public int index = -1;
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            index = (index + 1) % missions.Length;
            Select(missions[index]);
        }
    }

    public void StartMission() {
        
    }
    public void Cancel() {
        
    }
    
    public void Show(){}
    public void Hide() { }

    public LineRenderer lineRenderer;
    [Button]
    public void AnimateLineRenderer() {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetFloat("_StartTime", Time.timeSinceLevelLoad);
        lineRenderer.SetPropertyBlock(propertyBlock);
    }
}