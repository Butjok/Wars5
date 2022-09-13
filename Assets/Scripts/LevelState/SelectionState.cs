using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionState : State2<Game2> {

	public List<Unit> unmovedUnits;
	public int unitIndex = -1;
	public Unit cycledUnit;
	public bool turnStart;

	public SelectionState(Game2 parent, bool turnStart = false) : base(parent) {
		unmovedUnits = parent.units.Values
			.Where(unit => unit.player == parent.CurrentPlayer && !unit.moved.v)
			.OrderBy(unit => Vector3.Distance(CameraRig.instance.transform.position, unit.view.center.position))
			.ToList();
		this.turnStart = turnStart;
	}

	public override void Start() {
		if (turnStart)
			if (!parent.levelLogic.OnTurnStart())
				PauseTo(new TurnStartAnimationState(parent));
	}

	public override void Update() {
		base.Update();

		if (parent.CurrentPlayer.IsAi) {
			var action = parent.CurrentPlayer.action = parent.CurrentPlayer.FindAction();
			if (action != null)
				ChangeTo(new UnitMovementAnimationState(parent, action.unit, action.path));
			else
				EndTurn();
		}
		else {
			if (cycledUnit != null && Camera.main) {
				var worldPosition = cycledUnit.view.center.position;
				var screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
				if (screenPosition.x is < 0 or > 1 || screenPosition.y is < 0 or > 1)
					cycledUnit = null;
			}
			if (Input.GetKeyDown(KeyCode.Tab)) {
				if (unmovedUnits.Count > 0) {
					unitIndex = (unitIndex + 1) % unmovedUnits.Count;
					var next = unmovedUnits[unitIndex];
					CameraRig.instance.Jump(next.view.center.position);
					cycledUnit = next;
				}
				else
					UiSound.Instance.notAllowed.Play();
			}
			else if (Input.GetMouseButtonDown(Mouse.left) &&
			         Mouse.TryGetPosition(out Vector2Int position) &&
			         parent.TryGetUnit(position, out var unit)) {

				if (unit.player != parent.CurrentPlayer || unit.moved.v)
					UiSound.Instance.notAllowed.Play();
				else
					SelectUnit(unit);
			}
			else if (Input.GetKeyDown(KeyCode.Return))
				if (cycledUnit != null)
					SelectUnit(cycledUnit);
				else
					UiSound.Instance.notAllowed.Play();

			else if (Input.GetKeyDown(KeyCode.KeypadEnter))
				EndTurn();

			else if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftShift))
				ChangeTo(new VictoryState(parent));

			else if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift))
				ChangeTo(new DefeatState(parent));
		}
	}

	public void EndTurn() {
		foreach (var unit in parent.units.Values)
			unit.moved.v = false;
		if (parent.Turn is not { } turn)
			throw new Exception();
		parent.Turn = turn + 1;
		if (!parent.levelLogic.OnTurnEnd())
			ChangeTo(new SelectionState(parent, true));
	}

	public void SelectUnit(Unit unit) {
		unit.view.selected.v = true;
		ChangeTo(new PathSelectionState(parent, unit));
	}
}