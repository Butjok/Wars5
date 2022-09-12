using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public enum UnitActionType { Stay, Join, Capture, Attack, GetIn, DropOut, Supply }
public class UnitAction : IDisposable {

	public UnitActionType type;
	public UnitActionView view;
	public ChangeTracker<bool> selected;
	public Unit unit, targetUnit;
	public MovePath path;
	public int weaponIndex;
	public Vector2Int targetPosition;

	public static Dictionary<UnitActionType, Lazy<UnitActionView>> prefabs;
	static UnitAction() {
		prefabs = new Dictionary<UnitActionType, Lazy<UnitActionView>>();
		foreach (UnitActionType type in Enum.GetValues(typeof(UnitActionType)))
			prefabs.Add(type, Lazy.Resource<UnitActionView>("UnitActionView"));
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
		view = Object.Instantiate(prefab.v);
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

	public void Execute() {

		Assert.IsTrue(path.positions.Count >= 1);
		var pathEnd = path.positions[^1];

		var level = unit.game;
		var units = level.units;

		level.TryGetUnit(pathEnd, out var unitAtPathEnd);
		level.TryGetBuilding(pathEnd, out var buildingAtPathEnd);

		Debug.Log($"EXECUTING: {unit} {type} {targetUnit}");

		void moveUnit() {
			if (unit.position.v is not { } position || position != pathEnd) {
				Assert.IsTrue(unitAtPathEnd == null || unitAtPathEnd == unit);
				unit.position.v = pathEnd;
			}
		}
		void destroyUnit() {
			unit.Dispose();
			unit = null;
		}

		switch (type) {

			case UnitActionType.Stay: {
				unit.position.v = pathEnd;
				break;
			}

			case UnitActionType.Join: {
				unitAtPathEnd.hp.v = Mathf.Min(Rules.MaxHp(unitAtPathEnd), unitAtPathEnd.hp.v + unit.hp.v);
				unit.Dispose();
				unit = null;
				break;
			}

			case UnitActionType.Capture: {
				unit.position.v = pathEnd;
				buildingAtPathEnd.cp.v -= Rules.Cp(unit);
				if (buildingAtPathEnd.cp.v <= 0) {
					buildingAtPathEnd.player.v = unit.player;
					buildingAtPathEnd.cp.v = Rules.MaxCp(buildingAtPathEnd);
				}
				break;
			}

			case UnitActionType.Attack: {
				unit.position.v = pathEnd;
				targetUnit.hp.v = Mathf.Max(0, targetUnit.hp.v - (int)Rules.Damage(unit, targetUnit, weaponIndex));
				if (targetUnit.hp.v > 0) {
					unit.hp.v = Mathf.Max(0, unit.hp.v - (int)Rules.Damage(targetUnit, unit, weaponIndex));
				}
				else
					targetUnit.Dispose();
				break;
			}

			case UnitActionType.GetIn: {
				unit.position.v = null;
				unit.carrier.v = unitAtPathEnd;
				unitAtPathEnd.cargo.Add(unit);
				break;
			}

			case UnitActionType.DropOut: {
				unit.position.v = pathEnd;
				unit.cargo.Remove(targetUnit);
				targetUnit.position.v = targetPosition;
				targetUnit.carrier.v = null;
				break;
			}

			case UnitActionType.Supply: {
				unit.position.v = pathEnd;
				targetUnit.fuel.v = Rules.MaxFuel(targetUnit);
				foreach (var weaponIndex in Rules.Weapons(targetUnit))
					targetUnit.ammo[weaponIndex] = Rules.MaxAmmo(targetUnit, weaponIndex);
				break;
			}

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public override string ToString() {
		var text = type.ToString();
		text += $" - {path.positions.First()} -> {path.positions.Last()}";
		if (targetUnit != null)
			text += $" - {targetUnit}";
		return text;
	}
}