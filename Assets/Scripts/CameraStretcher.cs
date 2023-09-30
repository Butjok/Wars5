using UnityEngine;

[ExecuteInEditMode]
public class CameraStretcher : MonoBehaviour {

    public Camera camera;

    public void Awake() {
        camera = GetComponent<Camera>();
    }

    public void LateUpdate() {
        camera.ResetProjectionMatrix();
        var m = camera.projectionMatrix;
        m.m11 *= CameraRig.verticalStretch;
        camera.projectionMatrix = m;
    }
}