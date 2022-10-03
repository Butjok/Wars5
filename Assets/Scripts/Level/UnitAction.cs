using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public enum UnitActionType { Stay, Join, Capture, Attack, GetIn, Drop, Supply }
public class UnitAction : IDisposable {

	public UnitActionType type;
	public UnitActionView view;
	public ChangeTracker<bool> selected;
	public Unit unit, targetUnit;
	public MovePath path;
	public int weaponIndex;
	public Vector2Int targetPosition;

	public static Dictionary<UnitActionType, Func<UnitActionView>> prefabs;
	static UnitAction() {
		prefabs = new Dictionary<UnitActionType, Func<UnitActionView>>();
		foreach (UnitActionType type in Enum.GetValues(typeof(UnitActionType)))
			prefabs.Add(type,()=>Resources.Load<UnitActionView>("UnitActionView"));
	}

	public UnitAction(UnitActionType type, Unit unit, MovePath path, Unit targetUnit = null, Building targetBuilding = null, int weaponIndex = -1, Vector2Int targetPosition = default) {

		this.type = type;
		this.unit = unit;
		this.path = path;
		this.targetUnit = targetUnit;
		this.weaponIndex = weaponIndex;
		this.targetPosition = targetPosition;

		Assert.IsTrue(prefabs.ContainsKey(type));
		var prefab = prefabs[type];
		view = Object.Instantiate(prefab());
		Object.DontDestroyOnLoad(view);
		view.action = this;

		selected = new ChangeTracker<bool>(_ => view.selected.v = selected.v);
	}

	public void Dispose() {
		if (view && view.gameObject) {
			Object.Destroy(view.gameObject);
			view = null;
		}
	}

	public IEnumerator  Execute() {

		Assert.IsTrue(path.Count >= 1);

		unit.player.game.TryGetUnit(path.Destination, out var unitAtPathEnd);
		unit.player.game.TryGetBuilding(path.Destination, out var buildingAtPathEnd);

		unit.moved.v = true;
		
		Debug.Log($"EXECUTING: {unit} {type} {targetUnit}");

		switch (type) {

			case UnitActionType.Stay: {
				unit.position.v = path.Destination;
				break;
			}

			case UnitActionType.Join: {
				unitAtPathEnd.hp.v = Mathf.Min(Rules.MaxHp(unitAtPathEnd), unitAtPathEnd.hp.v + unit.hp.v);
				unit.Dispose();
				unit = null;
				break;
			}

			case UnitActionType.Capture: {
				unit.position.v = path.Destination;
				buildingAtPathEnd.cp.v -= Rules.Cp(unit);
				if (buildingAtPathEnd.cp.v <= 0) {
					buildingAtPathEnd.player.v = unit.player;
					buildingAtPathEnd.cp.v = Rules.MaxCp(buildingAtPathEnd);
				}
				break;
			}

			case UnitActionType.Attack:
				return BattleAnimation.New(this);

			case UnitActionType.GetIn: {
				unit.position.v = null;
				unit.carrier.v = unitAtPathEnd;
				unitAtPathEnd.cargo.Add(unit);
				break;
			}

			case UnitActionType.Drop: {
				unit.position.v = path.Destination;
				unit.cargo.Remove(targetUnit);
				targetUnit.position.v = targetPosition;
				targetUnit.carrier.v = null;
				break;
			}

			case UnitActionType.Supply: {
				unit.position.v = path.Destination;
				targetUnit.fuel.v = Rules.MaxFuel(targetUnit);
				foreach (var weaponIndex in Rules.Weapons(targetUnit))
					targetUnit.ammo[weaponIndex] = Rules.MaxAmmo(targetUnit, weaponIndex);
				break;
			}

			default:
				throw new ArgumentOutOfRangeException();
		}

		return null;
	}

	public override string ToString() {
		var text = type.ToString();
		text += $" - {path[0]} -> {path.Destination}";
		if (targetUnit != null)
			text += $" - {targetUnit}";
		return text;
	}
}