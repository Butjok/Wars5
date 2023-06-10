using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelSessionState : StateMachineState {

    public Level level = new();
    public string input;
    public LevelSessionState(StateMachine stateMachine, string input = "") : base(stateMachine) {
        this.input = input;
    }

    public override IEnumerator<StateChange> Sequence {
        get {
            LevelView.TryLoadScene(level.missionName);
            level.view = LevelView.TryInstantiate();
            Assert.IsTrue(level.view);
            LevelReader.ReadInto(level, input);
            
            yield return StateChange.Push(new PlayerTurnState(stateMachine));
        }
    }

    public override void Dispose() {
        LevelView.TryUnloadScene(level.missionName);
        Object.Destroy(level.view.gameObject);
        level.view = null;
    }
}