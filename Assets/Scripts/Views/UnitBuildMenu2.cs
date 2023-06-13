using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UnitBuildMenu2 : MonoBehaviour {

    public UnitBuildMenuButton[] buttons = { };
    public UnitBuildMenuButton selectedButton;
    public Button buildButton;

    public FactionName factionName;
    public ColorName colorName;
    public Func<UnitType, int> getCost;
    public Func<UnitType, string> getFullName;
    public Func<UnitType, string> getDescription;

    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private void Start() {
        var factionName = FactionName.Novoslavia;
        var credits = 9000;
        Show(factionName, ColorName.Blue, credits,
            type => Rules.Cost(type) <= credits,
            type => Rules.Cost(type),
            type => UnitInfo.GetFullName(factionName, type),
            type => UnitInfo.GetDescription(factionName, type),
            type => UnitInfo.TryGetThumbnail(factionName, type));
    }

    public void Show(FactionName factionName, ColorName colorName, int credits,
        Func<UnitType, bool> isAvailable,
        Func<UnitType, int> getCost,
        Func<UnitType, string> getFullName,
        Func<UnitType, string> getDescription,
        Func<UnitType, Sprite> tryGetThumbnail) {

        gameObject.SetActive(true);

        this.factionName = factionName;
        this.colorName = colorName;
        this.getCost = getCost;
        this.getFullName = getFullName;
        this.getDescription = getDescription;

        foreach (var button in buttons) {
            button.Available = isAvailable(button.unitType);
            var newThumbnail = tryGetThumbnail(button.unitType);
            if (newThumbnail)
                button.image.sprite = newThumbnail;
        }

        if (buttons.Length > 0)
            Select(buttons[0]);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    private void Update() {
        var offset = 0f;//Input.GetAxisRaw("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.Tab))
            offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
        if (!Mathf.Approximately(0, offset)) {
            var index = Array.IndexOf(buttons, selectedButton);
            Select(buttons[(index + offset.Sign()).PositiveModulo(buttons.Length)]);
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            TryBuild();
    }

    public void Select(UnitBuildMenuButton button) {

        selectedButton = button;

        foreach (var sibling in buttons)
            if (sibling != button)
                sibling.Unemphasize();
    
        button.Emphasize();
        // button.transform.SetSiblingIndex(button.transform.parent.childCount-1);

        if (nameText)
            nameText.text = UnitInfo.GetShortName(button.unitType);
        if (descriptionText)
            descriptionText.text = $"{getDescription(button.unitType)}";

        if (buildButton)
            buildButton.interactable = button.Available;
    }

    public bool TryBuild() {
        if (selectedButton) {

            return true;
        }
        return false;
    }
}