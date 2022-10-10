using UnityEngine;
using UnityEngine.Assertions;

public class Projectile : MonoBehaviour {

    public Turret source;
    public BattleView.TargetingSetup targetingSetup;
    public BallisticCurve ballisticCurve;
    public float time;
    public const float adjacentPositionDeltaTime = .1f;
    public ParticleSystem impactPrefab;
    public float impactForce = 500;
    public bool destroy;
    public bool isLastProjectile;

    public void Update() {
        if (destroy)
            Destroy(gameObject);

        else if (ballisticCurve.totalTime is { } totalTime && time >= totalTime) {

            destroy = true;
            transform.position = ballisticCurve.Sample(totalTime);

            if (targetingSetup.targets != null && targetingSetup.targets.TryGetValue(source.unitView, out var targets)) {

                foreach (var target in targets) {

                    Assert.AreNotEqual(0, target.impactPoints.Length);
                    var impactPoint = target.impactPoints.Random();
                    Instantiate(impactPrefab, impactPoint.transform.position, impactPoint.transform.rotation).Play();

                    if (isLastProjectile)
                        targetingSetup.remainingAttackersCount[target]--;

                    if (isLastProjectile && targetingSetup.remainingAttackersCount[target] == 0)
                        target.Die(this, impactPoint);
                    else
                        target.TakeDamage(this, impactPoint);
                }
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