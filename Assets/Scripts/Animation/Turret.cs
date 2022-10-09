using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

[SelectionBase]
[RequireComponent(typeof(BallisticComputer))]
public class Turret : MonoBehaviour {

	public bool aim;
	[Space] public Rotator[] rotators = Array.Empty<Rotator>();
	public BallisticComputer computer;
	public bool restIfImpossible = true;
	public Transform restTarget;
	public float shotForce = 500;
	public Projectile projectilePrefab;
	public BodyTorque bodyTorque;

	public void Awake() {
		computer = GetComponent<BallisticComputer>();
		Assert.IsTrue(computer);
	}

	public void Update() {
		var possible = computer.curve != null;
		foreach (var rotator in rotators) {
			if (aim && possible)
				rotator.target = computer.virtualTarget;
			else if (!aim || restIfImpossible)
				rotator.target = restTarget;
		}
	}

	public void Shoot(IReadOnlyList<UnitView> targets, IReadOnlyCollection<UnitView>survivingTargets, bool isLastProjectile) {

		Assert.IsTrue(bodyTorque);
		Assert.IsTrue(computer);
		var barrel = computer.barrel;
		Assert.IsTrue(barrel);
		Assert.IsTrue(projectilePrefab);

		bodyTorque.AddWorldForceTorque(barrel.position, -barrel.forward * shotForce);

		var projectile = Instantiate(projectilePrefab, barrel.position, barrel.rotation);
		var impactPoints = new List<ImpactPoint>();
		if (targets!=null)
			foreach (var target in targets) {
				Assert.AreNotEqual(0, target.impactPoints.Length);
				impactPoints.Add(target.impactPoints.Random());
			}
		projectile.impactPoints = impactPoints;
		projectile.survivingTargets = survivingTargets;
		projectile.isLastProjectile = isLastProjectile;
		projectile.ballisticCurve = BallisticCurve.From(barrel.position,barrel.forward,computer.velocity,computer.gravity);
		if (computer.curve is { } curve)
			projectile.ballisticCurve.totalTime = curve.totalTime;
	}
}