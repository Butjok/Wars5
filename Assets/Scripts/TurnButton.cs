using System;
using System.Collections;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static Gettext;

[RequireComponent(typeof(Button))]
public class TurnButton : MonoBehaviour {

    private static TurnButton instance;
    public static TurnButton TryGet() {
        if (!instance)
            instance = FindObjectOfType<TurnButton>(true);
        return instance;
    }
    public static bool TryGet(out TurnButton turnButton) {
        turnButton = TryGet();
        return turnButton;
    }

    public bool Visible {
        set => gameObject.SetActive(value);
    }

    public RectTransform carousel;
    public string gettextText = "Day {0}";
    public float duration = 2.5f;
    public Button button;
    public TMP_Text text;
    public Easing.Name easingName;
    public Sun debugSun;
    public int direction = -1;
    public bool debugMakeInteractable = false;

    public Color highlightTint = Color.grey;
    public Color highlightEmissive = Color.yellow;

    public Color Color {
        set {
            var colors = button.colors;
            colors.normalColor = colors.selectedColor = value;
            colors.pressedColor = colors.highlightedColor = value * highlightTint + highlightEmissive;
            button.colors = colors;
        }
    }
    private int? day;
    public int? Day {
        get => day;
        set {
            day = value;
            text.text = value is { } actualValue ? string.Format(_(gettextText), actualValue + 1) : "";
        }
    }

    private void Reset() {
        button = GetComponent<Button>();
        Assert.IsTrue(button);
        text = button.GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(text);
    }

    private void OnEnable() {
        text.text = "";
            Interactable = debugMakeInteractable;
    }

    [Command]
    public Func<bool> PlayAnimation(int nextDay) {
        StopAllCoroutines();
        var completed = false;
        StartCoroutine(Animation(nextDay, () => completed=true));
        if (debugSun)
            debugSun.PlayDayChange();
        return () => completed;
    }

    public bool Interactable {
        set => button.interactable = value;
    }

    public IEnumerator Animation(int nextDay, Action onComplete=null) {

        Assert.IsTrue(carousel);

        var changedDay = false;
        var startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + duration) {
            var t = (Time.unscaledTime - startTime) / duration;
            t = Easing.Dynamic(easingName, t);
            carousel.rotation = Quaternion.Euler(0, 0, direction * 360 * t);
            if (text && !changedDay && t >= .5f) {
                changedDay = true;
                Day = nextDay;
            }
            Interactable = false;
            yield return null;
        }
        Interactable = debugMakeInteractable;
        carousel.rotation = Quaternion.identity;
        
        onComplete?.Invoke();
    }
}