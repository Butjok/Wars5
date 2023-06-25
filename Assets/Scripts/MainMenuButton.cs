using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

    public Renderer renderer;
    private MaterialPropertyBlock propertyBlock;
    public MaterialPropertyBlock PropertyBlock => propertyBlock ??= new MaterialPropertyBlock();
    public UnityEvent onClick = new();

    public void OnPointerEnter(PointerEventData eventData) {
        PropertyBlock.SetFloat("_Selected", 1);
        renderer.SetPropertyBlock(PropertyBlock);
    }
    public void OnPointerExit(PointerEventData eventData) {
        PropertyBlock.SetFloat("_Selected", 0);
        renderer.SetPropertyBlock(PropertyBlock);
    }
    public void OnPointerDown(PointerEventData eventData) {
        onClick.Invoke();
    }
}