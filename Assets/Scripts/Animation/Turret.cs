using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Turret : MonoBehaviour {

	public RotateTo[] rotators = Array.Empty<RotateTo>();
	public Transform ballisticAim;
	public bool aim;
	public BallisticComputer ballisticComputer;
	public bool restIfImpossible = true;

	public void Start() {
		ballisticComputer = GetComponent<BallisticComputer>();
		Assert.IsTrue(ballisticComputer);
		Assert.IsTrue(ballisticAim);
	}
	public void Update() {
		foreach (var rotator in rotators) {
			if (aim && ballisticComputer.curveOption != null)
				rotator.target = ballisticAim;
			else if (!aim || restIfImpossible)
				rotator.target = null;
		}
	}
	private void OnDrawGizmos() {
		if (!ballisticComputer || ballisticComputer.curveOption is not { } curve) {
			return;
		}
		Gizmos.color = Color.yellow;
		foreach (var (a, b)in curve.Segments(.1f))
			Gizmos.DrawLine(a, b);
	}
}