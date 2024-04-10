using System;
using Cinemachine;
using UnityEngine;

public class MainMenuUnitLoop : MonoBehaviour {

    public float length = 1;
    public Transform[] actors = { };
    public float speed = 1;
    public bool rotate = true;
    public CinemachineVirtualCamera virtualCamera;
    public float noiseAmplitude = 1;

    public Vector3 LineStart => transform.position;
    public Vector3 LineEnd => transform.position + LineDirection * length;
    public Vector3 LineDirection => transform.forward;

    public static Vector3 GetClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
        var lineDirection = lineEnd - lineStart;
        var lineMagnitude = lineDirection.magnitude;
        lineDirection.Normalize();

        var closestPointDistance = Vector3.Dot((point - lineStart), lineDirection);
        closestPointDistance = Mathf.Clamp(closestPointDistance, 0f, lineMagnitude);

        return lineStart + lineDirection * closestPointDistance;
    }

    public CinemachineBasicMultiChannelPerlin noise;
    public float startNoiseAmplitude;
    public float startNoiseFrequency;
    public void Start() {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        startNoiseAmplitude = noise.m_AmplitudeGain;
        startNoiseFrequency = noise.m_FrequencyGain;
    }
    
    public float lastFallOff;

    public void Update() {

        noise.m_AmplitudeGain = startNoiseAmplitude;
        noise.m_FrequencyGain = 0;
        
        foreach (var actor in actors) {
            var closestPoint = GetClosestPointOnLineSegment(LineStart, LineEnd, actor.position);
            var distance = (closestPoint - LineStart).magnitude;
            var nextDistance = distance + speed * Time.deltaTime;
            var oldPosition = actor.position;
            var nextPosition = LineStart + LineDirection * nextDistance;
            if (rotate && nextPosition != oldPosition)
                actor.rotation = Quaternion.LookRotation(nextPosition - oldPosition, transform.up);
            var wrappedDistance = nextDistance % length;
            actor.position = LineStart + LineDirection * wrappedDistance;
            
            var distanceToCamera = Vector3.Distance(actor.position, virtualCamera.transform.position);
            var falloff = 1 / (distanceToCamera * distanceToCamera);
            noise.m_AmplitudeGain = Mathf.Max( noise.m_AmplitudeGain, noiseAmplitude * falloff);
        }

        noise.m_FrequencyGain = noise.m_AmplitudeGain * startNoiseFrequency;
    }

    /*private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($" noise amplitude: {noise.m_AmplitudeGain}");
    }*/

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(LineStart, LineEnd);
    }
}