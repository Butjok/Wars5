using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(MoveTypeAtlas))]
public class MoveTypeAtlas : ScriptableObject {

	[Serializable]
	public struct Entry {
		public MovePath.MoveType moveType;
		public Rect rect;
	}
	[SerializeField] private List<Entry> entries = new();

	private Dictionary<MovePath.MoveType, Rect> cache;
	private void EnsureCached() {
		cache = null;
		if (cache == null) {
			cache = new Dictionary<MovePath.MoveType, Rect>();
			foreach (var entry in entries)
				cache[entry.moveType] = entry.rect;
		}
	}

	public Rect this[MovePath.MoveType moveType] {
		get {
			EnsureCached();
			return cache[moveType];
		}
	}
	public IEnumerable<MovePath.MoveType> Keys => cache.Keys;
}