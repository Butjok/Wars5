using System;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Wheels : MonoBehaviour
{
	public WheelRaycaster[] wheelRaycasters = { };
	public LayerMask layerMask;
	public Transform body;
	public Axis[] rollAxis = { };
	public Axis[] pitchAxis = { };
	public float clearance;

	[Serializable]
	public struct WheelRaycaster
	{
		public Transform raycaster;
		public Transform wheel;
	}

	[Serializable]
	public struct Axis
	{
		public Transform a, b;
	}

	public void Update()
	{
		foreach (var pair in wheelRaycasters)
		{
			if (!pair.raycaster || !pair.wheel) continue;
			if (!Physics.Raycast(pair.raycaster.position, pair.raycaster.forward, out var hit, float.MaxValue, layerMask)) continue;
			pair.wheel.position = hit.point;
		}

		if (body)
		{
			var localAccumulator = Vector3.zero;
			int count = 0;
			foreach (var w in wheelRaycasters)
			{
				if (w.wheel)
				{
					localAccumulator += transform.InverseTransformPoint(w.wheel.position);
					count++;
				}
			}

			if (count > 0)
				body.position = transform.TransformPoint((localAccumulator / count)) + Vector3.up * clearance;

			var rollAngle = rollAxis.Average(axis =>
			{
				var left = transform.InverseTransformPoint(axis.a.position);
				var right = transform.InverseTransformPoint(axis.b.position);
				var direction = right - left;
				var angle = Vector3.SignedAngle(Vector3.right, direction, Vector3.forward);
				return angle;
			});

			var pitchAngle = pitchAxis.Average(axis =>
			{
				var front = transform.InverseTransformPoint(axis.a.position);
				var back = transform.InverseTransformPoint(axis.b.position);
				var direction = back - front;
				var angle = Vector3.SignedAngle(Vector3.back, direction, Vector3.right);
				return angle;
			});

			body.rotation = transform.rotation * Quaternion.Euler(pitchAngle, 0, rollAngle);
		}
	}
}