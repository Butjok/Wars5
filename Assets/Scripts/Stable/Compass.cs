using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour {
    public Image arrow;
    public float angle;
    private void Update() {
        CameraRig.TryFind(out var cameraRig);
        if (cameraRig && arrow) {
            arrow.rectTransform.rotation = Quaternion.Euler(0, 0, cameraRig.transform.rotation.eulerAngles.y);
            //cameraRig.transform.rotation.eulerAngles;
        }
    }
    public void ResetRotation() {
        CameraRig.TryFind(out var cameraRig);
        if (cameraRig)
            cameraRig.TryRotate(angle: 0);
    }
}