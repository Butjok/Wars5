using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[ExecuteInEditMode]
[SelectionBase]
public class Body : MonoBehaviour {

	[Serializable]
	public class Axis {
		public Piston a, b;
	}

	public List<Axis> rollAxis = new();
	public List<Axis> pitchAxis = new();
	public List<Piston> pistons = new();

	[Range(-1,1)]public float rollAxisMultiplier=1;
	[Range(-1,1)]public float pitchAxisMultiplier=1;

	[ContextMenu(nameof(Start))]
	public void Start() {

		rollAxis.Clear();
		pitchAxis.Clear();
		pistons.Clear();
		
		var wheels = GetComponentInParent<UnitView>().GetComponentsInChildren<Wheel>();

		foreach (var wheel in wheels) {

			var piston = wheel.GetComponent<Piston>();
			if (!piston)
				Debug.LogError("no piston on the wheel", wheel);
			pistons.Add(piston);

			var wheelName = wheel.transform.parent.name;
			var side = wheelName[0];
			var index = int.Parse(wheelName[1..]);

			for (var i = rollAxis.Count; i <= index; i++)
				rollAxis.Add(new Axis());
			if (side == 'L')
				rollAxis[index].a = piston;
			if (side == 'R')
				rollAxis[index].b = piston;
		}

		for (var i = 1; i < rollAxis.Count; i++) {
			pitchAxis.Add(new Axis { a = rollAxis[i].a, b = rollAxis[i-1].a });
			pitchAxis.Add(new Axis { a = rollAxis[i].b, b = rollAxis[i-1].b });
		}

		pistons = pistons.Distinct().ToList();
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

		var center = average(pistons.Select(piston => piston.position));
		var right = average(rollAxis.Select(axis => (axis.b.position - axis.a.position).normalized * rollAxisMultiplier)).normalized;
		var forward = average(pitchAxis.Select(axis => (axis.a.position - axis.b.position).normalized * pitchAxisMultiplier)).normalized;

		transform.rotation = Quaternion.LookRotation(forward, Vector3.Cross(right, -forward));
		transform.position = center;
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.blue;
		foreach (var axis in rollAxis)
			Gizmos.DrawLine(axis.a.transform.position, axis.b.transform.position);
		Gizmos.color = Color.yellow;
		foreach (var axis in pitchAxis)
			Gizmos.DrawLine(axis.a.transform.position, axis.b.transform.position);
	}
}