using UnityEditor;

[CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
[CustomPropertyDrawer(typeof(MoodSpriteDictionary))]
[CustomPropertyDrawer(typeof(TileTypeGameObjectDictionary))]
[CustomPropertyDrawer(typeof(UnitTypeInfoDictionary))]
[CustomPropertyDrawer(typeof(TileTypeColorDictionary))]
[CustomPropertyDrawer(typeof(TileTypeBuildingViewDictionary))]
public class WarsSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }