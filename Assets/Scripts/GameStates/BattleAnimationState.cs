using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BattleAnimationState {

    public static IEnumerator<StateChange> Run(UnitAction action, bool skipAnimation = false) {
        yield return StateChange.Pop();
    }
}