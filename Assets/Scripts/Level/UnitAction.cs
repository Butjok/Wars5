using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public enum UnitActionType { Stay, Join,Capture, Attack, GetIn, GetOut, Supply }
public class UnitAction : IDisposable {
	
	public UnitActionType type;
	public UnitActionView view;
	public ChangeTracker<bool> selected;
	public Level level;
	public Unit unit, unitTarget;
	public Building buildingTarget;
	public List<Vector2Int> path;
	public int weapon;

	public static Dictionary<UnitActionType, Lazy<UnitActionView>> prefabs;
	static UnitAction() {
		prefabs = new Dictionary<UnitActionType, Lazy<UnitActionView>>();
		foreach (UnitActionType type in Enum.GetValues(typeof(UnitActionType)))
			prefabs.Add(type, Lazy.Resource<UnitActionView>("UnitActionView"));
	}

	public UnitAction(UnitActionType type,Level level,Unit unit,List<Vector2Int>path, Unit unitTarget=null, Building buildingTarget=null,int weapon=-1) {
		
		this.type = type;
		this.level = level;
		this.unit = unit;
		this.path = path;
		this.unitTarget = unitTarget;
		this.buildingTarget = buildingTarget;
		this.weapon = weapon;
		
		Assert.IsTrue(prefabs.ContainsKey(type));
		var prefab = prefabs[type];
		view = Object.Instantiate(prefab.v);
		Object.DontDestroyOnLoad(view);
		view.action = this;
		
		selected = new ChangeTracker<bool>(_ => view.selected.v = selected.v);
	}
	
	public void Dispose() {
		if (view&&view.gameObject) {
			Object.Destroy(view.gameObject);
			view = null;
		}
	}

	public void Execute() {
		Debug.Log($"EXECUTING: {unit} {type} {unitTarget}");
	}

	public override string ToString() {
		var text = type.ToString();
		if (unitTarget != null)
			text += $"; {unitTarget}";
		if (buildingTarget != null)
			text += $"; {buildingTarget}";
		return text;
	}
}