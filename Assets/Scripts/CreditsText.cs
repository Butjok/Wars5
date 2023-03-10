using System.Collections;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class CreditsText : MonoBehaviour {

    public TMP_Text text;
    public float duration = 1;
    public Color defaultColor = Color.white;
    public Color positiveChangeColor = Color.green;
    public Color negativeChangeColor = Color.red;
    public Image coinImage;
    public Sprite[] coinSprites = { };

    private void OnEnable() {
        text = GetComponent<TMP_Text>();
        Assert.IsTrue(text);
    }
    private void OnValidate() {
        text = GetComponent<TMP_Text>();
        Assert.IsTrue(text);
        Amount = 0;
    }

    private int amount;
    public int Amount {
        get => amount;
        set {
            amount=value;
            text.text = value.ToStringWithThousandsSeparator();
            text.color = defaultColor;
        }
    }

    [Command]
    public void SetAmount(int value, bool animate = true) {
        if (Amount == value)
            return;
        if (animate) {
            StopAllCoroutines();
            StartCoroutine(Animation(value));
        }
        else
            Amount = value;
    }

    private IEnumerator Animation(int to) {
        var from = Amount;
        var startTime = Time.unscaledTime;
        var spriteIndex = 0;
        while (Time.unscaledTime < startTime + duration) {
            var t = (Time.unscaledTime - startTime) / duration;
            Amount = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            text.color = to > from ? positiveChangeColor : negativeChangeColor;
            if (coinImage && coinSprites.Length > 0)
                coinImage.sprite = coinSprites[++spriteIndex % coinSprites.Length];
            yield return null;
        }
        Amount = to;
        if (coinImage && coinSprites.Length > 0)
            while (true) {
                coinImage.sprite = coinSprites[++spriteIndex % coinSprites.Length];
                if (coinImage.sprite == coinSprites[0])
                    break;
                yield return null;
            }
    }
}