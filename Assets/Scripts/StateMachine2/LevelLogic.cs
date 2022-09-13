using UnityEngine.Assertions;

public abstract class LevelLogic {

	public Game2 game;

	protected LevelLogic(Game2 game) {
		Assert.IsTrue(game);
		this.game = game;
	}

	public virtual bool OnTurnStart() {
		return false;
	}
	public virtual bool OnTurnEnd() {
		return false;
	}
	public virtual bool OnActionCompletion(UnitAction action) {
		return false;
	}
	public virtual bool OnVictory() {
		return false;
	}
	public virtual bool OnDefeat() {
		return false;
	}
}