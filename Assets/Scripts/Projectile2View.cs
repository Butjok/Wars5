using System;
using System.Collections;
using Drawing;
using UnityEngine;

public class Projectile2View : MonoBehaviour {

    public Projectile2 projectile;
    public AudioSource impactAudioSource;
    public AudioClip impactAudioClip;
    public ParticleSystem impactParticleSystem;
    public float startTime;

    public void PlayImpact(Transform hitPoint) {
        if (impactAudioSource && impactAudioClip)
            impactAudioSource.PlayOneShot(impactAudioClip);
        if (impactParticleSystem) {
            impactParticleSystem.transform.SetPositionAndRotation(
                hitPoint.position, hitPoint.rotation);
            impactParticleSystem.Play();
        }
    }

    private void Start() {
        StartCoroutine(Animation());
    }
    private IEnumerator Animation() {
        yield return new WaitForSeconds(.5f);
        projectile.HitTargets();
    }

    private void Update() {
        if (!projectile.hitPoint)
            return;
        using (Draw.ingame.WithLineWidth(2)) {
            Draw.ingame.Line(transform.position, projectile.hitPoint.position, Color.red);
            foreach (var target in projectile.Targets)
                if (target)
                    Draw.ingame.Line(projectile.hitPoint.position, target.transform.position, Color.yellow);
        }
    }
}