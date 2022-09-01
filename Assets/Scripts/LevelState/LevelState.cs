using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class LevelState : State {

	public Level level;
	protected LevelState( Level level) : base(level) {
		Assert.IsNotNull(level);
		this.level = level;
	}

	public Player CurrentPlayer {
		get {
			Assert.AreNotEqual(0, Players.Count);
			return level.players[Turn % Players.Count];
		}
	}
	public Map2D< TileType> Tiles => level.tiles;
	public Map2D< Unit> Units => level.units;
	public List<Player> Players => level.players;
	public int Turn {
		get {
			Assert.AreNotEqual(null, level.turn);
			return (int)level.turn;
		}
	}
}