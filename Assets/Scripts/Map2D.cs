using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public class Map2D<T> : IEnumerable<KeyValuePair<Vector2Int, T>> {

	public Vector2Int Min { get; }
	public Vector2Int Max { get; }
	public Vector2Int[] positions;

	private int width;
	private T[] data;
	private bool[] hasValue;

	public Map2D(Vector2Int min, Vector2Int max) {
		
		Assert.IsTrue(max.x >= min.x);
		Assert.IsTrue(max.y >= min.y);
		
		Min = min;
		Max = max;
		width = max.x - min.x + 1;
		var height = max.y - min.y + 1;
		data = new T[width * height];
		hasValue = new bool[width * height];

		 positions = new Vector2Int[data.Length];
		var i = 0;
		for (var y = Min.y; y <= Max.y; y++)
		for (var x = Min.x; x <= Max.x; x++)
			positions[i++] = new Vector2Int(x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InBounds(Vector2Int position) {
		return Min.x <= position.x && position.x <= Max.x &&
		       Min.y <= position.y && position.y <= Max.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ContainsKey(Vector2Int position) {
		return InBounds(position) && hasValue[Index(position)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int Index(Vector2Int position) {
		Assert.IsTrue(InBounds(position));
		var offset = position - Min;
		return width * offset.y + offset.x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Vector2Int FromIndex(int index) {
		Assert.IsTrue(0 <= index);
		Assert.IsTrue(index < data.Length);
		return Min + new Vector2Int(index % width, index / width);
	}

	public T this[Vector2Int position] {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get {
			Assert.IsTrue(ContainsKey(position));
			return data[Index(position)];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set {
			Assert.IsTrue(InBounds(position));
			var index = Index(position);
			data[index] = value;
			hasValue[index] = true;
		}
	}

	public void Remove(Vector2Int position) {
		Assert.IsTrue(InBounds(position));
		var index = Index(position);
		data[index] = default;
		hasValue[index] = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue(Vector2Int position, out T value) {
		if (ContainsKey(position)) {
			value = data[Index(position)];
			return true;
		}
		value = default;
		return false;
	}

	public IEnumerable<Vector2Int> Keys {
		get {
			for (var i = 0; i < data.Length; i++)
				if (hasValue[i])
					yield return FromIndex(i);
		}
	}

	public IEnumerable<T> Values {
		get {
			for (var i = 0; i < data.Length; i++)
				if (hasValue[i])
					yield return data[i];
		}
	}

	public IEnumerator<KeyValuePair<Vector2Int, T>> GetEnumerator() {
		foreach (var position in Keys)
			yield return new KeyValuePair<Vector2Int, T>(position, this[position]);
	}
	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}