using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitMap : IEnumerable<Unit> {

	private Dictionary<Vector2Int, Unit> map = new();
	private HashSet<Unit> set = new();

	public Unit this[Vector2Int position] {
		get => map.TryGetValue(position, out var unit) ? unit : null;
		set {
			var unit = value;
			var other = this[position];
			
			if (other == unit && other?.position.v == unit?.position.v)
				return;
			
			if (other != null) {
				map.Remove(position);
				other.position.v = null;
			}
			
			if (unit.position.v is { } oldPosition) {
				Assert.AreEqual(unit, this[oldPosition]);
				map.Remove(oldPosition);
			}
			
			unit.position.v = position;
			
			map.Add(position, unit);
			set.Add(unit);
		}
	}

	public Vector2Int? this[Unit unit] {
		get => unit.position.v;
		set {
			if (value is { } newPosition) {
				if (this[newPosition] == unit)
					return;
				this[newPosition] = unit;
			}
			else if (unit.position.v is { } position)
				this[position] = null;
		}
	}

	public void Register(Unit unit) {
		set.Add(unit);
	}
	public void Remove(Unit unit) {
		if (unit.position.v is { } position) {
			Assert.AreEqual(unit, this[position]);
			map.Remove(position);
		}
		set.Remove(unit);
	}

	public IEnumerator<Unit> GetEnumerator() {
		return set.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}