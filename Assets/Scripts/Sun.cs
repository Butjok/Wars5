using System;
using System.Collections;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class Sun : MonoBehaviour {

    public float dayChangeDuration = 2.5f;
    public int axis = 0;
    public Vector3 startAngles;

    public Color nightColorFilterColor = Color.blue;
    public AnimationCurve nightIntensity = new(
        new Keyframe(0, 0),
        new Keyframe(90, 0),
        new Keyframe(90 + 45, 1),
        new Keyframe(270 - 45, 1),
        new Keyframe(270, 0),
        new Keyframe(360, 0));

    public Color duskColor = Color.yellow;
    public AnimationCurve duskIntensity = new(
        new Keyframe(0, 0),
        new Keyframe(90 - 45, 0),
        new Keyframe(90, 1),
        new Keyframe(270, 1),
        new Keyframe(270 + 45, 0),
        new Keyframe(360, 0));

    public Light light;
    public float timeScale = 5;

    [Command]
    public Func<bool> PlayDayChange() {
        StopAllCoroutines();
        var completed = false;
        StartCoroutine(Animation(() => completed = true));
        return () => completed;
    }

    private void OnEnable() {
        startAngles = transform.localRotation.eulerAngles;
    }

    public Easing.Name easing = Easing.Name.Linear;

    public Texture2D[] cookies = { };
    public Transform camera;
    public Vector2 cookieHeightLimits = new(5, 25);

    public void OnGUI() {
        if (camera && cookies.Length > 0 && light) {
            var height = camera.position.y;
            var t = Mathf.Clamp01((height - cookieHeightLimits[0]) / (cookieHeightLimits[1] - cookieHeightLimits[0]));
            var targetCookie = cookies[Mathf.RoundToInt(t * (cookies.Length - 1))];
            if (light.cookie != targetCookie)
                light.cookie = targetCookie;
            //GUILayout.Label($"\n\nt = {t}");
        }
    }

    [Command]
    public float CameraHeight => camera.position.y;

    public IEnumerator Animation(Action onComplete = null) {
        Assert.IsTrue(axis is 0 or 1 or 2);

        var startTime = Time.unscaledTime;
        var angles = startAngles;
        var a = Random.Range(0, 360f);
        var axis2 = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
        var startRotation = transform.rotation;
        var from = angles[axis];
        var to = from + 360;
        var lightIntensityAmplitude = light ? light.intensity : 0;
        var startAmbientIntensity = RenderSettings.ambientIntensity;
        var startLightIntensity = light ? light.intensity : 0;
        Time.timeScale = timeScale;
        while (Time.unscaledTime < startTime + dayChangeDuration) {
            var t = (Time.unscaledTime - startTime) / dayChangeDuration;
            t = Easing.Dynamic(easing, t);
            var angle = Mathf.Lerp(from, to, t);
            angles[axis] = angle;
            transform.rotation = startRotation * Quaternion.AngleAxis(t * 360, axis2);
            var nightIntensity = this.nightIntensity.Evaluate(360 * t);
            PostProcessing.ColorFilter = Color.Lerp(Color.white, nightColorFilterColor, nightIntensity);
            if (light) {
                light.color = Color.Lerp(Color.white, duskColor, duskIntensity.Evaluate(360 * t));
                light.intensity = lightIntensityAmplitude * (1 - nightIntensity);
                if (Vector3.Dot(light.transform.forward, Vector3.up) > 0)
                    light.intensity = 0;
                //RenderSettings.ambientIntensity = startAmbientIntensity * (1 - nightIntensity);
            }

            yield return null;
        }

        Time.timeScale = 1;
        angles[axis] = to;
        transform.rotation = startRotation;
        PostProcessing.ColorFilter = Color.white;
        //RenderSettings.ambientIntensity = startAmbientIntensity;
        if (light) {
            light.color = Color.white;
            light.intensity = lightIntensityAmplitude;
        }

        onComplete?.Invoke();
    }
}