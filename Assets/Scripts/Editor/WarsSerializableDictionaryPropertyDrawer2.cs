using UnityEditor;

    [CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
    [CustomPropertyDrawer(typeof(TileTypeBuildingViewDictionary))]
    [CustomPropertyDrawer(typeof(TriggerNameColorDictionary))]

    public class WarsSerializableDictionaryPropertyDrawer2 : SerializableDictionaryPropertyDrawer { }