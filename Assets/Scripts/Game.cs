using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class Game : IDisposable {

	public UnitMap unitMap = new();
	public Dictionary<Vector2Int, Tile> tileAt = new();
	public List<Player> playerLoop = new();
	public Dictionary<Color, Player> players = new();
	public int? turn;
	public GameRunner runner;

	public ChangeTracker<GameState> state = new(old => old?.Dispose());

	public Game(string name = null) {
		var go = new GameObject(name ?? nameof(Game));
		Object.DontDestroyOnLoad(go);
		runner = go.AddComponent<GameRunner>();
		runner.game = this;
	}
	public void Dispose() {
		state.v?.Dispose();
		if (runner && runner.gameObject)
			Object.Destroy(runner.gameObject);
	}
}

public abstract class GameState : IDisposable {

	public Game game;
	protected GameState(Game game) {
		Assert.IsNotNull(game);
		this.game = game;
	}

	public Player CurrentPlayer {
		get {
			Assert.AreNotEqual(0, PlayerLoop.Count);
			return game.playerLoop[Turn % PlayerLoop.Count];
		}
	}
	public Dictionary<Vector2Int, Tile> TileAt => game.tileAt;
	public UnitMap UnitMap => game.unitMap;
	public List<Player> PlayerLoop => game.playerLoop;
	public int Turn {
		get {
			Assert.AreNotEqual(null, game.turn);
			return (int)game.turn;
		}
	}

	public virtual void Update() { }
	public virtual void DrawGUI() { }
	public virtual void Dispose() { }
	public virtual void DrawGizmos() { }
	public virtual void DrawGizmosSelected() { }
}