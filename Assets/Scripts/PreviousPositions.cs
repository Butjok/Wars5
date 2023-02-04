using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public struct PreviousPositions:IEnumerable<Vector2Int> {
	public Vector2Int? position0, position1, position2, position3;
	public Vector2Int? this[int index] {
		get {
			return index switch {
				0 => position0,
				1 => position1,
				2 => position2,
				3 => position3,
				_ => throw new ArgumentOutOfRangeException(index.ToString())
			};
		}
		set {
			switch (index) {
				case 0:
					position0 = value;
					break;
				case 1:
					position1 = value;
					break;
				case 2:
					position2 = value;
					break;
				case 3:
					position3 = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(index.ToString());
			}
		}
	}
	public int Count {
		get {
			for (var i = 0; i < 4; i++)
				if (this[i] == null)
					return i;
			return 4;
		}
	}
	public void Clear() {
		position0 = position1 = position2 = position3 = null;
	}
	public void Add(Vector2Int position) {
		Assert.IsTrue(Count < 4);
		this[Count] = position;
	}
	public IEnumerator<Vector2Int> GetEnumerator() {
		if (position0 is { } p0)
			yield return p0;
		if (position1 is { } p1)
			yield return p1;
		if (position2 is { } p2)
			yield return p2;
		if (position3 is { } p3)
			yield return p3;
	}
	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}