using System;
using UnityEngine;
using static Gettext;

public static class UnitDescriptions {

    public static string GetName(FactionId factionId, UnitType unitType) => (factionId, unitType)switch {
        (FactionId.Novoslavia, UnitType.Infantry) => _("Mechanized infantry"),
        (FactionId.Novoslavia, UnitType.AntiTank) => _("RPG squad"),
        (FactionId.UnitedTreaty, UnitType.Infantry) => _("Infantry squad"),
        (FactionId.UnitedTreaty, UnitType.AntiTank) => _("Anti-tank squad"),
        _ => throw new Exception()
    };

    public static string GetDescription(FactionId factionId, UnitType unitType) => (factionId, unitType)switch {
        (FactionId.Novoslavia, UnitType.Infantry) =>
            _("Novoslavian mechanized infantry is capable of effectively attacking enemy infantry units and capturing buildings."),
        (FactionId.Novoslavia, UnitType.AntiTank) =>
            _("RPG rocket launcher deals significant damage to tanks and other armored vehicles. The RPG squad is also capable of capturing buildings."),
        (FactionId.UnitedTreaty, UnitType.Infantry) =>
            _("Infantry squad is capable of effectively attacking enemy infantry units and capturing buildings."),
        (FactionId.UnitedTreaty, UnitType.AntiTank) =>
            _("Having a recoilless rifle as their primary weapon the anti-tank squad can deal significant damage to tanks and other armored vehicles. It is also capable of capturing buildings."),
        _ => throw new Exception()
    };

    public static Sprite TryGetThumbnail(FactionId factionId, UnitType unitType) => Resources.Load<Sprite>($"Thumbnail{factionId}{unitType}");
}