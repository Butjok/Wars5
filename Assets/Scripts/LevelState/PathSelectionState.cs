using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class PathSelectionState : LevelState {

	public static Traverser traverser = new();
	public Unit unit;
	public List<Vector2Int> path;
	public MeshRenderer renderer;

	public PathSelectionState(Level level, Unit unit) : base(level) {
		this.unit = unit;
		Assert.IsTrue(unit.position.v != null);
		var position = (Vector2Int)unit.position.v;
		Assert.IsTrue(level.tiles.ContainsKey(position));
		traverser.Traverse(level.tiles.Keys, position, Cost);

		var terrain = GameObject.FindGameObjectWithTag("Terrain");
		if (terrain) {
			 renderer = terrain.GetComponent<MeshRenderer>();
			if (renderer) {
				var positions = level.tiles.Keys.Where(p => traverser.IsReachable(p)).Select(p=>(Vector4)(Vector2)p).ToList();
				var propertyBlock = new MaterialPropertyBlock();
				Debug.Log(positions.Count);
				propertyBlock.SetInteger("_Size", positions.Count);
				propertyBlock.SetVectorArray("_Positions", positions);
				renderer.SetPropertyBlock(propertyBlock);
			}
		}
	}

	public int? Cost(Vector2Int position, int length) {
		if (length >= Rules.MoveDistance(unit) ||
		    !level.TryGetTile(position, out var tile) ||
		    level.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
			return null;

		return Rules.MoveCost(unit, tile);
	}

	public override void Update() {
		base.Update();
		
		if (Input.GetMouseButtonDown(Mouse.right)) {
			level.State = new SelectionState(level);
			unit.view.selected.v = false;
			return;
		}
		if (Input.GetMouseButtonDown(Mouse.left)) {

			if (Mouse.TryGetPosition(out var position) && traverser.IsReachable(position.ToVector2().RoundToInt())) {
				path = traverser.ReconstructPath(position.ToVector2().RoundToInt());
				level.State = new UnitMovementAnimationState(level, unit, path);
				return;
			}
			else
				Sounds.NotAllowed.Play();
		}
	}

	public override void DrawGizmos() {
		base.DrawGizmos();
		foreach (var position in level.tiles.Keys)
			Handles.Label(position.ToVector3Int(), traverser.GetDistance(position).ToString(), new GUIStyle { normal = new GUIStyleState { textColor = Color.black } });
	}

	public override void Dispose() {
		base.Dispose();
		if (renderer) {
			var propertyBlock = new MaterialPropertyBlock();
			propertyBlock.SetInteger("_Size",0);
			renderer.SetPropertyBlock(propertyBlock);
		}
	}
}