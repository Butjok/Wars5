using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class DepthOfFieldDistanceSetter : MonoBehaviour {

    public PostProcessProfile postProcessProfile;
    public DepthOfField depthOfField;

    public int priority;

    public Vector2 focalLengthBounds = new Vector2(1, 0);
    [FormerlySerializedAs("setConstantDistance")]
    public bool setConstant = false;
    public float constantDistance;
    [FormerlySerializedAs("focalLength")] public float constantFocalLength = 83;

    public static int lastFrame = -1;
    public static int lastPriority;

    private void Start() {
        
    }

    private void Update() {

        if (!postProcessProfile)
            postProcessProfile = Resources.Load<PostProcessProfile>("PostProcessProfile");
        if (postProcessProfile)
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
                    depthOfField.focusDistance.value = cameraRig.distance;
                    var t = (cameraRig.distance - cameraRig.distanceBounds[0]) / (cameraRig.distanceBounds[1] - cameraRig.distanceBounds[0]);
                    depthOfField.focalLength.value = Mathf.Lerp(focalLengthBounds[0], focalLengthBounds[1], t);
                }
            }
        }
    }
}