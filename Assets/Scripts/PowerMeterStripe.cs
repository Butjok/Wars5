using System.Collections;
using Butjok.CommandLine;
using UnityEngine;

public class PowerMeterStripe : MonoBehaviour {

    public float speed = 2.5f;
    public Material material;

    [Command]
    public void SetProgress(float value, bool animate = true) {
        
        if (value == GetUniformValue())
            return;

        if (animate) {
            StopAllCoroutines();
            StartCoroutine(Animation(value));
        }
        else
            SetUniformValue(value);
    }

    public IEnumerator Animation(float to) {
        var from = GetUniformValue();
        var duration = Mathf.Abs(to - from) / speed;
        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            SetUniformValue(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetUniformValue(to);
    }

    private float GetUniformValue() {
        return material.GetFloat("_Progress");
    }
    private void SetUniformValue(float value) {
        material.SetFloat("_Progress", value);
    }
}