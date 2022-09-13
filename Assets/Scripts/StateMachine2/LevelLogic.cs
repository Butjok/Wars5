using UnityEngine.Assertions;

public abstract class LevelLogic {

	public Game2 game;

	protected LevelLogic(Game2 game) {
		Assert.IsTrue(game);
		this.game = game;
	}

	public virtual void OnTurnStart() { }
	public virtual void OnTurnEnd() { }
	public virtual void OnActionCompletion(UnitAction action) { }
	public virtual void OnVictory() {}
	public virtual void OnDefeat() { }
}
