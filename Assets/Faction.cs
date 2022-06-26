using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(Faction))]
public class Faction : ScriptableObject {
	[SerializeField] private UnitTypeUnitViewDictionary unitPrefabs = new();
	public UnitView GetUnitViewPrefab(UnitType type) {
		return unitPrefabs.TryGetValue(type, out var prefab) ? prefab : WarsResources.test.v;
	}
}

[Serializable]
public class UnitTypeUnitViewDictionary : SerializableDictionary<UnitType, UnitView> { }