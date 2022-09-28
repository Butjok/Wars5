using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[SelectionBase]
[RequireComponent(typeof(BallisticComputer))]
public class Turret : MonoBehaviour {

	public bool aim;
	[Space] public Rotator[] rotators = Array.Empty<Rotator>();
	public BallisticComputer ballisticComputer;
	public bool restIfImpossible = true;
	public Transform restTarget;
	public float shotForce = 500;
	public Projectile projectilePrefab;
	public BodyTorque bodyTorque;

	public void Start() {
		ballisticComputer = GetComponent<BallisticComputer>();
		Assert.IsTrue(ballisticComputer);
	}

	public void Update() {
		var possible = ballisticComputer.curve != null;
		foreach (var rotator in rotators) {
			if (aim && possible)
				rotator.target = ballisticComputer.virtualTarget;
			else if (!aim || restIfImpossible)
				rotator.target = restTarget;
		}
	}

	public void Fire(List<ImpactPoint> impactPoints) {

		Assert.IsTrue(bodyTorque);
		Assert.IsTrue(ballisticComputer);
		Assert.IsTrue(ballisticComputer.barrel);
		Assert.IsTrue(projectilePrefab);

		bodyTorque.AddWorldForceTorque(ballisticComputer.barrel.position, -ballisticComputer.barrel.forward * shotForce);

		var projectile = Instantiate(projectilePrefab, ballisticComputer.barrel.position, ballisticComputer.barrel.rotation);
		projectile.impactPoints = impactPoints;

		Assert.IsTrue(ballisticComputer.curve != null);
		projectile.ballisticCurve = (BallisticCurve)ballisticComputer.curve;
	}
}