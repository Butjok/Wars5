using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Mathf;

public class BallisticComputer : MonoBehaviour {

	public enum Angle { Low, High }

	public Transform barrel;
	public Transform target;
	public Transform virtualTarget;
	public float velocity = 1;
	public Vector3 gravity = Vector3.down;
	public Angle angle = Angle.Low;
	public BallisticCurve? curveOption;
	[Header("Debug")] public bool showActualTrajectory = true;
	public bool showIdealTrajectory = false;

	public void Update() {
		if (BallisticCurve.Calculate(barrel.position, target.position, velocity, gravity, out var low, out var high)) {
			var curve = angle == Angle.Low ? low : high;
			virtualTarget.position = curve.from + curve.forward * Cos(curve.theta) + curve.up * Sin(curve.theta);
			curveOption = curve;
		}
		else
			curveOption = null;
	}

	private void OnDrawGizmosSelected() {

		if (showActualTrajectory) {
			var curve = BallisticCurve.From(barrel.position, barrel.forward, velocity, gravity);
			Gizmos.color = Color.red;
			foreach (var (a, b)in curve.Segments())
				Gizmos.DrawLine(a, b);
		}

		if (showIdealTrajectory && BallisticCurve.Calculate(barrel.position, target.position, velocity, gravity, out var low, out var high)) {
			var curve = angle == Angle.Low ? low : high;
			Gizmos.color = Color.yellow;
			foreach (var (a, b)in curve.Segments())
				Gizmos.DrawLine(a, b);
		}
	}
}