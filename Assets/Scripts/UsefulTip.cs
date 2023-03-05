using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using static Gettext;
using Random = UnityEngine.Random;

public class UsefulTip : MonoBehaviour {

    public static string FixedText => _p("UsefulTip", "Please wait. It seems to take a while...");

    public static IEnumerable<string> Tips => new[] {
        _p("UsefulTip", "In the beginning of a turn try to use all your artillery units first to cause as much damage as possible before direct attack units."),
        _p("UsefulTip", "Try to place artillery units near roads, shores etc."),
        _p("UsefulTip", "Missile silos can damage enemy units and destroy bridges."),
    };

    public float delay = 1;
    public float displayTime = 5;
    public TMP_Text text;
    public int typingStep = 2;
    public float fixedTextDelay = 1;
    public bool showFixedTextAfterAllTipsAreExhausted = true;

    private void Start() {
        text = GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(text);
        StartCoroutine(Loop());
    }

    public IEnumerator ReplaceText(string value) {
        var counter = 0;
        while (text.text.Length > 0) {
            text.text = text.text[..^1];
            if (++counter % typingStep == 0)
                yield return null;
        }
        counter = 0;
        foreach (var c in value) {
            text.text += c;
            if (++counter % typingStep == 0)
                yield return null;
        }
    }

    public IEnumerator Loop() {
        var tips = Tips.OrderBy(_ => Random.value).ToList();
        text.enabled = false;
        yield return new WaitForSeconds(delay);
        text.enabled = true;
        text.text = "";
        foreach (var tip in tips) {
            yield return ReplaceText(tip);
            yield return new WaitForSeconds(displayTime);
            yield return ReplaceText("");
        }
        if (showFixedTextAfterAllTipsAreExhausted) {
            yield return new WaitForSeconds(fixedTextDelay);
            yield return ReplaceText(FixedText);
        }
        else
            text.enabled = false;
    }
}