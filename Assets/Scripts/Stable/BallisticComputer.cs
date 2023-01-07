using UnityEngine;
using static UnityEngine.Mathf;

public class BallisticComputer : MonoBehaviour {

	public enum Angle { Low, High }

	[SerializeField] private Transform target;
	[Space] public Transform barrel;
	public Transform virtualTarget;
	public float velocity = 1;
	public Vector3 gravity = Vector3.down;
	public Angle angle = Angle.Low;
	public BallisticCurve? curve;
	[Header("Debug")] public bool showActualTrajectory = true;
	public bool showIdealTrajectory = false;

	public Transform Target {
		get => target;
		set {
			target = value;
			UpdateBallisticCurve();
		}
	}

	public void UpdateBallisticCurve() {
		if (Target && BallisticCurve.Calculate(barrel.position, Target.position, velocity, gravity, out var low, out var high)) {
			var curve = angle == Angle.Low ? low : high;
			virtualTarget.position = curve.from + curve.forward * Cos(curve.theta) + curve.up * Sin(curve.theta);
			this.curve = curve;
		}
		else
			curve = null;
	}

	public void Update() {
		UpdateBallisticCurve();
	}

	private void OnDrawGizmosSelected() {

		if (showActualTrajectory) {
			var curve = BallisticCurve.From(barrel.position, barrel.forward, velocity, gravity);
			Gizmos.color = Color.red;
			foreach (var (a, b)in curve.Segments())
				Gizmos.DrawLine(a, b);
		}

		if (showIdealTrajectory && Target && BallisticCurve.Calculate(barrel.position, Target.position, velocity, gravity, out var low, out var high)) {
			var curve = angle == Angle.Low ? low : high;
			Gizmos.color = Color.yellow;
			foreach (var (a, b)in curve.Segments())
				Gizmos.DrawLine(a, b);
		}
	}
}