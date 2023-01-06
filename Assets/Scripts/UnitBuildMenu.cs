using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class UnitBuildMenu : MonoBehaviour {

    public TMP_Text credits;
    public TMP_Text description;
    public TMP_Text moveDistance;
    public TMP_Text attackRange;
    public TMP_Text cost;
    public TMP_Text typeName;
    public string costFormat = "−{0}¤";
    public string attackRangeFormat = "[{0}..{1}]";
    public string attackRangeFormatCollapsed = "{0}";
    public string attackRangeNotAvailable = "<color=grey>N/A</color>";
    public string creditsFormat = "{0}¤";
    public string descriptionFormat = "{0}";
    public string typeNameFormat = "{0}";
    public Image thumbnail;
    public UnitType defaultUnitType = UnitType.Infantry;
    [FormerlySerializedAs("build")] public Button buildButton;
    public Building building;
    public UnitType unitType;
    public Button[] unitTypeButtons = Array.Empty<Button>();
    public Transform unitTypeButtonsContainer;

    public bool TryBuild() {
        if (!building.player.v.CanAfford(unitType))
            return false;
        building.main.stack.Push(unitType);
        building.main.commands.Enqueue(UnitBuildState.build);
        return true;
    }
    public void Cancel() {
        building.main.commands.Enqueue(UnitBuildState.close);
    }

    public void Show(Building building) {
        this.building = building;
        gameObject.SetActive(true);
        var player = building.player.v;
        credits.text = string.Format(creditsFormat, player.credits);
        UnitType = defaultUnitType;
        foreach (var button in unitTypeButtons) {
            var type = Enum.Parse<UnitType>(button.name);
            button.interactable = player.CanAfford(type);
        }
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
    public bool Visible => gameObject.activeSelf;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            var button = unitTypeButtons.First(button => button.name == unitType.ToString());
            var index = Array.IndexOf(unitTypeButtons, button);
            var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
            var nextButton = unitTypeButtons[(index + offset).PositiveModulo(unitTypeButtons.Length)];
            UnitType = Enum.Parse<UnitType>(nextButton.name);
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            if (!TryBuild())
                UiSound.Instance.notAllowed.PlayOneShot();
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) {
            Cancel();
        }
    }

    public UnitType UnitType {
        set {
            Assert.IsNotNull(building);

            unitType = value;

            var player = building.player.v;
            if (!player.co.unitTypesInfoOverride.TryGetValue(unitType, out var info))
                info = player.co.unitTypesInfo.get[unitType];

            typeName.text = string.Format(typeNameFormat, info.name);
            description.text = string.Format(descriptionFormat, info.description);
            thumbnail.sprite = info.thumbnail;
            moveDistance.text = Rules.MoveDistance(unitType, player).ToString();
            var cost = Rules.Cost(unitType, player);
            this.cost.text = string.Format(costFormat, cost);

            var attackRange = Rules.AttackRange(unitType, player);
            this.attackRange.text = attackRange[1] > 0
                ? attackRange[0] == attackRange[1]
                    ? string.Format(attackRangeFormatCollapsed, attackRange[0])
                    : string.Format(attackRangeFormat, attackRange[0], attackRange[1])
                : attackRangeNotAvailable;

            buildButton.interactable = player.CanAfford(unitType);
        }
    }
    
    public string UnitTypeName {
        set => UnitType = Enum.Parse<UnitType>(value);
    }
}