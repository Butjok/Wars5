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
    public Vector2 highlightFramePadding = new Vector2(0, 0);

    public Action<Button, UnitAction> onClick;

    public Dictionary<UnitAction, Button> buttons = new();

    private Action enqueueCancelCommand;
    public void Show(Action enqueueCancelCommand, IEnumerable<UnitAction> actions, Action<Button, UnitAction> onClick) {

        this.enqueueCancelCommand = enqueueCancelCommand;
        
        this.onClick = onClick;
        foreach (var action in actions) {
        
            var button = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            buttons[action] = button;
            button.gameObject.SetActive(true);
            
            var text = action.type.ToString();
            if (action.targetUnit != null)
                text += $" {action.targetUnit.type}";
            if (action.type == UnitActionType.Attack)
                text += $" with {action.weaponName}";
            button.GetComponentInChildren<TMP_Text>().text = text;
            
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
            highlightFrame.sizeDelta = buttonRectTransform.sizeDelta+highlightFramePadding*2;
        }
        else {
            highlightFrame.DOAnchorPos(buttonRectTransform.anchoredPosition , highlightFrameMoveDuration).SetEase(highlightFrameMoveEase);
            highlightFrame.DOSizeDelta(buttonRectTransform.sizeDelta + highlightFramePadding*2, highlightFrameMoveDuration).SetEase(highlightFrameMoveEase);
        }
    }

    public void Cancel() {
        enqueueCancelCommand?.Invoke();
    }
}