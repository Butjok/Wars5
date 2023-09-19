using System;
using UnityEngine;
using static Gettext;

public static class UnitInfo {

    public static string GetShortName(UnitType unitType) => unitType switch {
        UnitType.Infantry => _("Infantry"),
        UnitType.Apc=> _("APC"),
        UnitType.AntiTank => _("Anti-tank"),
        UnitType.Artillery => _("Artillery"),
        UnitType.Recon => _("Recon"),
        UnitType.LightTank => _("Light tank"),
        UnitType.Rockets => _("Rockets"),
        UnitType.MediumTank => _("Medium tank"),
        UnitType.TransportHelicopter => _("Transport helicopter"),
        UnitType.AttackHelicopter => _("Attack helicopter"),
        UnitType.FighterJet => _("Fighter jet"),
        UnitType.Bomber => _("Bomber"),
        _ => unitType.ToString()
    };

    public static string GetFullName(FactionName factionName, UnitType unitType) => (factionId: factionName, unitType)switch {
        (FactionName.Novoslavia, UnitType.Infantry) => _("Mechanized infantry"),
        (FactionName.Novoslavia, UnitType.AntiTank) => _("RPG squad"),
        (FactionName.UnitedTreaty, UnitType.Infantry) => _("Infantry squad"),
        (FactionName.UnitedTreaty, UnitType.AntiTank) => _("Anti-tank squad"),
        _ => GetShortName(unitType)
    };

    public static string GetDescription(FactionName factionName, UnitType unitType) => (factionId: factionName, unitType)switch {
        (FactionName.Novoslavia, UnitType.Infantry) =>
            _("Novoslavian mechanized infantry is capable of effectively attacking enemy infantry units and capturing buildings."),
        (FactionName.Novoslavia, UnitType.AntiTank) =>
            _("RPG rocket launcher deals significant damage to tanks and other armored vehicles. The RPG squad is also capable of capturing buildings."),
        (FactionName.UnitedTreaty, UnitType.Infantry) =>
            _("Infantry squad is capable of effectively attacking enemy infantry units and capturing buildings."),
        (FactionName.UnitedTreaty, UnitType.AntiTank) =>
            _("Having a recoilless rifle as their primary weapon the anti-tank squad can deal significant damage to tanks and other armored vehicles. It is also capable of capturing buildings."),
        _ => _("No description yet.")
    };

    public static Sprite TryGetThumbnail(FactionName factionName, UnitType unitType) => Resources.Load<Sprite>($"Thumbnail{factionName}{unitType}");
}