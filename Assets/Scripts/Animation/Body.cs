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

	[Range(-1, 1)] public float rollAxisMultiplier = 1;
	[Range(-1, 1)] public float pitchAxisMultiplier = 1;

	[ContextMenu(nameof(Start))]
	public void Start() {

		rollAxis.Clear();
		pitchAxis.Clear();
		pistons.Clear();

		var wheels = GetComponentInParent<UnitView>().GetComponentsInChildren<Wheel>();

		foreach (var wheel in wheels) {

			var piston = wheel.GetComponent<Piston>();
			if (!piston)
				continue;
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
			pitchAxis.Add(new Axis { a = rollAxis[i].a, b = rollAxis[i - 1].a });
			pitchAxis.Add(new Axis { a = rollAxis[i].b, b = rollAxis[i - 1].b });
		}

		pistons = pistons.Distinct().ToList();
	}

	public void Update() {

		if (pistons.Count > 0) {
			var center = Vector3.zero;
			foreach (var piston in pistons)
				center += piston.position;
			center /= pistons.Count;
			transform.position = center;
		}

		if (rollAxis.Count > 0 && pitchAxis.Count > 0) {

			var right = Vector3.zero;
			foreach (var axis in rollAxis) 
				right += (axis.b.position - axis.a.position).normalized;
			right *= rollAxisMultiplier / rollAxis.Count;

			var forward = Vector3.zero;
			foreach (var axis in pitchAxis) 
				forward += (axis.a.position - axis.b.position).normalized;
			forward *= pitchAxisMultiplier / pitchAxis.Count;

			transform.rotation = Quaternion.LookRotation(forward, Vector3.Cross(right, -forward));
		}
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