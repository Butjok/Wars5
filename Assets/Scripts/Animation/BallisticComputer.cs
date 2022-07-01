using UnityEngine;
using static UnityEngine.Mathf;

public class BallisticComputer : MonoBehaviour {

	public enum Angle { Low, High }

	public Transform from;
	public Transform to;
	public Transform aimTarget;
	public float velocity;
	public Vector3 gravity = Vector3.down;
	public Angle angle = Angle.Low;
	public BallisticCurve? curveOption;
	public Transform barrel;
	public bool showActualTrajectory = true;
	public bool showIdealTrajectories = false;

	public void Update() {
		if (BallisticCurve.Calculate(@from.position, to.position, velocity, gravity, out var low, out var high)) {
			var curve = angle == Angle.Low ? low : high;
			aimTarget.position = curve.from + curve.forward * Cos(curve.theta) + curve.up * Sin(curve.theta);
			curveOption = curve;
		}
		else {
			curveOption = null;
		}
	}
	private void OnDrawGizmos() {

		if (showActualTrajectory && barrel) {
			var actualCurve = BallisticCurve.From(@from.position, barrel.forward, velocity, gravity);
			Gizmos.color = Color.blue;
			foreach (var (a, b)in actualCurve.Segments(.1f))
				Gizmos.DrawLine(a, b);
		}

		if (showIdealTrajectories && BallisticCurve.Calculate(@from.position, to.position, velocity, gravity, out var low, out var high)) {
			Gizmos.color = Color.yellow;
			foreach (var (a, b)in low.Segments(.1f))
				Gizmos.DrawLine(a, b);
			Gizmos.color = Color.blue;
			foreach (var (a, b)in high.Segments(.1f))
				Gizmos.DrawLine(a, b);
		}
	}
}