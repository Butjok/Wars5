using System;
using System.Collections;
using Butjok.CommandLine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Canvas))]
public class MarketingUiDrawing : MonoBehaviour {

    public class TextBox {

        public static Func<float, float> defaultEasing = Easing.InOutExpo;
        
        public RectTransform rectTransform;
        public TMP_Text text;
        public Image background;

        public Vector2 Alpha {
            get => new Vector2(text.color.a, background.color.a);
            set {
                var textColor = text.color;
                textColor.a = value.x;
                text.color = textColor;
                
                var backgroundColor = background.color;
                backgroundColor.a = value.y;
                background.color = backgroundColor;
            }
        }

        public IEnumerator Fade(Vector2 targetAlpha, float duration, Func<float, float> easing = null) {
            var oldAlpha = Alpha;
            var startTime = Time.time;
            while (Time.time < startTime + duration) {
                var t = (Time.time - startTime) / duration;
                Alpha = Vector2.Lerp(oldAlpha, targetAlpha, (easing ?? defaultEasing) (t));
                yield return null;
            }
            Alpha = targetAlpha;
        }

        public IEnumerator MoveTo(Vector2 targetPosition, float duration, Func<float, float> easing = null) {
            var oldPosition = rectTransform.anchoredPosition;
            var startTime = Time.time;
            while (Time.time < startTime + duration) {
                var t = (Time.time - startTime) / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(oldPosition, targetPosition, (easing ?? defaultEasing) (t));
                yield return null;
            }
            rectTransform.anchoredPosition = targetPosition;
        }
    }

    public static class Easing {
        public static float Linear(float t) => t;
        public static float InOutExpo(float x) {
            return x != 0
                ? Mathf.Approximately(1, x)
                    ? 1
                    : x < 0.5
                        ? Mathf.Pow(2, 20 * x - 10) / 2
                        : (2 - Mathf.Pow(2, -20 * x + 10)) / 2
                : 0;
        }
    }

    public Canvas canvas;
    public Sprite textBoxBackgroundSprite;
    public TMP_FontAsset font;

    private void Awake() {
        canvas = GetComponent<Canvas>();
        Assert.IsTrue(canvas);
    }

    private void Start() {
        PlayAnimation();
    }

    [Command]
    public void PlayAnimation() {
        StartCoroutine(Animation());
    }

    public IEnumerator Animation() {
        
        var fadeDuration = .5f;
        var moveDuration = .66f;
        var itemSize = new Vector2(100, 50);
        var stackSpacer = new Vector2(0,55);
        var stackBottom = -3 * stackSpacer;
        var delay = 1;
        var sideSpacer = new Vector2(105, 0);
        
        var box1 = CreateTextBox(Vector2.zero,itemSize, "1", Vector2.zero);
        yield return box1.Fade(Vector2.one, fadeDuration);
        yield return box1.MoveTo(stackBottom, moveDuration);
        
        var box_90 = CreateTextBox(Vector2.zero, itemSize, "-90", Vector2.zero);
        yield return box_90.Fade(Vector2.one, fadeDuration);
        yield return box_90.MoveTo(stackBottom + stackSpacer, moveDuration);
        
        var box90 = CreateTextBox(Vector2.zero, itemSize, "90", Vector2.zero);
        yield return box90.Fade(Vector2.one, fadeDuration);
        yield return box90.MoveTo(stackBottom + stackSpacer * 2, moveDuration);
        
        yield return box90.MoveTo(Vector2.zero, moveDuration);
        yield return box90.MoveTo(sideSpacer, moveDuration);
        
        yield return box_90.MoveTo(Vector2.zero, moveDuration);

        var randomRange = CreateTextBox(new Vector2(-52.5f, 0), new Vector2(750, 50), "RandomRange(           )", Vector2.zero);
        yield return randomRange.Fade(new Vector2(1, 0), fadeDuration);
        yield return new WaitForSeconds(delay);

        StartCoroutine(box90.Fade(Vector2.zero, fadeDuration));
        StartCoroutine(box_90.Fade(Vector2.zero, fadeDuration));
        StartCoroutine(randomRange.Fade(Vector2.zero, fadeDuration));
        
        var box42 = CreateTextBox(Vector2.zero, itemSize, "42", Vector2.zero);
        // StartCoroutine(RandomizeText(box42.text));
        yield return box42.Fade(Vector2.one, fadeDuration);
        yield return new WaitForSeconds(delay);
        
        yield return box42.MoveTo(stackBottom + stackSpacer, moveDuration);
        
        var box2 = CreateTextBox(Vector2.zero,itemSize, "2", Vector2.zero);
        yield return box2.Fade(Vector2.one, fadeDuration);
        yield return box2.MoveTo(stackBottom + 2*stackSpacer, moveDuration);
        
        yield return box2.MoveTo(Vector2.zero, moveDuration);
        yield return box2.MoveTo(2*sideSpacer, moveDuration);
        
        yield return box42.MoveTo(Vector2.zero, moveDuration);
        yield return box42.MoveTo(sideSpacer, moveDuration);
        
        yield return box1.MoveTo(Vector2.zero, moveDuration);
    }
    
    public IEnumerator RandomizeText(TMP_Text text) {
        while (true) {
            yield return null;
            // yield return null;
            text.text = Random.Range(-90, 90).ToString().PadLeft(3).PadRight(4).Replace(" ", "<color=#fff0>_</color>");
        }
    }

    public TextBox CreateTextBox(Vector2 position, Vector2 size, string text = "", Vector2? alpha = null, RectTransform parent = null) {
        var go = new GameObject($"TextBox: {text}");
        var rectTransform = go.AddComponent<RectTransform>();
        rectTransform.SetParent(parent ? parent : transform);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        var background = go.AddComponent<Image>();
        background.sprite = textBoxBackgroundSprite;
        background.pixelsPerUnitMultiplier = 2;
        background.type = Image.Type.Sliced;
        var textGo = new GameObject("Text");
        var textRectTransform = textGo.AddComponent<RectTransform>();
        textRectTransform.SetParent(rectTransform);
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.anchoredPosition = Vector2.zero;
        textRectTransform.sizeDelta = Vector2.zero;
        var tmpText = textGo.AddComponent<TextMeshProUGUI>();
        tmpText.font = font;
        tmpText.color = Color.black;
        tmpText.fontSize = 32;
        tmpText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmpText.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmpText.text = text;
        var textBox =  new TextBox {
            rectTransform = rectTransform,
            text = tmpText,
            background = background
        };
        if (alpha is { } values)
            textBox.Alpha = values;
        return textBox;
    }
}