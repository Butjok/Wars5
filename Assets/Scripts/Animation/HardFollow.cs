using UnityEngine;

public class HardFollow:MonoBehaviour {
	public Transform target;
	public void LateUpdate() {
		transform.position = target.position;
	}
}