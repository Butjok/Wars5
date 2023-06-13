public  class LevelLogic {

    public virtual StateChange OnLevelStart(LevelSessionState state) {
        return StateChange.none;
    }
    
    public virtual StateChange OnLevelEnd(LevelSessionState state) {
        return StateChange.none;
    }
    
    public virtual StateChange OnTurnStart(PlayerTurnState state) {
        return StateChange.none;
    }
    
    public virtual StateChange OnTurnEnd(PlayerTurnState state) {
        return StateChange.none;
    }

    public virtual StateChange OnActionCompletion(ActionSelectionState state) {
        return StateChange.none;
    }

    public virtual StateChange OnVictory(StateMachineState state) {
        return StateChange.none;
    }

    public virtual StateChange OnDefeat(StateMachineState state) {
        return StateChange.none;
    }
}

public class TutorialLevelLogic : LevelLogic {

    public override StateChange OnLevelStart(LevelSessionState state) {
        return state.isFreshStart ? StateChange.Push(new TestDialogueState(state.stateMachine)) : StateChange.none;
    }
}