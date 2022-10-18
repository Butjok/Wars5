using UnityEditor;

[CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
[CustomPropertyDrawer(typeof(MoodSpriteDictionary))]
[CustomPropertyDrawer(typeof(TileTypeGameObjectDictionary))]
[CustomPropertyDrawer(typeof(UnitTypeInfoDictionary))]
public class WarsSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }