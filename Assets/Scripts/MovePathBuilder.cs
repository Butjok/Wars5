using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class MovePathBuilder {

	public readonly Vector2Int startPosition;
	private List<Vector2Int> positions = new();
	private HashSet<Vector2Int> set = new();

	public IReadOnlyList<Vector2Int> Positions => positions;

	public MovePathBuilder(Vector2Int startPosition) {
		this.startPosition = startPosition;
		Clear();
	}

	public void Clear() {
		positions.Clear();
		positions.Add(startPosition);
		set.Clear();
		set.Add(startPosition);
	}

	public void Add(Vector2Int position) {

		var previous = positions.Last();
		Assert.AreEqual(1, (position - previous).ManhattanLength());

		if (!set.Contains(position)) {
			positions.Add(position);
			set.Add(position);
		}
		else
			for (var i = positions.Count - 1; i >= 0; i--) {
				if (positions[i] == position)
					break;
				set.Remove(positions[i]);
				positions.RemoveAt(i);
			}
	}
}