using System;
using System.Collections;
using System.Collections.Generic;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static Gettext;
using Random = UnityEngine.Random;

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

    public bool shouldEndTurn;
    public void OnClick() {
        shouldEndTurn = true;
    }

    public bool Visible {
        set {
            gameObject.SetActive(value);
        }
    }

    public RectTransform carousel;
    public string gettextText = "Day {0}";
    public float duration = 2.5f;
    public Button button;
    public TMP_Text text;
    public Easing.Name easingName;
    public Sun debugSun;
    public int direction = -1;

    public Color highlightTint = Color.grey;
    public Color highlightEmissive = Color.yellow;
    
    public Color Color {
        get => button.colors.normalColor;
        set {
            var colors = button.colors;
            colors.normalColor = colors.selectedColor = value;
            colors.pressedColor = colors.highlightedColor = value * highlightTint + highlightEmissive;
            button.colors = colors;
        }
    }
    public bool interactable = true;

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
    }

    [Command]
    public void PlayAnimation(int nextDay) {
        StopAllCoroutines();
        StartCoroutine(Animation(nextDay));
        if (debugSun)
            debugSun.PlayDayChange();
    }

    public IEnumerator Animation(int nextDay) {

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
            button.interactable = false;
            yield return null;
        }
        button.interactable = true;
        carousel.rotation = Quaternion.identity;
    }
}