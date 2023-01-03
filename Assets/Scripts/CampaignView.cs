using System;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class CampaignView : MonoBehaviour {

    [Serializable]
    public class MissionView {
        public string name;
        public string humanFriendlyName;
        public MeshRenderer[] renderers = { };
        public string availableUniformName = "_Available";
        public string completedUniformName = "_Completed";
        public Collider collider;
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

    public MissionView[] missionViews = { };
    public Link[] links = { };

    public MissionView FindMissionView(Collider collider) {
        return missionViews.SingleOrDefault(mv => mv.collider == collider);
    }

    public void ActualizeMissionViews(Campaign campaign) {
        
        var propertyBlock = new MaterialPropertyBlock();
        
        foreach (var missionView in missionViews) {

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

    public LineRenderer lineRenderer;
    [Button]
    public void AnimateLineRenderer() {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetFloat("_StartTime", Time.timeSinceLevelLoad);
        lineRenderer.SetPropertyBlock(propertyBlock);
    }
}