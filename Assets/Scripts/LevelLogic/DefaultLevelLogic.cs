using System.Collections;

public enum ControlFlow { Ignore, Replace, Pause }

public class DefaultLevelLogic {

    public virtual (ControlFlow controlFlow, IEnumerator state) OnTurnStart(Game2 game) {
        return (ControlFlow.Ignore, null);
    }

    public virtual (ControlFlow controlFlow, IEnumerator state) OnTurnEnd(Game2 game) {
        return (ControlFlow.Ignore, null);
    }

    public virtual (ControlFlow controlFlow, IEnumerator state) OnActionCompletion(Game2 game, UnitAction action) {
        return (ControlFlow.Ignore, null);
    }

    public virtual IEnumerator OnVictory(Game2 game) {
        return null;
    }

    public virtual IEnumerator OnDefeat(Game2 game) {
        return null;
    }
}