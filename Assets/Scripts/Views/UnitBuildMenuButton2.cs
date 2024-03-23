using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitBuildMenuButton2 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler {

    public Image image;
    public Color highlightColor = Color.white;
    public Vector2 highlightedSizeMultiplier = new(1.25f, 1.25f);
    public Vector2 highlightedOffset = new();
    public float duration = .5f;
    public Easing.Name easing = Easing.Name.Linear;
    public UiLabel label;
    public float labelDuration = .25f;
    public Easing.Name labelEasing = Easing.Name.Linear;

    public Color startColor = Color.white;
    public Vector2 startSize = Vector2.one;
    public Vector2 startPosition = Vector2.zero;

    public UnitBuildMenu2 buildMenu;

    private void Start() {
        if (!image) {
            image = GetComponentInChildren<Image>();
            Assert.IsTrue(image);
        }
        if (!label)
            label = GetComponentInChildren<UiLabel>();
        startColor = image.color;
        startSize = image.rectTransform.sizeDelta;
        startPosition = image.rectTransform.anchoredPosition;
        //image.sprite = Resources.Load<Sprite>("UnitThumbnails/" + name);
    }

    private bool highlighted;
    public bool Highlighted {
        set {
            if (value == highlighted)
                return;

            highlighted = value;
            image.color = value ? highlightColor : startColor;
            var targetSize = startSize * (value ? highlightedSizeMultiplier : Vector2.one);
            var targetPosition = startPosition + (value ? highlightedOffset : Vector2.zero);
            var targetColor = value ? highlightColor : startColor;
            var targetLabelAlpha = value ? 1 : 0;
            StopAllCoroutines();
            StartCoroutine(Animation(targetSize, targetPosition, targetColor, targetLabelAlpha, duration));
            if(label)
                label.Fade(highlighted ? 1 : 0, labelDuration, labelEasing);
            
            
        }
        get => highlighted;
    }

    private bool isAvailable = true;
    public bool IsAvailable {
        get => isAvailable;
        set {
            isAvailable = value;
            if (!value)
                image.color = Color.grey;
        }
    }

    public UnitType unitType;

    public void UpdateColor() {
        
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Highlighted = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
        Highlighted = false;
    }
    public void OnPointerMove(PointerEventData eventData) {
        Highlighted = true;
    }

    public IEnumerator Animation(Vector2 targetSize, Vector2 targetOffset, Color targetColor, float targetLabelAlpha, float duration) {

        var startSize = image.rectTransform.sizeDelta;
        var startOffset = image.rectTransform.anchoredPosition;
        var startColor = image.color;
        var startTime = Time.time;

        while (Time.time - startTime < duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.Dynamic(easing, t);
            image.rectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            image.rectTransform.anchoredPosition = Vector2.Lerp(startOffset, targetOffset, t);
            image.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        image.rectTransform.sizeDelta = targetSize;
        image.rectTransform.anchoredPosition = targetOffset;
        image.color = targetColor;
    }
}