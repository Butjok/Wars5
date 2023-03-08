using System.Collections;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PowerMeterStripe : MonoBehaviour {

    public float speed = 2.5f;
    public Material material;

    private void OnEnable() {
        var image = GetComponent<Image>();
        Assert.IsTrue(image);
        material = image.material;
        Assert.IsTrue(material);
    }

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