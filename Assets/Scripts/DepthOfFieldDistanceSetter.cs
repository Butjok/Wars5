using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class DepthOfFieldDistanceSetter : MonoBehaviour {

    private DepthOfField depthOfField;
    public DepthOfField DepthOfField {
        get {
            if (!depthOfField) {
                var postProcessProfile = "PostProcessProfile1".LoadAs<PostProcessProfile>();
                depthOfField = postProcessProfile.GetSetting<DepthOfField>();
            }
            return depthOfField;
        }
    }

    public int priority;
    public Vector2 focalLengthBounds = new Vector2(1, 0);
    public bool setConstant = false;
    public float constantDistance;
    public float constantFocalLength = 83;
    public CameraRig cameraRig;

    public static int lastFrame = -1;
    public static int lastPriority;

    public void OnEnable() {
        DepthOfField.enabled.value = true;
    }
    public void OnDisable() {
        DepthOfField.enabled.value = false;
    }
}