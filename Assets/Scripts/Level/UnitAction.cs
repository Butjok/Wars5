using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public enum UnitActionType {
    Stay,
    Join, 
    Capture, 
    Attack, 
    GetIn,
    Drop,
    Supply, 
    LaunchMissile,  
    PickUpCrate,
    Gather,
    TravelThroughTunnel,
    TakeMissile,
    LoadMissileSilo,
    AttackPipeSection
}

public class UnitAction {

    public UnitActionType type;
    public Unit unit, targetUnit;
    public List<Vector2Int> path;
    public WeaponName weaponName;
    public Vector2Int targetPosition;
    public Building targetBuilding;
    public Crate targetCrate;
    public TunnelEntrance targetTunnelEntrance;
    public PipeSection targetPipeSection;

    public UnitAction(UnitActionType type, Unit unit, IEnumerable<Vector2Int> path, Unit targetUnit = null, Building targetBuilding = null,
        WeaponName weaponName = default, Vector2Int targetPosition = default, Crate targetCrate = null,  TunnelEntrance targetTunnelEntrance = null,
        PipeSection targetPipeSection = null) {
        this.type = type;
        this.unit = unit;

        this.targetUnit = targetUnit;
        this.weaponName = weaponName;
        this.targetPosition = targetPosition;
        this.targetBuilding = targetBuilding;
        this.targetCrate = targetCrate;
        this.targetTunnelEntrance = targetTunnelEntrance;
        this.targetPipeSection = targetPipeSection;

        this.path = path.ToList();
        Assert.AreNotEqual(0, this.path.Count);
        Assert.AreEqual(unit.Position, this.path[0]);
    }

    public override string ToString() {
        var text = type.ToString();
        if (targetUnit != null)
            text += $" {targetUnit}";
        if (type == UnitActionType.Drop)
            text += $" to {targetPosition}";
        if (type == UnitActionType.Attack)
            text += $" with {weaponName}";
        return text;
    }
}