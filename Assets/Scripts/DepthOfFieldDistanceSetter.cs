using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class DepthOfFieldDistanceSetter : MonoBehaviour {

    private PostProcessProfile postProcessProfile;
    private DepthOfField depthOfField;

    public int priority;
    public Vector2 focalLengthBounds = new Vector2(1, 0);
    public bool setConstant = false;
    public float constantDistance;
    public float constantFocalLength = 83;

    public static int lastFrame = -1;
    public static int lastPriority;

    private void Update() {

        if (!postProcessProfile)
            postProcessProfile = Resources.Load<PostProcessProfile>("PostProcessProfile");

        Assert.IsTrue(postProcessProfile);
        depthOfField = postProcessProfile.GetSetting<DepthOfField>();

        if (depthOfField) {
            if (Time.frameCount != lastFrame) {
                lastFrame = Time.frameCount;
                lastPriority = int.MinValue;
            }
            if (priority > lastPriority) {
                if (setConstant) {
                    depthOfField.focusDistance.value = constantDistance;
                    depthOfField.focalLength.value = constantFocalLength;
                }
                else if (CameraRig.TryFind(out var cameraRig)) {
                    depthOfField.focusDistance.value = cameraRig.Distance;
                    var t = (cameraRig.Distance - cameraRig.distanceClamp[0]) / (cameraRig.distanceClamp[1] - cameraRig.distanceClamp[0]);
                    depthOfField.focalLength.value = Mathf.Lerp(focalLengthBounds[0], focalLengthBounds[1], t);
                }
            }
        }
    }
}