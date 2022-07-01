using UnityEngine;
using Color = UnityEngine.Color;

public class BallisticMotion : MonoBehaviour {

	public Transform target;
	public Vector3 gravity = Vector3.down;
	public float velocity = 1;
	public float timeStep = .1f;
	public float time;
	public bool possible;
	public BallisticCurve curve;
	public bool orient = true;
	public Vector3 up = Vector3.up;

	public void OnDrawGizmos() {
		if (possible) {
			Gizmos.color = Color.yellow;
			foreach (var (a, b) in curve.Segments(timeStep))
				Gizmos.DrawLine(a, b);
		}
	}
	public void Update() {
		if (!possible) {
			return;
		}
		transform.position = curve.Sample(time);
		if (orient) {
			var near = curve.Sample(time - Time.deltaTime);
			transform.rotation = Quaternion.LookRotation(transform.position - near, up);
		}
		time = (time + Time.deltaTime) % curve.totalTime;
	}
}