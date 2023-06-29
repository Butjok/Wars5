using System.Collections.Generic;

public class MainMenuGameSettingsState : StateMachineState {

    public MainMenuGameSettingsState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            yield break;
        }
    }
}