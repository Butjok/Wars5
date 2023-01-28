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
	public float startSpinSpeed = 360;
	public float spinDeceleration = 1;
	public float spinAngle = 0;
	public bool fallingDown = false;

	public void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		foreach (var (a, b) in curve.Segments(timeStep))
			Gizmos.DrawLine(a, b);
	}

	public float lastHeight = float.MinValue;
	
	public void Update() {

		if (transform.position.y < lastHeight && !fallingDown) {
			fallingDown = true;
			Debug.Log("FALLING");
		}
		lastHeight = transform.position.y;
		
		var spinSpeed = Mathf.Max(0, startSpinSpeed - spinDeceleration * time);
		spinAngle += spinSpeed * Time.deltaTime;
		// transform.rotation *= Quaternion.AngleAxis(spinSpeed*Time.deltaTime, transform.forward);
		
		transform.position = curve.Sample(time);
		if (orient) {
			var near = curve.Sample(time - Time.deltaTime);
			var delta = transform.position - near;
			if (delta != Vector3.zero)
				transform.rotation = Quaternion.LookRotation(delta, up) * Quaternion.Euler(0,0,spinAngle);
		}
		time += Time.deltaTime;
		if (curve.totalTime is { } totalTime && time > totalTime)
			Destroy(gameObject);
	}
}