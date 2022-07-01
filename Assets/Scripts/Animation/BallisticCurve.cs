using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

[Serializable]
public struct BallisticCurve {

	public static bool Calculate(
		Vector3 from, Vector3 to, float velocity, Vector3 gravity,
		out BallisticCurve curveLow, out BallisticCurve curveHigh) {

		var up = -gravity.normalized;
		var deltaUp = Vector3.Project(to - from, up);
		var y = Vector3.Dot(up, deltaUp);
		var deltaForward = to - from - deltaUp;
		var x = deltaForward.magnitude;

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

		curveLow = new BallisticCurve(from, to, deltaForward, up, velocity, thetaLow, g);
		curveHigh = new BallisticCurve(from, to, deltaForward, up, velocity, thetaHigh, g);

		return true;
	}

	public float theta, velocity, gravity, totalTime;
	public Vector3 from, to, forward, up;

	public BallisticCurve(Vector3 from, Vector3 to, Vector3 projectedDelta, Vector3 up, float velocity, float theta, float gravity) {
		this.theta = theta;
		this.velocity = velocity;
		this.gravity = gravity;
		this.from = from;
		this.to = to;
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

	public IEnumerable<(Vector3, Vector3)> Segments(float timeStep, int maxSamples = 10000) {
		var lastPoint = from;
		var i = 0;
		for (var time = timeStep; time < totalTime && i < maxSamples; time += timeStep, i++) {
			var point = Sample(time);
			yield return (lastPoint, point);
			lastPoint = point;
		}
		yield return (lastPoint, to);
	}
}