using System;
using System.Collections;
using System.Globalization;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.UI;

public class CaptureScreen : MonoBehaviour {

    public Image fillMeter;
    public Easing.Name easing = Easing.Name.Linear;
    public float speed = 1;
    public UiLabel label;

    public bool Visible {
        set => gameObject.SetActive(value);
    }
    public Color Color {
        set => fillMeter.color = value;
    }

    [Command]
    public Func<bool> SetProgress(float targetValue, int maxCp) {
        var completed = false;
        StopAllCoroutines();
        StartCoroutine(ProgressAnimation(targetValue, speed, easing, maxCp, () => completed=true));
        return () => completed;
    }

    public IEnumerator ProgressAnimation(float targetValue, float speed, Easing.Name easing, int maxCp, Action onComplete) {
        var startValue = fillMeter.fillAmount;
        var startTime = Time.time;
        var duration = Mathf.Abs(targetValue - startValue) / speed;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.Dynamic(easing, t);
            fillMeter.fillAmount = Mathf.Lerp(startValue, targetValue, t);
            if (label)
                label.text.text = Mathf.RoundToInt(fillMeter.fillAmount * maxCp).ToString();
            yield return null;
        }
        fillMeter.fillAmount = targetValue;
        if (label)
            label.text.text = Mathf.RoundToInt(fillMeter.fillAmount * maxCp).ToString();
        onComplete?.Invoke();
    }
}