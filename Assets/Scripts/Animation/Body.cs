using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Body : MonoBehaviour {

	[Serializable]
	public struct Axis {
		public Transform a, b;
	}

	public Axis[] rollAxis = { };
	public Axis[] pitchAxis = { };

	public List<Transform> wheels = new();
	public float clearance = 1;

	[ContextMenu(nameof(Awake))]
	public void Awake() {
		foreach (var axis in rollAxis) {
			wheels.Add(axis.a);
			wheels.Add(axis.b);
		}
		foreach (var axis in pitchAxis) {
			wheels.Add(axis.a);
			wheels.Add(axis.b);
		}
		wheels = wheels.Distinct().ToList();
	}

	public void Start() {
		if (Application.isPlaying)
		foreach (var wheel in wheels)
			wheel.transform.SetParent(null);
	}

	public void Update() {

		Vector3 average(IEnumerable<Vector3> vectors) {
			var sum = Vector3.zero;
			var count = 0;
			foreach (var vector in vectors) {
				sum += vector;
				count++;
			}
			return sum / count;
		}
		
		var center =average(wheels.Select(wheel => wheel.position)); 
		var right = average(rollAxis.Select(axis => (axis.b.position - axis.a.position).normalized)).normalized;
		var forward = average(pitchAxis.Select(axis => (axis.a.position - axis.b.position).normalized)).normalized;
		
		transform.rotation = Quaternion.LookRotation(forward,Vector3.Cross(right,-forward));
		transform.position = center + transform.up * clearance;
	}
}