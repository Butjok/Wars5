using System;
using System.Collections.Generic;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class UnitBrain {

    public readonly Unit unit;
    public Zone assignedZone;
    public Stack<UnitBrainState> states = new();
    public UnitBrain(Unit unit) {
        this.unit = unit;
    }
}

public abstract class UnitBrainState {

    [Command] public static Color zoneAlpha = new(1, 1, 1, .25f);
    [Command] public static Color lineAlpha = new(1, 1, 1, .75f);

    protected UnitBrain brain;
    public readonly int turn;
    protected UnitBrainState(UnitBrain brain) {
        this.brain = brain;
        turn = brain.unit.Player.level.turn;
    }
    public virtual bool TryAssignZone(Zone zone) {
        brain.assignedZone = zone;
        return true;
    }
    public virtual bool TrySuggestAction(out float score, out Action action) {
        (score, action) = (0, null);
        return false;
    }
    public virtual void OnTurnEnd() { }
    public virtual void OnGUI() { }

    public override string ToString() {
        var name = GetType().Name;
        if (name.EndsWith("UnitBrainState"))
            name = name[..^"UnitBrainState".Length];
        return name;
    }
}

public class StayingInZoneUnitBrainState : UnitBrainState {

    [Command] public static Color color = Color.blue;

    public StayingInZoneUnitBrainState(UnitBrain brain) : base(brain) { }
    public override void OnGUI() {
        if (brain.assignedZone != null) {
            Draw.ingame.Line(brain.unit.view.body.position, brain.assignedZone.GetCenter(), color * lineAlpha);
            foreach (var position in brain.assignedZone.tiles)
                Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, color * zoneAlpha);
        }
    }
}

public class ReturningToZoneUnitBrainState : UnitBrainState {

    [Command] public static Color color = Color.yellow;

    public ReturningToZoneUnitBrainState(UnitBrain unit) : base(unit) { }
    public override void OnGUI() {
        if (brain.assignedZone != null) {
            Draw.ingame.Line(brain.unit.view.body.position, brain.assignedZone.GetCenter(), color * lineAlpha);
            foreach (var position in brain.assignedZone.tiles)
                Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, color * zoneAlpha);
        }
    }
}

public class AttackingAnEnemyUnitBrainState : UnitBrainState {

    [Command] public static Color color = Color.red;

    public Unit target;
    public AttackingAnEnemyUnitBrainState(UnitBrain unit) : base(unit) { }
    public override bool TryAssignZone(Zone zone) {
        return false;
    }
    public override void OnGUI() {
        if (target != null)
            Draw.ingame.Line(brain.unit.view.body.position, target.view.body.position, color * lineAlpha);
    }
}