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
    public UnityEvent<MainMenuButton> onClick = new();
    public MainMenuSelectionState.Command command;

    public float HighlightIntensity {
        set {
            PropertyBlock.SetFloat("_Selected", value);
            renderer.SetPropertyBlock(PropertyBlock);
        }
    }

    public bool active;
    public bool isUnderPointer;

    public void OnPointerEnter(PointerEventData eventData) {
        isUnderPointer = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
        isUnderPointer = false;
    }
    public void OnPointerDown(PointerEventData eventData) {
        onClick.Invoke(this);
    }

    private void Update() {
        HighlightIntensity = active && isUnderPointer ? 1 : 0;
    }
}