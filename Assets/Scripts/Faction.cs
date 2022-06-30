using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(Faction))]
public class Faction : ScriptableObject {
	[SerializeField] private UnitTypeUnitViewDictionary unitPrefabs = new();
	public UnitView GetUnitViewPrefab(UnitType type) {
		return unitPrefabs.TryGetValue(type, out var prefab)&&prefab ? prefab : WarsResources.test.v;
	}
}

public class Factions {

	public const string Novoslavia = nameof(Novoslavia);
	public const string UnitedTreaty = nameof(UnitedTreaty);
	public static string[] names = { Novoslavia, UnitedTreaty };

	public static Lazy<Faction> novoslavia = new(() => Resources.Load<Faction>(Novoslavia));
	public static Lazy<Faction> unitedTreaty = new(() => Resources.Load<Faction>(UnitedTreaty));

	private static Dictionary<string, Lazy<Faction>> get = new() {
		[Novoslavia] = novoslavia,
		[UnitedTreaty] = unitedTreaty,
	};
}

[Serializable]
public class UnitTypeUnitViewDictionary : SerializableDictionary<UnitType, UnitView> { }