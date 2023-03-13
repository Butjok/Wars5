using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using static Gettext;

public class DayText : MonoBehaviour {

    private static DayText instance;
    public static bool TryFind(out DayText dayText) {
        if (!instance)
            instance = FindObjectOfType<DayText>();
        dayText = instance;
        return instance;
    }

    public TMP_Text text;
    public string format = "DAY {0}";
    public float duration = 2;

    private void Reset() {
        text = GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(text);
    }
    private void OnEnable() {
        text.enabled = false;
        IsPlaying = false;
        StopAllCoroutines();
    }
    public void PlayAnimation(int day, Color color) {
        StopAllCoroutines();
        IsPlaying = false;
        StartCoroutine(Animation(day, color));
    }
    public bool IsPlaying { get; private set; }
    private IEnumerator Animation(int day, Color color) {
        IsPlaying = true;
        text.enabled = true;
        text.text = string.Format(_(format), day + 1);
        text.color = color;
        var startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + duration)
            yield return null;
        IsPlaying = false;
        text.enabled = false;
    }
}