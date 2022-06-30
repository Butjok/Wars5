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
			var newUnit = value;
			var oldUnit = this[position];
			
			if (oldUnit == newUnit && oldUnit?.position.v == newUnit?.position.v)
				return;
			
			if (oldUnit != null) {
				map.Remove(position);
				oldUnit.position.v = null;
			}
			map.Add(position, newUnit);
			set.Add(newUnit);
			if (newUnit.position.v is { } oldPosition) {
				Assert.AreEqual(newUnit, this[oldPosition]);
				map.Remove(oldPosition);
			}
			newUnit.position.v = position;
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