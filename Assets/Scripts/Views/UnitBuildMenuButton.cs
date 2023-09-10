using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UnitBuildMenuButton : MonoBehaviour, IPointerMoveHandler {

    public Vector2 normalSize = new(75, 75);
    public Vector2 hoverSize = new(100, 100);

    public Button button;
    public RectTransform rectTransform;

    public UnitBuildMenu2 buildMenu;
    public UnitType unitType;
    public Image image;

    public TMP_Text unavailableText;
    public float emphasizeDuration = .025f;
    public Ease emphasizeEase = Ease.Linear;

    public bool Available {
        set {
            button.interactable = value;
            // if (image)
                // image.color = new Color(value ? 0 : 1, 0, 0, 1);
        }
        get => button.interactable;
    }
    public Tween lastTween;
    public void Emphasize() {
        // lastTween?.Kill();
        lastTween = rectTransform.DOSizeDelta(hoverSize, emphasizeDuration).SetEase(emphasizeEase);
        if (unavailableText && !Available)
            unavailableText.enabled = true;
    }
    public void Unemphasize() {
        // lastTween?.Kill();
        lastTween = rectTransform.DOSizeDelta(normalSize, emphasizeDuration).SetEase(emphasizeEase);
        if (unavailableText)
            unavailableText.enabled = false;
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(() => buildMenu.TryBuild());
        image = GetComponent<Image>();
        unavailableText = GetComponentsInChildren<TMP_Text>().SingleOrDefault(c => c.name == "UNAVAILABLE");
        Assert.IsTrue(buildMenu);
        Assert.IsTrue(rectTransform);
        Assert.IsTrue(button);
        Assert.IsTrue(image);
    }

    public void OnPointerMove(PointerEventData eventData) {
        buildMenu.Select(this);
    }
}