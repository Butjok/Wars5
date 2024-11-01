using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = nameof(MoveSequenceAtlas))]
public class MoveSequenceAtlas : ScriptableObject {
	[FormerlySerializedAs("atlas")] public PathSegmentTypeRectDictionary uv = new();
}

[Serializable]
public class PathSegmentTypeRectDictionary : SerializableDictionary<MoveSequence.Segment.Type, Rect>{}

[Serializable]
public class TileTypeSpriteDictionary : SerializableDictionary<TileType, Sprite>{}
[Serializable]
public class UnitTypeSpriteDictionary : SerializableDictionary<UnitType, Sprite>{}