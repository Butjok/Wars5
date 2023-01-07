using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitActionsPanel : MonoBehaviour {

    public RectTransform container;
    public Button buttonPrefab;
    public RectTransform highlightFrame;
    public float highlightFrameMoveDuration = .5f;
    public Ease highlightFrameMoveEase = Ease.Linear;

    public Action<Button, UnitAction> onClick;

    public Dictionary<UnitAction, Button> buttons = new();
    public Main main;

    public void Show(Main main,IEnumerable<UnitAction> actions, Action<Button, UnitAction> onClick) {

        this.main = main;
        
        this.onClick = onClick;
        foreach (var action in actions) {
            var button = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            buttons[action] = button;
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = action.type.ToString();
            button.onClick.AddListener(() => onClick(button,action));
        }
        gameObject.SetActive(true);

        highlightFrame.gameObject.SetActive(false);
    }

    public void Hide() {
        foreach (var button in buttons.Values)
            Destroy(button.gameObject);
        buttons.Clear();
        gameObject.SetActive(false);
    }
    
    public void HighlightAction(UnitAction action) {
        var buttonRectTransform = buttons[action].GetComponent<RectTransform>();
        if (!highlightFrame.gameObject.activeSelf) {
            highlightFrame.gameObject.SetActive(true);
            highlightFrame.anchoredPosition = buttonRectTransform.anchoredPosition;
            highlightFrame.sizeDelta = buttonRectTransform.sizeDelta;
        }
        else {
            highlightFrame.DOAnchorPos(buttonRectTransform.anchoredPosition, highlightFrameMoveDuration).SetEase(highlightFrameMoveEase);
            highlightFrame.DOSizeDelta(buttonRectTransform.sizeDelta, highlightFrameMoveDuration).SetEase(highlightFrameMoveEase);
        }
    }

    public void Cancel() {
        main.commands.Enqueue("action-selection-state.cancel");
    }
}