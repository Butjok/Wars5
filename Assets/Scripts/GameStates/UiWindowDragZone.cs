using UnityEngine;
using UnityEngine.EventSystems;

public class UiWindowDragZone : UIBehaviour, IDragHandler {
    public UiWindow window;
    public void OnDrag(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left && window.draggable)
            window.transform.position += (Vector3)eventData.delta;
    }
}