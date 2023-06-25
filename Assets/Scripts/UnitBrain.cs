using System;
using System.Collections.Generic;

public class Zone { }

public class UnitBrain {

    public readonly Unit unit;
    private readonly Stack<UnitBrainState> states = new();
    public UnitBrain(Unit unit) {
        this.unit = unit;
    }
    public bool TryGetState(out UnitBrainState brainState) {
        return states.TryPeek(out brainState);
    }
}

public abstract class UnitBrainState {
        
    protected UnitBrain brain;
    public readonly int turn;
    public Zone Zone { get; private set; }
    protected UnitBrainState(UnitBrain brain) {
        this.brain = brain;
        turn = brain.unit.Player.level.turn;
    }
    public virtual bool TryAcceptZoneAssignment(Zone zone) {
        Zone = zone;
        return true;
    }
    public virtual bool TrySuggestAction(out float score, out Action action) {
        (score, action) = (0, null);
        return false;
    }
    public virtual void OnTurnEnd() {}
}

public class StayingInZoneUnitBrainState : UnitBrainState {
    public StayingInZoneUnitBrainState(UnitBrain brain) : base(brain) { }
}

public class ReturningToZoneUnitBrainState : UnitBrainState {
    public ReturningToZoneUnitBrainState(UnitBrain unit) : base(unit) { }
}

public class AttackingAnEnemyUnitBrainState : UnitBrainState {
    public AttackingAnEnemyUnitBrainState(UnitBrain unit) : base(unit) { }
    public override bool TryAcceptZoneAssignment(Zone zone) {
        return false;
    }
}