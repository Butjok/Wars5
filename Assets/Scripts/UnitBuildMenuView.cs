using System;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class UnitBuildMenuView : MonoBehaviour {

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
    public Game game;
    public UnitType defaultUnitType = UnitType.Infantry;
    public Button build;
    public Building building;
    public UnitType unitType;
    public Button[] unitTypeButtons = Array.Empty<Button>();
    public Transform unitTypeButtonsContainer;

    public void Build() {
        building.game.input.buildUnitType = unitType;
    }
    public void Cancel() {
        building.game.input.cancel = true;
    }

    public void Show(Building building) {
        this.building = building;
        gameObject.SetActive(true);
        var player = building.player.v;
        credits.text = string.Format(creditsFormat, player.credits);
        UnitType = defaultUnitType;
        foreach (var button in unitTypeButtons) {
            var type = Enum.Parse<UnitType>(button.name);
            button.interactable = player.credits >= Rules.Cost(type, player);
        }
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
    public bool Visible => gameObject.activeSelf;

    public UnitType UnitType {
        set {
            Assert.IsNotNull(building);

            unitType = value;

            var player = building.player.v;
            if (!player.co.unitTypesInfoOverride.TryGetValue(value, out var info))
                info = player.co.unitTypesInfo.get[value];

            typeName.text = string.Format(typeNameFormat, info.name);
            description.text = string.Format(descriptionFormat, info.description);
            thumbnail.sprite = info.thumbnail;
            moveDistance.text = Rules.MoveDistance(value, player).ToString();
            var cost = Rules.Cost(value, player);
            this.cost.text = string.Format(costFormat, cost);

            var attackRange = Rules.AttackRange(value, player);
            this.attackRange.text = attackRange[1] > 0
                ? attackRange[0] == attackRange[1]
                    ? string.Format(attackRangeFormatCollapsed, attackRange[0])
                    : string.Format(attackRangeFormat, attackRange[0], attackRange[1])
                : attackRangeNotAvailable;

            build.interactable = cost <= player.credits;
        }
    }
    public string UnitTypeName {
        set => UnitType = Enum.Parse<UnitType>(value);
    }

    [Button]
    private void SelectRandomUnitType() {
        UnitType = new[] {
            UnitType.Infantry,
            UnitType.AntiTank,
            UnitType.Artillery,
            UnitType.Apc,
            UnitType.TransportHelicopter,
            UnitType.AttackHelicopter,
            UnitType.FighterJet,
            UnitType.Bomber,
            UnitType.Recon,
            UnitType.LightTank,
        }.Random();
    }

    [Button]
    private void ToggleVisibility() {
        if (!Visible) {
            game = FindObjectOfType<Game>();
            Assert.IsTrue(game);
            var building = game.buildings.Values.Where(building => building.IsAccessible).OrderBy(_ => UnityEngine.Random.value).First();
            Show(building);
        }
        else
            Hide();
    }

    [Button]
    private void FindUnitTypeButtons() {
        if (unitTypeButtonsContainer)
            unitTypeButtons = unitTypeButtonsContainer.GetComponentsInChildren<Button>();
    }
}