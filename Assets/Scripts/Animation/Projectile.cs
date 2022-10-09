using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Projectile : MonoBehaviour {

    public List<ImpactPoint> impactPoints = new();
    public BallisticCurve ballisticCurve;
    public float time;
    public const float adjacentPositionDeltaTime = .1f;
    public ParticleSystem impactPrefab;
    public float impactForce = 500;
    public bool destroy;
    public bool isLastProjectile;
    public IReadOnlyCollection<UnitView> survivingTargets = Array.Empty<UnitView>();

    public void Update() {
        if (destroy)
            Destroy(gameObject);

        else if (ballisticCurve.totalTime is { } totalTime && time >= totalTime) {

            destroy = true;
            transform.position = ballisticCurve.Sample(totalTime);

            foreach (var impactPoint in impactPoints) {
                Instantiate(impactPrefab, impactPoint.transform.position, impactPoint.transform.rotation).Play();
                var unitView = impactPoint.unitView;

                // TODO: destroy target only if ALL attackers fires all of their projectiles and not only after the last projectile
                // TODO: of any of them hits the target
                if (isLastProjectile && !survivingTargets.Contains(unitView))
                    unitView.Die(this, impactPoint);
                else
                    unitView.TakeDamage(this, impactPoint);
                //impactPoint.unitView.bodyTorque.AddWorldForceTorque(impactPoint.transform.position, -impactPoint.transform.forward * impactForce);
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