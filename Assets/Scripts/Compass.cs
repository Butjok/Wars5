using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour {
    public Image arrow;
    public float angle;
    public CameraRig cameraRig;
    private void Update() {
        if (cameraRig && arrow) {
            arrow.rectTransform.rotation = Quaternion.Euler(0, 0, cameraRig.transform.rotation.eulerAngles.y);
            //cameraRig.transform.rotation.eulerAngles;
        }
    }
    public void ResetRotation() {
        if (cameraRig)
            cameraRig.TryRotate(angle: 0);
    }
}