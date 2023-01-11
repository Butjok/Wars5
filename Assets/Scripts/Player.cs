using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using Object = UnityEngine.Object;



public class Player : IDisposable {

	public static readonly HashSet<Player> undisposed = new();

	public Main main;
	public Team team = Team.None;
	public Color color;
	public Co co;
	public PlayerType type = PlayerType.Human;
	public AiDifficulty difficulty = AiDifficulty.Normal;
	public PlayerView view;
	public int credits;
	public int powerMeter;
	public int? abilityActivationTurn;

	public Player(Main main, Color color, Team team = Team.None, int credits=0, Co co = null, PlayerView viewPrefab = null,
		PlayerType type = PlayerType.Human, AiDifficulty difficulty=AiDifficulty.Normal, Vector2Int? unitLookDirection=null) {

		undisposed.Add(this);
		
		this.main = main;
		this.color = color;
		this.team = team;
		this.credits = credits;
		this.co = co ? co : Co.Natalie;
		this.type = type;
		this.difficulty = difficulty;
		
		main.players.Add(this);
		
		viewPrefab = viewPrefab ? viewPrefab : PlayerView.DefaultPrefab;
		Assert.IsTrue(viewPrefab);
		view = Object.Instantiate(viewPrefab, main.transform);
		view.Initialize(this);
		view.visible = false;
		view.unitLookDirection = unitLookDirection ?? Vector2Int.up;
	}

	public override string ToString() {
		if (color == Color.red)
			return "Red";
		if (color == Color.green)
			return "Green";
		if (color == Color.blue)
			return "Blue";
		return color.ToString();
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