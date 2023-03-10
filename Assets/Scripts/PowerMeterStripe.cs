using System;
using System.Collections;
using Butjok.CommandLine;
using UnityEngine;

public class PowerMeterStripe : MonoBehaviour {

    public float speed = 2.5f;
    public Material material;

    [Command]
    public void SetProgress(float value, bool animate = true, Action onComplete=null) {
        
        if (value == GetUniformValue())
            return;

        if (animate) {
            StopAllCoroutines();
            StartCoroutine(Animation(value, onComplete));
        }
        else
            SetUniformValue(value);
    }

    public IEnumerator Animation(float to, Action onComplete=null) {
        var from = GetUniformValue();
        var duration = Mathf.Abs(to - from) / speed;
        var startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + duration) {
            var t = (Time.unscaledTime - startTime) / duration;
            SetUniformValue(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetUniformValue(to);
        onComplete?.Invoke();
    }

    private float GetUniformValue() {
        return material.GetFloat("_Progress");
    }
    private void SetUniformValue(float value) {
        material.SetFloat("_Progress", value);
    }
}