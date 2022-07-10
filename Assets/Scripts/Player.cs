using System;
using UnityEngine;

[Flags]
public enum Team { None = 0, Alpha = 1, Bravo = 2, Delta = 4 }
public enum PlayerType { Human, Ai }
public enum AiDifficulty { Normal, Easy, Hard }

public class Player {

	public Level level;
	public Team team = Team.None;
	public Color32 color;
	public Co co;
	public PlayerType type = PlayerType.Human;
	public AiDifficulty difficulty = AiDifficulty.Normal;

	public Player(Level level, Color32 color, Team team = Team.None) {
		this.level = level;
		this.color = color;
		this.team = team;
		level.players.Add(this);
	}

	public override string ToString() {
		return Palette.ToString(color);
	}
}