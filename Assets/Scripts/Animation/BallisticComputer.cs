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
}