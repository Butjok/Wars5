using UnityEngine;
using Color = UnityEngine.Color;

public class BallisticMotion : MonoBehaviour {

	public Transform target;
	public Vector3 gravity = Vector3.down;
	public float velocity = 1;
	public float timeStep = .1f;
	public float time;
	public BallisticCurve curve;
	public bool orient = true;
	public Vector3 up = Vector3.up;

	public void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		foreach (var (a, b) in curve.Segments(timeStep))
			Gizmos.DrawLine(a, b);
	}
	public void Update() {
		transform.position = curve.Sample(time);
		if (orient) {
			var near = curve.Sample(time - Time.deltaTime);
			var delta = transform.position - near;
			if (delta != Vector3.zero)
				transform.rotation = Quaternion.LookRotation(delta, up);
		}
		time += Time.deltaTime;
		if (curve.totalTime is { } totalTime && time > totalTime)
			Destroy(gameObject);
	}
}