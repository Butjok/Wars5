using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerTurnState : StateMachineState {

    public PlayerTurnState(StateMachine stateMachine) : base(stateMachine) { }

    public Player player;
    
    public override IEnumerator<StateChange> Sequence {
        get {
            var level = stateMachine.TryFind<LevelSessionState>()?.level;
            Assert.IsNotNull(level);
            player = level.CurrentPlayer;
            player.view.visible = true;
            Debug.Log($"Start of turn #{level.turn}: {player}");
            
            // if(level.turn==0)
                // yield return StateChange.Push(new TestDialogueState(stateMachine));
            
            yield return StateChange.Push(new SelectionState(stateMachine));
        }
    }
    public override void Dispose() {
        player.view.visible = false;
        Debug.Log($"End of turn");
    }
}

public class DayChangeState : StateMachineState {
    public DayChangeState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Sequence {
        get {
            yield break;
        }
    }
}