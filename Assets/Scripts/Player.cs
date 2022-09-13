using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

[Flags]
public enum Team { None = 0, Alpha = 1, Bravo = 2, Delta = 4 }
[Flags]
public enum PlayerType { Human, Ai }
public enum AiDifficulty { Normal, Easy, Hard }

public class Player : IDisposable {

	public Game2 game;
	public Team team = Team.None;
	public Color32 color;
	public Co co;
	public PlayerType type = PlayerType.Human;
	public AiDifficulty difficulty = AiDifficulty.Normal;
	public PlayerView view;

	public UnitAction bestAction;

	public Player(Game2 game, Color32 color, Team team = Team.None, PlayerView viewPrefab = null) {
		this.game = game;
		this.color = color;
		this.team = team;
		game.players.Add(this);
		
		viewPrefab = viewPrefab ? viewPrefab : Resources.Load<PlayerView>(nameof(PlayerView));
		Assert.IsTrue(viewPrefab);
		view = Object.Instantiate(viewPrefab);
		Object.DontDestroyOnLoad(view.gameObject);
		view.Initialize(this);
	}

	public override string ToString() {
		return color.Name();
	}

	public void Dispose() {
		if (view && view.gameObject)
			Object.Destroy(view.gameObject);
	}

	public bool IsAi => (type & PlayerType.Ai) != 0;

	public UnitAction FindAction() {
		return null;
	}
}