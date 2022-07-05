public abstract class LevelScript {

	public Level level;

	protected LevelScript(Level level) {
		this.level = level;
	}

	public virtual void OnTurnStart(int day, Player player) { }
	public virtual void OnTurnEnd(int day, Player player) { }

	public virtual void OnActionExecuted(UnitAction action) { }
}