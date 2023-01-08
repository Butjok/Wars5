using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(PathAtlas))]
public class PathAtlas : ScriptableObject {
	public PathSegmentTypeRectDictionary atlas = new();
}

[Serializable]
public class PathSegmentTypeRectDictionary : SerializableDictionary<Path.Segment.Type, Rect>{}