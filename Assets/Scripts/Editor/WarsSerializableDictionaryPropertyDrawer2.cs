using UnityEditor;

    [CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
    [CustomPropertyDrawer(typeof(TileTypeBuildingViewDictionary))]
    [CustomPropertyDrawer(typeof(TriggerNameColorDictionary))]
    [CustomPropertyDrawer(typeof(WeaponNameBattleAnimationInputsDictionary))]

    public class WarsSerializableDictionaryPropertyDrawer2 : SerializableDictionaryPropertyDrawer { }