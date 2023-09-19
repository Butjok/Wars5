using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UiLabel : MonoBehaviour {

    public Image background;
    public TMP_Text text;
    public Color startBackgroundColor;
    public Color startTextColor;
    public float alpha = 1;
    public bool hideOnStart = false;

    public void Awake() {
        if (!background)
            background = GetComponent<Image>();
        if (!text)
            text = GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(background);
        Assert.IsTrue(text);
        startBackgroundColor = background.color;
        startTextColor = text.color;
    }

    private void Start() {
        if (hideOnStart)
            Alpha = 0;
    }

    public float Alpha {
        get => alpha;
        set {
            alpha = value;
            var backgroundColor = startBackgroundColor;
            backgroundColor.a = alpha;
            background.color = backgroundColor;
            var textColor = startTextColor;
            textColor.a = alpha;
            text.color = textColor;
        }
    }

    public void Fade(float targetAlpha, float duration, Easing.Name easing = Easing.Name.Linear) {
        StopAllCoroutines();
        StartCoroutine(FadeAnimation(targetAlpha, duration));
    }
    public IEnumerator FadeAnimation(float targetAlpha, float duration, Easing.Name easing = Easing.Name.Linear) {
        var startTime = Time.time;
        var startAlpha = Alpha;
        while (Time.time - startTime < duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.Dynamic(easing, t);
            Alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        Alpha = targetAlpha;
    }
}