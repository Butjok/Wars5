using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

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
    
    public TMP_Text text;
    public string gettextText = "Day {0}";
    public float duration = 2.5f;
    
    public void PlayAnimation(int nextDay) {
        StopAllCoroutines();
        StartCoroutine(Animation(nextDay));
    }
    public IEnumerator Animation(int nextDay) {
        var changedDay = false;
        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.InOutQuad(t);
            if (text && !changedDay && t >= .5f) {
                changedDay = true;
                text.text = "";
            }
            yield return null;
        }
    }
}