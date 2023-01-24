using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(Co))]
public class Co : ScriptableObject {

    public static string[] names = { nameof(Natalie), nameof(Vladan) };

    public static Co Natalie => nameof(Natalie).LoadAs<Co>();
    public static Co Vladan => nameof(Vladan).LoadAs<Co>();
    
    public static bool TryGet(string name, out Co co) {
        co = null;
        switch (name) {
            case nameof(Natalie):
                return Natalie;
            case nameof(Vladan):
                return Vladan;
        }
        return co != null;
    }
    
    public Faction faction;
    public PlayerView viewPrefab;
    public Sprite portrait;
    public AudioClip[] themes = { };

    public UnitTypeInfoDictionary unitTypesInfo;
    public UnitTypeInfoDictionary unitTypesInfoOverride = new();

    [SerializeField] private UnitTypeUnitViewDictionary unitPrefabsOverrides = new();
    public UnitView GetUnitViewPrefab(UnitType type) {
        return unitPrefabsOverrides.TryGetValue(type, out var prefab) && prefab ? prefab : faction.GetUnitViewPrefab(type);
    }
}
