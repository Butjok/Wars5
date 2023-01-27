using System;
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
	public UnitView unitView;

	private bool initialized;
	private void EnsureInitialized() {
		if(initialized)
			return;
		initialized = true;
		
		unitView = GetComponentInChildren<UnitView>();
		//Assert.IsTrue(unitView);
		
		computer = GetComponent<BallisticComputer>();
		Assert.IsTrue(computer);
	}
	
	private void Awake() {
		EnsureInitialized();
	}

	private void Update() {
		var possible = computer.curve != null;
		foreach (var rotator in rotators) {
			if (aim && possible)
				rotator.target = computer.virtualTarget;
			else if (!aim || restIfImpossible)
				rotator.target = restTarget;
		}
	}

	public void Shoot(BattleView.TargetingSetup targetingSetup, bool isLastProjectile) {

		Assert.IsTrue(bodyTorque);
		Assert.IsTrue(computer);
		var barrel = computer.barrel;
		Assert.IsTrue(barrel);
		Assert.IsTrue(projectilePrefab);
		
		EnsureInitialized();

		bodyTorque.AddWorldForceTorque(barrel.position, -barrel.forward * shotForce);

		var projectile = Instantiate(projectilePrefab, barrel.position, barrel.rotation);
		projectile.source = this;
		projectile.targetingSetup = targetingSetup;
		projectile.isLastProjectile = isLastProjectile;
		
		projectile.ballisticCurve = BallisticCurve.From(barrel.position,barrel.forward,computer.velocity,computer.gravity);
		if (computer.curve is { } curve)
			projectile.ballisticCurve.totalTime = curve.totalTime;
	}
}