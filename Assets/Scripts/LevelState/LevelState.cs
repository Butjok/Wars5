using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class LevelState : State {

	public Level level;
	protected LevelState(Level level) : base(level) {
		Assert.IsNotNull(level);
		this.level = level;
	}


	public Map2D<TileType> Tiles => level.tiles;
	public Map2D<Unit> Units => level.units;
	public List<Player> Players => level.players;
	public int Turn {
		get {
			Assert.IsTrue(level.Turn != null, "level.Turn != null");
			return (int)level.Turn;
		}
	}
}

public class VictoryState : State2<Game2> {
	public VictoryState(Game2 parent) : base(parent) {
	}
	public override void Start() {
		Debug.Log("Victory!");
	}
}

public class DefeatState : State2<Game2> {

	public DefeatState(Game2 parent) : base(parent) {
	}
	public override void Start() {
		Debug.Log("Defeat...");
	}
}