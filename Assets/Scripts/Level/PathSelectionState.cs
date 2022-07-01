using UnityEngine;

public class PathSelectionState : GameState {

	static public Traverser traverser;
	public Unit unit;
	
	public PathSelectionState(Level level, Unit unit) : base(level) {
		this.unit = unit;
		unit.selected.v = true;
	}
	
	public override void Dispose() {
		unit.selected.v = false;
	}
	
	public override void Update() {
		if (Input.GetMouseButtonDown(Mouse.right)) {
			level.state.v = new SelectionState(level);
			return;
		}
	}
}