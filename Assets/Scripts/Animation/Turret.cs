using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Turret : MonoBehaviour {

	public RotateTo[] rotators = Array.Empty<RotateTo>();
	public bool aim;
	public BallisticComputer ballisticComputer;
	public bool restIfImpossible = true;
	public bool possible;

	public void Start() {
		ballisticComputer = GetComponent<BallisticComputer>();
		Assert.IsTrue(ballisticComputer);
	}
	public void Update() {
		possible = ballisticComputer.curveOption != null;
		foreach (var rotator in rotators) {
			if (aim && possible)
				rotator.target = ballisticComputer.aimTarget;
			else if (!aim || restIfImpossible)
				rotator.target = null;
		}
	}
}