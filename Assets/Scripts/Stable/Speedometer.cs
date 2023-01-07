using UnityEngine;

public class Speedometer : MonoBehaviour {

	private Vector3? oldPosition;
	public Vector3? deltaPosition;
	public void Update() {
		if (oldPosition is { } position) {
			var delta = transform.position - position;
			var length = delta.magnitude;
			speed = length / Time.deltaTime;
			deltaPosition = delta;
		}
		oldPosition = transform.position;
	}
	public void Clear() {
		oldPosition = deltaPosition = null;
		speed = 0;
	}

	public double speed;
}