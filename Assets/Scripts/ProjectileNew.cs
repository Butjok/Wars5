using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Cinemachine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[RequireComponent(typeof(AudioSource))]
public class ProjectileNew : MonoBehaviour {

    public Transform target;
    public Transform hitPoint;
    public float incomingElevation;
    public float duration = 3;
    public float speed = 1;
    public List<Renderer> renderers = new();
    private bool ready = true;
    public float spinAngle;
    public AudioSource audioSource;
    public ParticleSystem hitParticleSystem;
    public float delay = 5;
    public float shakeAmplitude = 1;
    public float shakeFrequency = 1;
    public float amplitudeTimeFalloffPower = 5;
    public float frequencyTimeFalloffPower = 1;

    [Command]
    public void Play() {
        Play(target, hitPoint);
    }
    public void Play(Transform target, Transform hitPoint) {
        if (ready) {
            ready = false;
            StartCoroutine(Animation(target, hitPoint, () => ready = true));
        }
    }
    private void Reset() {
        renderers = GetComponentsInChildren<Renderer>().ToList();
        audioSource = GetComponent<AudioSource>();
    }

    private Dictionary<CinemachineVirtualCamera, IEnumerator> shakes = new();

    [Command]
    public void ShakeCameras() {
        foreach (var virtualCamera in FindObjectsOfType<CinemachineVirtualCamera>())
            ShakeCamera(virtualCamera);
    }
    
    public void ShakeCamera(CinemachineVirtualCamera virtualCamera) {
        var noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (!noise)
            return;
        if (shakes.TryGetValue(virtualCamera, out var coroutine))
            StopCoroutine(coroutine);
        coroutine = shakes[virtualCamera] = ShakeAnimation(noise);
        StartCoroutine(coroutine);
    }
    private IEnumerator ShakeAnimation(CinemachineBasicMultiChannelPerlin noise) {
        var startTime = Time.time;
        while (true) {
            var time = Time.time - startTime;
            var amplitude = shakeAmplitude * Mathf.Exp(-time * amplitudeTimeFalloffPower);
            var frequency = shakeFrequency * Mathf.Exp(-time * frequencyTimeFalloffPower);
            noise.m_FrequencyGain = frequency;
            noise.m_AmplitudeGain = amplitude;
            yield return null;
        }
    }

    private IEnumerator Animation(Transform target, Transform hitPoint, Action onComplete = null,
        AudioClip incomingAudioClip = null) {

        var targetProjectedForward = new Vector3(target.forward.x, 0, target.forward.z);
        Assert.AreNotEqual(Vector3.zero, targetProjectedForward);

        var startPosition = transform.position;
        var startRotation = transform.rotation;
        var startForward = transform.forward;
        var endPosition = hitPoint.position;
        var endForward = -(targetProjectedForward.normalized * Mathf.Cos(incomingElevation) + Vector3.up * Mathf.Sin(incomingElevation)).normalized;

        var incomingSoundPlayed = false;

        foreach (var renderer in renderers)
            renderer.enabled = true;

        var startTime = Time.time;
        while (Time.time < startTime + duration) {

            var timeFromStart = Time.time - startTime;
            var timeUntilEnd = duration - timeFromStart;

            Vector3 from, to;
            if (Time.time < startTime + duration / 2) {
                from = startPosition;
                to = startPosition + startForward * duration / 2 * speed;
            }
            else {
                from = endPosition - endForward * duration / 2 * speed;
                to = endPosition;
            }

            var t = timeFromStart % (duration / 2) / (duration / 2);
            transform.position = Vector3.Lerp(from, to, t);
            transform.rotation = Quaternion.LookRotation(to - from, Vector3.up) * Quaternion.Euler(0, 0, spinAngle);

            if (audioSource && incomingAudioClip && !incomingSoundPlayed && timeUntilEnd <= incomingAudioClip.length) {
                incomingSoundPlayed = true;
                audioSource.PlayOneShot(incomingAudioClip);
            }

            yield return null;
        }

        foreach (var renderer in renderers)
            renderer.enabled = false;

        if (hitParticleSystem) {
            hitParticleSystem.transform.SetPositionAndRotation(hitPoint.position, hitPoint.rotation);
            hitParticleSystem.Play();
        }

        ShakeCameras();

        transform.position = startPosition;
        transform.rotation = startRotation;

        onComplete?.Invoke();

        // Destroy(gameObject, delay);
    }
}