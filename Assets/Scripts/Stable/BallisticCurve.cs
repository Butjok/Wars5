using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

[Serializable]
public struct BallisticCurve {

	public static bool TryCalculate(
		Vector3 from, Vector3 to, float velocity, Vector3 gravity,
		out BallisticCurve curveLow, out BallisticCurve curveHigh) {

		var up = -gravity.normalized;
		var deltaUp = Vector3.Project(to - from, up);
		var deltaForward = to - from - deltaUp;

		var x = deltaForward.magnitude;
		var y = Vector3.Dot(up, deltaUp);
		var v = velocity;
		var g = gravity.magnitude;

		var d = v * v * v * v - g * (g * x * x + 2 * y * v * v);
		if (d < Epsilon) {
			curveLow = curveHigh = default;
			return false;
		}

		var a1 = Atan2(v * v + Sqrt(d), g * x);
		var a2 = Atan2(v * v - Sqrt(d), g * x);

		var thetaLow = Min(a1, a2);
		var thetaHigh = Max(a1, a2);

		curveLow = new BallisticCurve(from, deltaForward, up, velocity, thetaLow, g);
		curveHigh = new BallisticCurve(from, deltaForward, up, velocity, thetaHigh, g);

		return true;
	}

	public static BallisticCurve From(Vector3 from, Vector3 barrelForward, float velocity, Vector3 gravity) {

		var up = -gravity.normalized;
		var deltaUp = Vector3.Project(barrelForward, up);
		var deltaForward = barrelForward - deltaUp;

		var x = deltaForward.magnitude;
		var y = Vector3.Dot(up, deltaUp);
		var v = velocity;
		var g = gravity.magnitude;

		var theta = Atan2(y, x);
		return new BallisticCurve(from, deltaForward, up, velocity, theta, g) { totalTime = null };
	}

	public float theta, velocity, gravity;
	public float? totalTime;
	public Vector3 from, forward, up;

	public BallisticCurve(Vector3 from, Vector3 projectedDelta, Vector3 up, float velocity, float theta, float gravity) {
		this.theta = theta;
		this.velocity = velocity;
		this.gravity = gravity;
		this.from = from;
		forward = projectedDelta.normalized;
		this.up = up;
		totalTime = projectedDelta.magnitude / (velocity * Cos(theta));
	}

	public Vector3 Sample(float time) {
		var v = velocity;
		var x = forward * v * Cos(theta) * time;
		var y = up * (v * Sin(theta) * time - gravity * time * time / 2);
		return from + x + y;
	}

	public IEnumerable<(Vector3, Vector3)> Segments(float timeStep = .1f, int maxSamples = 100) {
		var lastPoint = from;
		for (var i = 0; i < maxSamples; i++) {
			var time = timeStep * (i + 1);
			if (totalTime is { } totalTime2 && time > totalTime2)
				break;
			var point = Sample(time);
			yield return (lastPoint, point);
			lastPoint = point;
		}
	}
}