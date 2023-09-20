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
    public UiCircle circle;
    public Color defaultColor = Color.white;
    public float pauseBefore = .5f;
    public float pauseOwnerChange = .25f;
    public float pauseAfter = .5f;

    public bool Visible {
        set => gameObject.SetActive(value);
    }
    public Color Color {
        set => fillMeter.color = value;
    }

    private int Label {
        set {
            if(label)
                label.text.text = value.ToString();
        }
    }

    public void SetCp(int cp, int maxCp) {
        fillMeter.fillAmount = (float)cp / maxCp;
        Label = cp;
    }

    [Command]
    public Func<bool> AnimateCp(int cp, int maxCp) {
        var completed = false;
        StopAllCoroutines();
        StartCoroutine(ProgressAnimation((float)cp/maxCp, speed, easing, maxCp, () => completed=true));
        return () => completed;
    }

    public IEnumerator ProgressAnimation(float targetFillAmount, float speed, Easing.Name easing, int maxCp, Action onComplete) {
        var startValue = fillMeter.fillAmount;
        var startTime = Time.time;
        var duration = Mathf.Abs(targetFillAmount - startValue) / speed;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.Dynamic(easing, t);
            fillMeter.fillAmount = Mathf.Lerp(startValue, targetFillAmount, t);
            Label = Mathf.RoundToInt(fillMeter.fillAmount * maxCp);
            yield return null;
        }
        fillMeter.fillAmount = targetFillAmount;
        Label =  Mathf.RoundToInt(fillMeter.fillAmount * maxCp);
        onComplete?.Invoke();
    }
}