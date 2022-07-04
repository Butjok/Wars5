using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class Unit : IDisposable {

	public Level level;
	public UnitType type;
	public Player player;
	public UnitView view;

	public ChangeTracker<Vector2Int?> position;
	public ChangeTracker<Vector2Int> forward;
	public ChangeTracker<bool> moved;
	public ChangeTracker<int> hp;
	public ChangeTracker<bool> selected;
	public ChangeTracker<IEnumerable<Vector2>> path;

	public Unit(Level level,Player player, bool moved = false, UnitType type = UnitType.Infantry, Vector2Int? position = null, Vector2Int? rotation =null, int hp = 10, UnitView viewPrefab = null) {
		
		Assert.IsNotNull(level);
		this.level = level;
		
		if (!viewPrefab)
			viewPrefab = WarsResources.test.v;
		Assert.IsTrue(viewPrefab);
		
		view = Object.Instantiate(viewPrefab);
		Object.DontDestroyOnLoad(view.gameObject);
		view.unit = this;
		
		this.position = new ChangeTracker<Vector2Int?>(old => {
			if (this.position.v is { } newPosition) {
				view.visible.v = true;
				view.position.v = newPosition;
			}
			else
				view.visible.v = false;

			level.unitMap[this] = this.position.v;
		});

		this.forward = new ChangeTracker<Vector2Int>(_ => view.forward.v = this.forward.v);
		this.moved = new ChangeTracker<bool>(_ => view.moved.v = this.moved.v);
		this.hp = new ChangeTracker<int>(_ => view.hp.v = this.hp.v);
		this.selected = new ChangeTracker<bool>(_ => view.selected.v = this.selected.v);

		this.type = type;
		this.player = player;
		this.position.v = position;
		this.forward.v = rotation??Vector2Int.up;
		this.moved.v = moved;
		this.hp.v = hp;
	}

	public void Dispose() {
		position.v = null;
		Object.Destroy(view.gameObject);
	}
}