using System;
using System.Collections;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static Gettext;

[RequireComponent(typeof(Button))]
public class DayButton : MonoBehaviour {

    private static DayButton instance;
    public static DayButton TryGetInstance() {
        if (!instance)
            instance = FindObjectOfType<DayButton>();
        return instance;
    }

    public bool shouldEndTurn;
    public void OnClick() {
        shouldEndTurn = true;
    }

    public RectTransform carousel;
    public string gettextText = "Day {0}";
    public float duration = 2.5f;
    public Button button;
    public TMP_Text text;
    public Easing.Name easingName;
    public Sun debugSun;
    public int nextDay = 1;
    public int direction = -1;

    public int Day {
        set => text.text = string.Format(_(gettextText), value);
    }

    public void CycleNextDay() {
        PlayAnimation(nextDay++);
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
        if(debugSun)
            debugSun.PlayDayChange();
    }

    public IEnumerator Animation(int nextDay) {
        Assert.IsTrue(carousel);
        var oldInteractable = button.interactable;
        button.interactable = false;
        var changedDay = false;
        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.Dynamic(easingName,t);
            carousel.rotation = Quaternion.Euler(0,0,direction*360*t);
            if (text && !changedDay && t >= .5f) {
                changedDay = true;
                Day = nextDay;
            }
            yield return null;
        }
        button.interactable = oldInteractable;
        carousel.rotation=Quaternion.identity;
    }
}