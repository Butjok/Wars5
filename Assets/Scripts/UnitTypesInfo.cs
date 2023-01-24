using System;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(UnitTypesInfo))]
public class UnitTypesInfo : ScriptableObject {

    public static UnitTypesInfo Instance => nameof(UnitTypesInfo).LoadAs<UnitTypesInfo>();
    public static bool TryGet(UnitType unitType, out Record record) {
        return Instance.map.TryGetValue(unitType, out record);
    }
    
    [Serializable]
    public struct Record {
        public string name;
        public Sprite thumbnail;
        
        public string description;
        public UnitView viewPrefab;
    }
    
    [SerializeField] private UnitTypeInfoDictionary map = new() {
        [UnitType.Infantry] = new Record {
            name = nameof(UnitType.Infantry).ToWords(),
            thumbnail = null,
            description = ""
        },
        [UnitType.AntiTank] = new Record {
            name = nameof(UnitType.AntiTank).ToWords(),
            thumbnail = null,
            description = ""
        },
        [UnitType.Artillery] = new Record {
            name = nameof(UnitType.Artillery).ToWords(),
            thumbnail = null,
            description = ""
        },
        [UnitType.Apc] = new Record {
            name = nameof(UnitType.Apc).ToWords(),
            thumbnail = null,
            description = ""
        },
        [UnitType.Recon] = new Record {
            name = nameof(UnitType.Recon).ToWords(),
            thumbnail = null,
            description = ""
        },
        [UnitType.LightTank] = new Record {
            name = nameof(UnitType.LightTank).ToWords(),
            thumbnail = null,
            description = ""
        },
    };
}

[Serializable]
public class UnitTypeInfoDictionary : SerializableDictionary<UnitType, UnitTypesInfo.Record> { }