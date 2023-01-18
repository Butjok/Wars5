using UnityEditor;

    [CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
    [CustomPropertyDrawer(typeof(TileTypeBuildingViewDictionary))]
    public class WarsSerializableDictionaryPropertyDrawer2 : SerializableDictionaryPropertyDrawer { }