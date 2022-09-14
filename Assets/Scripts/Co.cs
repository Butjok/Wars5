using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(Co))]
public class Co : ScriptableObject {

    public static Co Natalie => "NatalieCo".LoadAs<Co>();
    public static Co Vladan => "VladanCo".LoadAs<Co>();

    public Faction faction;
    public PlayerView viewPrefab;
    public Sprite portrait;
    public AudioClip[] themes = { };

    [SerializeField] private UnitTypeUnitViewDictionary unitPrefabsOverrides = new();
    public UnitView GetUnitViewPrefab(UnitType type) {
        return unitPrefabsOverrides.TryGetValue(type, out var prefab) && prefab ? prefab : faction.GetUnitViewPrefab(type);
    }
}
