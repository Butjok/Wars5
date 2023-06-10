using UnityEngine;
using UnityEngine.EventSystems;

public class Dragger : MonoBehaviour, IDragHandler {
    public RectTransform target;
    public void OnDrag(PointerEventData eventData) {
        target.anchoredPosition += eventData.delta;
    }
}