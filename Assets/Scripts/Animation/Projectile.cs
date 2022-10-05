using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public List<ImpactPoint> impactPoints = new();
	public BallisticCurve ballisticCurve;
	public float time;
	public const float adjacentPositionDeltaTime = .1f;
	public ParticleSystem impactPrefab;
	public float impactForce = 500;

	public void Update() {

		if (time >= ballisticCurve.totalTime) {

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

			Destroy(gameObject);
			return;
		}

		transform.position = ballisticCurve.Sample(time);
		var adjacent = ballisticCurve.Sample(time + adjacentPositionDeltaTime);
		transform.rotation = Quaternion.LookRotation(adjacent - transform.position, Vector3.up);
		time += Time.deltaTime;
	}
}