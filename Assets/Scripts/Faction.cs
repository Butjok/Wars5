using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = nameof(Faction))]
public class Faction : ScriptableObject {

    public const string Novoslavia = nameof(Novoslavia);
    public const string UnitedTreaty = nameof(UnitedTreaty);

    public static bool TryGet(string name, out Faction faction) {
        faction = null;
        switch (name) {
            case Novoslavia:
                faction = Novoslavia.LoadAs<Faction>();
                break;
            case UnitedTreaty:
                faction = UnitedTreaty.LoadAs<Faction>();
                break;
        }
        return faction != null;
    }

    [SerializeField] private UnitTypeUnitViewDictionary unitPrefabs = new();
    public UnitView GetUnitViewPrefab(UnitType type) {
        var prefab = unitPrefabs.TryGetValue(type, out var p) && p ? p : UnitView.DefaultPrefab;
        Assert.IsTrue(prefab);
        return prefab;
    }
}

[Serializable]
public class UnitTypeUnitViewDictionary : SerializableDictionary<UnitType, UnitView> { }