using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiWindow : UIBehaviour,  IPointerDownHandler {
    
    public TMP_Text title;
    public RectTransform body;
    public Button closeButton;
    
    public bool draggable = true;
    public Action close;

    public string Title {
        get => title.text;
        set => title.text = value;
    }

    protected override void Start() {
        base.Start();
        if (closeButton)
            closeButton.onClick.AddListener(Close);
    }
    public void Close() {
        close?.Invoke();
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
    public void Show() {
        gameObject.SetActive(true);
    }
    public void BringToFront() {
        transform.SetAsLastSibling();
    }
    public void BringToBack() {
        transform.SetAsFirstSibling();
    }
    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left)
            BringToFront();
    }
}