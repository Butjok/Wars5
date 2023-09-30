using System;
using System.Web.Compilation;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UnitBuildMenu2 : MonoBehaviour {

    public UnitBuildMenuButton[] buttons = { };
    public UnitBuildMenuButton selectedButton;
    public Button buildButton;

    public FactionName factionName = FactionName.Novoslavia;
    public ColorName colorName = ColorName.Red;
    public Func<UnitType, int> getCost = type => Rules.Cost(type);
    public Func<UnitType, string> getFullName = type => UnitInfo.GetFullName(FactionName.Novoslavia, type);
    public Func<UnitType, string> getDescription = type => UnitInfo.GetDescription(FactionName.Novoslavia, type);

    public TMP_Text nameText;
    public TMP_Text descriptionText;

    public Action<UnitType> build = type => { };
    public Action cancel = () => { };

    public Image circle;
    public BuildingView buildingView;
    public float circleRadius = .6f;
    public Vector3 circleWorldOffset = new(0, 0.25f, 0);
    public Camera mainCamera;
    public Camera previewCamera;

    // private void Start() {
    //     var factionName = FactionName.Novoslavia;
    //     var credits = 9000;
    //     Show(factionName, ColorName.Blue, credits,
    //         type => Rules.Cost(type) <= credits,
    //         type => Rules.Cost(type),
    //         type => UnitInfo.GetFullName(factionName, type),
    //         type => UnitInfo.GetDescription(factionName, type),
    //         type => UnitInfo.TryGetThumbnail(factionName, type));
    // }

    public void Show(
        Action<UnitType> build, Action cancel,
        FactionName factionName, ColorName colorName, int credits,
        Func<UnitType, bool> isAvailable,
        Func<UnitType, int> getCost,
        Func<UnitType, string> getFullName,
        Func<UnitType, string> getDescription,
        Func<UnitType, Sprite> tryGetThumbnail,
        BuildingView buildingView = null) {

        gameObject.SetActive(true);

        this.build = build;
        this.cancel = cancel;
        this.factionName = factionName;
        this.colorName = colorName;
        this.getCost = getCost;
        this.getFullName = getFullName;
        this.getDescription = getDescription;
        this.buildingView = buildingView;

        foreach (var button in buttons) {
            button.Available = isAvailable(button.unitType);
            var newThumbnail = tryGetThumbnail(button.unitType);
            if (newThumbnail)
                button.image.sprite = newThumbnail;
        }

        if (buttons.Length > 0)
            Select(buttons[0]);

        previewCamera.enabled = true;
    }

    public void Hide() {
        gameObject.SetActive(false);
        circle.enabled = false;
        previewCamera.enabled = false;
    }

    private void Update() {
        var offset = 0f; //Input.GetAxisRaw("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.Tab))
            offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
        if (!Mathf.Approximately(0, offset)) {
            var index = Array.IndexOf(buttons, selectedButton);
            Select(buttons[(index + offset.Sign()).PositiveModulo(buttons.Length)]);
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            if (selectedButton)
                build(selectedButton.unitType);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right))
            cancel();
    }

    public void LateUpdate() {
        if (mainCamera && circle && buildingView) {
            var screenPoint0 = mainCamera.WorldToScreenPoint(buildingView.transform.position + circleWorldOffset);
            var screenPoint1 = mainCamera.WorldToScreenPoint(buildingView.transform.position + circleWorldOffset + mainCamera.transform.right * circleRadius);
            if (screenPoint0.z > 0 && screenPoint1.z > 0) {
                circle.enabled = true;
                var center = screenPoint0;
                var halfLength = (screenPoint0 - screenPoint1).magnitude;
                circle.rectTransform.anchoredPosition = center - new Vector3(halfLength, halfLength);
                circle.rectTransform.sizeDelta = new Vector2(halfLength, halfLength) * 2;
                circle.materialForRendering.SetFloat("_Size", halfLength * 2);
            }
            else
                circle.enabled = false;
        }
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
            build(selectedButton.unitType);
            return true;
        }
        return false;
    }

    public void Cancel() {
        cancel();
    }
}