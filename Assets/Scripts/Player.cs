using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

[Flags]
public enum Team { None = 0, Alpha = 1, Bravo = 2, Charlie = 4, Delta = 8 }
[Flags]
public enum PlayerType { Human, Ai }
public enum AiDifficulty { Normal, Easy, Hard }

public class Player : IDisposable {

	public static readonly HashSet<Player> undisposed = new();

	public Game game;
	public Team team = Team.None;
	public Color32 color;
	public Co co;
	public PlayerType type = PlayerType.Human;
	public AiDifficulty difficulty = AiDifficulty.Normal;
	public PlayerView view;
	public int credits;

	public Player(Game game, Color32 color, Team team = Team.None, int credits=0, Co co = null, PlayerView viewPrefab = null,
		PlayerType type = PlayerType.Human, AiDifficulty difficulty=AiDifficulty.Normal) {

		undisposed.Add(this);
		
		this.game = game;
		this.color = color;
		this.team = team;
		this.credits = credits;
		this.co = co ? co : Co.Natalie;
		this.type = type;
		this.difficulty = difficulty;
		
		game.players.Add(this);
		
		viewPrefab = viewPrefab ? viewPrefab : PlayerView.DefaultPrefab;
		Assert.IsTrue(viewPrefab);
		view = Object.Instantiate(viewPrefab);
		Object.DontDestroyOnLoad(view.gameObject);
		view.Initialize(this);
		view.visible = false;
	}

	public override string ToString() {
		return color.Name();
	}

	public void Dispose() {
		Assert.IsTrue(undisposed.Contains(this));
		undisposed.Remove(this);
		if (view && view.gameObject)
			Object.Destroy(view.gameObject);
	}

	public bool IsAi => (type & PlayerType.Ai) != 0;

	public UnitAction FindAction() {
		return null;
	}
}