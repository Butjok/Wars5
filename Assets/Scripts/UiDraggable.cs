using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UiDraggable : MonoBehaviour,IDragHandler {
    public RectTransform rectTransform;
    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue(rectTransform);
    }
    public void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta;
    }
}