using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public class Player : IDisposable {

	public static readonly HashSet<Player> undisposed = new();

	public readonly Main main;
	public readonly Team team ;
	public readonly string name;
	
	private Color color;
	public Color Color {
		get => color;
		set {
			color = value;
			if (initialized) {
				foreach (var unit in main.FindUnitsOf(this))
					RecursivelyUpdateUnitColor(unit);
				foreach (var building in main.FindBuildingsOf(this))
					building.view.PlayerColor = Color;
			}
		}
	}
	public void RecursivelyUpdateUnitColor(Unit unit) {
		if (unit.Player == this && unit.view)
			unit.view.PlayerColor = Color;
		foreach (var cargo in unit.Cargo)
			RecursivelyUpdateUnitColor(cargo);
	}
	
	public readonly Co co;
	public readonly PlayerType type;
	public readonly AiDifficulty difficulty;
	public readonly PlayerView view;

	public int maxCredits = Rules.defaultMaxCredits;
	
	private int credits;
	public int Credits {
		get => credits;
		set => credits = Clamp(value, 0, initialized ? MaxCredits(this) : defaultMaxCredits);
	}

	private int abilityMeter;
	public int AbilityMeter {
		get => abilityMeter;
		set => abilityMeter = Clamp(value, 0, initialized ? defaultMaxAbilityMeter : MaxAbilityMeter(this));
	}
	public int? abilityActivationTurn;
	public Vector2Int unitLookDirection = Vector2Int.up;

	private bool initialized;
	
	public Player(Main main, Color color, Team team = Team.None, int credits=0, Co co = null, PlayerView viewPrefab = null,
		PlayerType type = PlayerType.Human, AiDifficulty difficulty=AiDifficulty.Normal, Vector2Int? unitLookDirection=null, string name=null) {

		undisposed.Add(this);
		
		this.main = main;
		Color = color;
		this.team = team;
		Credits = credits;
		this.co = co ? co : Co.Natalie;
		this.type = type;
		this.difficulty = difficulty;
		this.unitLookDirection = unitLookDirection ?? Vector2Int.up;
		
		main.players.Add(this);
		
		viewPrefab = viewPrefab ? viewPrefab : PlayerView.DefaultPrefab;
		Assert.IsTrue(viewPrefab);
		view = Object.Instantiate(viewPrefab, main.transform);
		view.Initialize(this);
		view.visible = false;

		this.name = name;

		initialized = true;
	}

	public override string ToString() {
		return name??Color.ToString();
	}

	public void Dispose() {
		Assert.IsTrue(undisposed.Contains(this));
		undisposed.Remove(this);
		if (view && view.gameObject)
			Object.Destroy(view.gameObject);
	}

	public bool IsAi => type == PlayerType.Ai;

	public UnitAction FindAction() {
		return null;
	}
}