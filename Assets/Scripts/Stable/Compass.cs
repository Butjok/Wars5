using UnityEngine;
using UnityEngine.UI;
public class Compass : MonoBehaviour {
	public Image arrow;
	public float angle;
	private void Update() {
		var cameraRig = CameraRig.Instance;
		if (cameraRig&&arrow) {
			arrow.rectTransform.rotation=Quaternion.Euler(0,0,cameraRig.transform.rotation.eulerAngles.y);
			//cameraRig.transform.rotation.eulerAngles;
		}
	}
	public void ResetRotation() {
		var cameraRig = CameraRig.Instance;
		if (cameraRig)
			cameraRig.TryRotate(0);
	}
}