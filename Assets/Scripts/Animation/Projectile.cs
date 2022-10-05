using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public List<ImpactPoint> impactPoints = new();
	public BallisticCurve ballisticCurve;
	public float time;
	public const float adjacentPositionDeltaTime = .1f;
	public ParticleSystem impactPrefab;
	public float impactForce = 500;
	public bool destroy;

	public void Update() {

		if (destroy)
			Destroy(gameObject);

		else if (ballisticCurve.totalTime is {} totalTime && time >= totalTime) {

			destroy = true;
			transform.position = ballisticCurve.Sample(totalTime);
			
			foreach (var impactPoint in impactPoints) {

				if (!impactPoint)
					continue;
				
				if (impactPrefab) {
					var impact = Instantiate(impactPrefab, impactPoint.transform.position, impactPoint.transform.rotation);
					impact.Play();
				}

				if (impactPoint.unitView && impactPoint.unitView.bodyTorque)
					impactPoint.unitView.bodyTorque.AddWorldForceTorque(impactPoint.transform.position, -impactPoint.transform.forward * impactForce);
			}
		}

		else {
			transform.position = ballisticCurve.Sample(time);
			var adjacent = ballisticCurve.Sample(time + adjacentPositionDeltaTime);
			transform.rotation = Quaternion.LookRotation(adjacent - transform.position, Vector3.up);
			time += Time.deltaTime;
		}
	}
}