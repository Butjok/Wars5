using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class NewBehaviourScript : MonoBehaviour {

	public Unit unit;
	public Level level;

	private void OnEnable() {
		
		var level = new Level();
		level.turn = 0;
		level.script = new Tutorial(level);
		
		var game = new Game();
		game.State = level;

		///var test = Resources.Load<UnitView>("Test");

		var player = new Player(level, Palette.red, Team.Alpha);
		var player2 = new Player(level, Palette.green, Team.Bravo);
		unit = new Unit(level, player, position: new Vector2Int(1, 1), viewPrefab: Resources.Load<UnitView>("rockets"));
		unit = new Unit(level, player2, position: new Vector2Int(2, 2), viewPrefab: Resources.Load<UnitView>("rockets"));
		unit = new Unit(level, player2, position: new Vector2Int(2, 1), viewPrefab: Resources.Load<UnitView>("rockets"));
		//var a_ = unit.view;

		for (var y = 0; y < 10; y++)
		for (var x = 0; x < 10; x++)
			level.tiles.Add(new Vector2Int(x, y), TileType.Plain);

		level.State = new SelectionState(level);
	}
	private void OnDisable() {
		unit?.Dispose();
		level?.Dispose();
	}


	// Start is ca
	// lled before the first frame update
	void Start() {
		//	var items = PlayerSettings.GetPreloadedAssets();
	}

	// Update is called once per frame
	void Update() { }
}

public class Game : StateMachine {
	public Game() : base(typeof(GameRunner), nameof(Game)) { }
}


public static class WarsResources {
	public static Lazy<UnitView> test = new(() => Resources.Load<UnitView>("Test"));
}


public static class Cos {

	public const string Natalie = nameof(Natalie);
	public const string Vladan = nameof(Vladan);
	public static string[] names = { Natalie, Vladan };

	public static Lazy<Co> natalie = new(() => Resources.Load<Co>(Natalie));
	public static Lazy<Co> vladan = new(() => Resources.Load<Co>(Vladan));

	private static Dictionary<string, Lazy<Co>> get = new() {
		[Natalie] = natalie,
		[Vladan] = vladan,
	};
}

[Flags]
public enum Team { None = 0, Alpha = 1, Bravo = 2, Delta = 4 }
public enum PlayerType { Human, Ai }
public enum AiDifficulty { Normal, Easy, Hard }

public class Player {

	public Level level;
	public Team team = Team.None;
	public Color color;
	public Co co;
	public PlayerType type = PlayerType.Human;
	public AiDifficulty difficulty = AiDifficulty.Normal;

	public Player(Level level, Color color, Team team = Team.None) {
		this.level = level;
		this.color = color;
		this.team = team;
		level.players.Add(this);
	}

	public override string ToString() {
		return Palette.ToString(color);
	}
}

public class Players : IDisposable {

	public List<Player> loop = new();
	public HashSet<Player> all = new();
	public GameObject go;

	public Players() {
		go = new GameObject(nameof(Players));
		Object.DontDestroyOnLoad(go);
	}

	public void Dispose() {
		Object.Destroy(go);
	}
}