using System;
using System.Collections;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class Sun : MonoBehaviour {

    public float dayChangeDuration = 2.5f;
    public int axis = 0;
    public Vector3 startAngles;

    public Color nightColorFilterColor = Color.blue;
    public AnimationCurve nightColorAnimationCurve = new(
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

    [Command]
    public void PlayDayChange() {
        StopAllCoroutines();
        StartCoroutine(Animation());
    }

    private void OnEnable() {
        startAngles = transform.rotation.eulerAngles;
    }

    public IEnumerator Animation() {

        Assert.IsTrue(axis is 0 or 1 or 2);

        var startTime = Time.time;
        var angles = startAngles;
        var from = angles[axis];
        var to = from + 360;
        while (Time.time < startTime + dayChangeDuration) {
            var t = (Time.time - startTime) / dayChangeDuration;
            var angle = Mathf.Lerp(from, to, Easing.InOutQuad(t));
            angles[axis] = angle;
            transform.rotation = Quaternion.Euler(angles);
            PostProcessing.ColorFilter = Color.Lerp(Color.white, nightColorFilterColor, nightColorAnimationCurve.Evaluate(360*t));
            if (light)
                light.color = Color.Lerp(Color.white, duskColor, duskIntensity.Evaluate(360 * t));
            yield return null;
        }
        angles[axis] = to;
        transform.rotation = Quaternion.Euler(startAngles);
        PostProcessing.ColorFilter = Color.white;
        if (light)
            light.color = Color.white;
    }
}