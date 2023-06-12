using UnityEditor;

[CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
[CustomPropertyDrawer(typeof(UnitTypeInfoDictionary))]
[CustomPropertyDrawer(typeof(PathSegmentTypeRectDictionary))]
[CustomPropertyDrawer(typeof(TileTypeSpriteDictionary))]
// [CustomPropertyDrawer(typeof(SerializableDialogueSideImageDictionary))]
public class WarsSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }