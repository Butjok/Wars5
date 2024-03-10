using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Projectile3View : MonoBehaviour {

    public float force = 100;
    public float speed = 1;
    public float flightTime = 1;
    public List<UnitView> targets = new();
    public AudioClip shotSound;
    public AudioClip hitSound;
    public ParticleSystem hitParticleSystem;
    public bool shakeCameras = false;
    public UnitView unitView;
    public WeaponName? weaponName;

    public void Setup(Transform transform, IEnumerable<UnitView> targets) {
        this.transform.SetPositionAndRotation(transform.position, transform.rotation);
        this.targets.AddRange(targets);
        Assert.IsTrue(this.targets.Count > 0);
    }
    public void Start() {
        StartCoroutine(Animation());
    }
    public IEnumerator Animation() {
        if (targets.Count > 0) {
            
            var startTime = Time.time;
            while (Time.time < startTime + flightTime / 2) {
                transform.position += transform.forward * speed * Time.deltaTime;
                yield return null;
            }

            var projected = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            var down = projected - transform.forward;
            var incomingForward = (-targets[0].transform.forward + down).normalized;
            transform.position = targets[0].body.position - incomingForward * flightTime / 2 * speed;
            transform.rotation = Quaternion.LookRotation(incomingForward, Vector3.up);

            while (Time.time < startTime + flightTime) {
                transform.position += transform.forward * speed * Time.deltaTime;
                yield return null;
            }

            var first = true;
            foreach (var target in targets)
                if (target) {
                    target.TakeHit(this, first);
                    first = false;
                }

            /*if (weaponName == WeaponName.Cannon)
                Sounds.PlayOneShot(Sounds.explosion);
            else if (weaponName is WeaponName.Rifle or WeaponName.MachineGun)
                Sounds.PlayOneShot(Sounds.bulletRicochet);*/
        }

        Destroy(gameObject);
    }
}