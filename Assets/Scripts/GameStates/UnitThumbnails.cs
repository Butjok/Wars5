using UnityEngine;
using UnityEngine.Assertions;

public static class UnitThumbnails {

    public static Sprite TryGet(Unit unit) {
        return null;
    }
    
    public static Sprite TryGet(FactionName faction,  UnitType type,Color color) {
        var exactThumbnail = Resources.Load<Sprite>($"UnitThumbnail{faction}{type}");
        return null;
    }
    
    public static Sprite TryGet(PersonName person,  UnitType type,Color color) {
         var hasFaction=People.TryGetFaction(person, out var faction);
         Assert.IsTrue(hasFaction);
         
         return TryGet(faction, type,color);
    }
}