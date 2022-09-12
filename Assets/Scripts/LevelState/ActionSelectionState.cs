using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSelectionState : State2<Game2> {

    public Unit unit;
    public MovePath path;
    public Vector2Int startForward;
    public List<UnitAction> actions = new();
    public int index = -1;
    public UnitAction action;

    public ActionSelectionState(Game2 parent, Unit unit, Vector2Int startForward, MovePath path) : base(parent) {

        this.unit = unit;
        this.startForward = startForward;
        this.path = path;

        var position = path.positions.Last();
        parent.TryGetUnit(position, out var other);
        
        // stay/capture
        if (other == null || other == unit) {
            if (parent.TryGetBuilding(position, out var building) && Rules.CanCapture(unit, building))
                actions.Add(new UnitAction(UnitActionType.Capture, unit, path, null, building));
            else
                actions.Add(new UnitAction(UnitActionType.Stay, unit, path));
        }
        
        // join
        if (other != null && Rules.CanJoin(unit,other))
            actions.Add(new UnitAction(UnitActionType.Join, unit, path, unit));
        
        // load in
        if (other != null && Rules.CanLoadAsCargo(other, unit))
            actions.Add(new UnitAction(UnitActionType.GetIn, unit, path, other));
        
        // attack
        if (!Rules.IsArtillery(unit) || path.positions.Count == 1)
            foreach (var otherPosition in parent.AttackPositions(position, Rules.AttackRange(unit)))
                if (parent.TryGetUnit(otherPosition, out other))
                    for (var weapon = 0; weapon < Rules.WeaponsCount(unit); weapon++)
                        if (Rules.CanAttack(unit, other, weapon))
                            actions.Add(new UnitAction(UnitActionType.Attack, unit, path, other, null, weapon));
        
        // supply
        foreach (var offset in Rules.offsets)
            if (parent.TryGetUnit(position + offset, out other) && Rules.CanSupply(unit, other))
                actions.Add(new UnitAction(UnitActionType.Supply, unit, path, other));
    }

    public override void Dispose() {
        base.Dispose();
        foreach (var action in actions)
            action.Dispose();
    }

    public override void Update() {
        base.Update();

        if (parent.CurrentPlayer.IsAi)
            Execute(parent.CurrentPlayer.action);

        else {
            if (Input.GetMouseButtonDown(Mouse.right)) {
                unit.view.Position = path.positions[0];
                unit.view.Forward = startForward;
                ChangeTo(new PathSelectionState(parent, unit));
            }

            else if (Input.GetKeyDown(KeyCode.Tab)) {
                if (actions.Count > 0) {
                    index = (index + 1) % actions.Count;
                    action = actions[index];
                    Debug.Log(action);
                }
                else
                    UiSound.Instance.notAllowed.Play();
            }

            else if (Input.GetKeyDown(KeyCode.Return)) {
                if (action != null)
                    Execute(action);
                else
                    UiSound.Instance.notAllowed.Play();
            }
        }

        /*if (unit.view.turret) {

            if (Mouse.TryGetPosition(out var position) &&
                level.TryGetUnit(position.RoundToInt(), out var target) &&	
                 Rules.CanAttack(unit,target)) {

                unit.view.turret.ballisticComputer.target = target.view.center;
                unit.view.turret.aim = true;
            }
            else
                unit.view.turret.aim = false;
        }*/
    }

    // TODO: fix this, move somewhere
    
    public void Execute(UnitAction action) {
        action.Execute();
        unit.moved.v = true;
        if (Rules.Won(parent.realPlayer))
            ChangeTo(new VictoryState(parent));
        else if (Rules.Lost(parent.realPlayer))
            ChangeTo(new DefeatState(parent));
        else if(!parent.levelLogic.OnActionCompletion(action))
            ChangeTo(new SelectionState(parent));
    }
}