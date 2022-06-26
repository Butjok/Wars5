using System;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class Unit : IDisposable {

	public Units units;
	public UnitType type;
	public Player player;
	private UnitView view;

	public ChangeTracker<Vector2Int?> position;
	public ChangeTracker<Vector2Int> rotation;
	public ChangeTracker<bool> moved;
	public ChangeTracker<int> hp;

	public Unit(Units units, Player player, bool moved = false, UnitType type = UnitType.Infantry, Vector2Int? position = null, Vector2Int? rotation =null, int hp = 10, UnitView viewPrefab = null) {

		view = Object.Instantiate(viewPrefab, units.go.transform);
		view.unit = this;
		
		this.position = new ChangeTracker<Vector2Int?>(old => {

			if (this.position.v is { } newPosition) {
				units.at.Add(newPosition, this);
				view.visible.v = true;
				view.position.v = newPosition;
			}
			else
				view.visible.v = false;

			if (old is { } oldPosition) {
				units.at.Remove(oldPosition);
			}
		});

		this.rotation = new ChangeTracker<Vector2Int>(_ => view.rotation.v = this.rotation.v);
		this.moved = new ChangeTracker<bool>(_ => view.moved.v = this.moved.v);
		this.hp = new ChangeTracker<int>(_ => view.hp.v = this.hp.v);

		Assert.IsNotNull(units);
		Assert.IsNotNull(player);

		units.all.Add(this);

		if (!viewPrefab)
			viewPrefab = WarsResources.test.v;
		Assert.IsTrue(viewPrefab);

		this.units = units;
		this.type = type;
		this.player = player;
		this.position.v = position;
		this.rotation.v = rotation??Vector2Int.up;
		this.moved.v = moved;
		this.hp.v = hp;
	}

	public void Dispose() {
		position.v = null;
		units.all.Remove(this);
		Object.Destroy(view.gameObject);
	}
}