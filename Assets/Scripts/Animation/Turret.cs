using System;
using UnityEngine;
using UnityEngine.Assertions;

[SelectionBase]
[RequireComponent(typeof(BallisticComputer))]
public class Turret : MonoBehaviour {

	public bool aim;
	[Space]
	public Rotator[] rotators = Array.Empty<Rotator>();
	public BallisticComputer ballisticComputer;
	public bool restIfImpossible = true;
	public Transform restTarget;

	public void Start() {
		ballisticComputer = GetComponent<BallisticComputer>();
		Assert.IsTrue(ballisticComputer);
	}
	public void Update() {
		var possible = ballisticComputer.curveOption != null;
		foreach (var rotator in rotators) {
			if (aim && possible)
				rotator.target = ballisticComputer.virtualTarget;
			else if (!aim || restIfImpossible)
				rotator.target = restTarget;
		}
	}
}