using UnityEditor;

[CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
[CustomPropertyDrawer(typeof(TileTypeGameObjectDictionary))]
[CustomPropertyDrawer(typeof(UnitTypeInfoDictionary))]
[CustomPropertyDrawer(typeof(TileTypeColorDictionary))]
[CustomPropertyDrawer(typeof(PathSegmentTypeRectDictionary))]
[CustomPropertyDrawer(typeof(SerializableDialogueSideImageDictionary))]
public class WarsSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }