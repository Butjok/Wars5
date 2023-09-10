using System.Collections;
using Butjok.CommandLine;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dissolve : MonoBehaviour {

    public Vector2 thresholdRange = new(30, -5);
    public MaterialPropertyBlock materialPropertyBlock;
    public Renderer renderer;
    public float3 rotation = new(0, 0, 0);
    public Vector2 noiseScaleRange = new(2, 10);
    public float duration = 1.5f;

    public Vector2 verticalSpeedRange = new(5, 10);
    public float verticalDrag = .1f;
    public float gravity = 2;
    public Vector2 angularSpeedRange = new(0, 45);
    public float movementDelay = 1;
    public ParticleSystem[] particleSystems = { };

    public CinemachineImpulseSource impulseSource;

    public void Awake() {
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    [Command]
    public void Animate() {
        StopAllCoroutines();
        StartCoroutine(MaterialAnimation());
        StartCoroutine(MovementAnimation());
    }

    public IEnumerator MovementAnimation() {
        yield return new WaitForSeconds(movementDelay);
        if (impulseSource)
            impulseSource.GenerateImpulse();
        foreach (var particleSystem in particleSystems)
            particleSystem.Play();
        var start = Time.time;
        var startRotation = transform.localRotation;
        var startPosition = transform.localPosition;
        var angularSpeed = new Vector3(
            Random.Range(angularSpeedRange[0], angularSpeedRange[1]),
            Random.Range(angularSpeedRange[0], angularSpeedRange[1]),
            Random.Range(angularSpeedRange[0], angularSpeedRange[1]));
        var verticalSpeed = Random.Range(verticalSpeedRange[0], verticalSpeedRange[1]);
        while (Time.time < start + duration) {
            var t = (Time.time - start) / duration;
            transform.localPosition += Vector3.up * verticalSpeed * Time.deltaTime;
            verticalSpeed -= Time.deltaTime * gravity;
            verticalSpeed -= verticalSpeed * verticalSpeed * verticalDrag * Mathf.Sign(verticalSpeed) * Time.deltaTime;
            transform.localRotation *= Quaternion.Euler(angularSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localRotation = startRotation;
        transform.localPosition = startPosition;
    }

    public IEnumerator MaterialAnimation() {
        var start = Time.time;
        while (Time.time < start + duration) {
            var t = (Time.time - start) / duration;
            Threshold = Mathf.Lerp(thresholdRange[0], thresholdRange[1], t);
            //NoiseScale = Mathf.Lerp(noiseScaleRange[0], noiseScaleRange[1], t);
            yield return null;
        }
        yield return new WaitForSeconds(5 - duration);
        Threshold = 100; //thresholdRange[0];
    }

    [Command]
    public void ResetAnimation() {
        Threshold = 100; //thresholdRange[0];
    }

    [Command]
    public float Threshold {
        set {
            materialPropertyBlock.SetFloat("_Threshold", value);
            if (renderer)
                renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
    [Command]
    public float NoiseScale {
        set {
            materialPropertyBlock.SetFloat("_NoiseScale", value);
            if (renderer)
                renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}