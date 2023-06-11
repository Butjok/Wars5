using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryState : StateMachineState {
    
    public VictoryState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Entry {
        get {
            using var dialogue = new DialoguePlayer();
            foreach (var stateChange in dialogue.Play(Strings.Victory))
                yield return stateChange;
        }
    }
}

public class DefeatState : StateMachineState {
    public DefeatState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Entry {
        get {
            using var dialogue = new DialoguePlayer();
            foreach (var stateChange in dialogue.Play(Strings.Defeat))
                yield return stateChange;
        }
    }
}