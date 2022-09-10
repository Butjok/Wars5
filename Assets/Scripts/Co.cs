using UnityEngine;

[CreateAssetMenu(menuName = nameof(Co))]
public class Co : ScriptableObject {

	public Faction faction;
	public PlayerView viewPrefab;
	public Sprite portrait;

	[SerializeField] private UnitTypeUnitViewDictionary unitPrefabsOverrides = new();
	public UnitView GetUnitViewPrefab(UnitType type) {
		return unitPrefabsOverrides.TryGetValue(type, out var prefab)&&prefab ? prefab : faction.GetUnitViewPrefab(type);
	}
}