using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLogic {

    
    protected IEnumerator ExecuteAction(Action action) {
        action();
        yield return null;
    }

    public virtual StateChange OnTurnEnd(Level level) {
        return StateChange.none;
    }

    public virtual StateChange OnActionCompletion(Level level, UnitAction action) {
        action.Dispose();
        return StateChange.none;
    }

    public virtual StateChange OnVictory(Level level, UnitAction winningAction) {
        return StateChange.none;
    }

    public virtual StateChange OnDefeat(Level level, UnitAction defeatingAction) {
        return StateChange.none;
    }
}