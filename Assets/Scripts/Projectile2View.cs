using System;
using Drawing;
using UnityEngine;

public class Projectile2View : MonoBehaviour {

    public Projectile2 projectile;
    public AudioSource impactAudioSource;
    public AudioClip impactAudioClip;
    public ParticleSystem impactParticleSystem;

    public void PlayImpact(Transform hitPoint) {
        if (impactAudioSource && impactAudioClip)
            impactAudioSource.PlayOneShot(impactAudioClip);
        if (impactParticleSystem) {
            impactParticleSystem.transform.SetPositionAndRotation(
                hitPoint.position, hitPoint.rotation);
            impactParticleSystem.Play();
        }
    }

    private void Update() {

        if (projectile == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space)) {
            projectile.HitTargets();
            return;
        }

        using (Draw.ingame.WithLineWidth(2)) {
            Draw.ingame.Line(transform.position, projectile.hitPoint.position, Color.red);
            foreach (var target in projectile.Targets)
                Draw.ingame.Line(projectile.hitPoint.position, target.transform.position, Color.yellow);
        }
    }
}