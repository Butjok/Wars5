using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class PathSelectionState : LevelState {

	public static Traverser traverser = new();
	public Unit unit;
	public List<Vector2Int> path;

	public PathSelectionState(Level level, Unit unit) : base(level) {
		this.unit = unit;
		Assert.IsTrue(unit.position.v != null);
		var position = (Vector2Int)unit.position.v;
		Assert.IsTrue(level.tiles.ContainsKey(position));
		traverser.Traverse(level.tiles.Keys, position, Cost);
	}

	public int? Cost(Vector2Int position, int length) {
		if (length >= Rules.MoveDistance(unit) ||
		    !level.TryGetTile(position, out var tile) ||
		    level.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
			return null;

		return Rules.MoveCost(unit, tile);
	}

	public override void Update() {
		if (Input.GetMouseButtonDown(Mouse.right)) {
			level.State = new SelectionState(level);
			unit.view.selected.v = false;
			return;
		}
		if (Input.GetMouseButtonDown(Mouse.left)) {

			if (Mouse.TryGetPosition(out var position) && traverser.IsReachable(position.RoundToInt())) {
				path = traverser.ReconstructPath(position.RoundToInt());
				level.State = new UnitMovementAnimationState(level, unit, path);
				return;
			}
			else
				UiSound.NotAllowed();
		}
	}

	public override void DrawGizmos() {
		foreach (var position in level.tiles.Keys)
			Handles.Label(position.ToVector3Int(), traverser.GetDistance(position).ToString(), new GUIStyle { normal = new GUIStyleState { textColor = Color.black } });
	}
}