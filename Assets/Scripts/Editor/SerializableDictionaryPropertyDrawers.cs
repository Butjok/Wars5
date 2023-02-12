using UnityEditor;

[CustomPropertyDrawer(typeof(UnitTypeUnitViewDictionary))]
[CustomPropertyDrawer(typeof(TileTypeGameObjectDictionary))]
[CustomPropertyDrawer(typeof(UnitTypeInfoDictionary))]
[CustomPropertyDrawer(typeof(TileTypeColorDictionary))]
[CustomPropertyDrawer(typeof(TileTypeBuildingViewDictionary))]
[CustomPropertyDrawer(typeof(PathSegmentTypeRectDictionary))]
public class WarsSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }