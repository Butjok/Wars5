using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class NewBehaviourScript : MonoBehaviour {

	public Unit unit;
	public Game game;
	
	private void OnEnable() {
		
		game = new Game("Wars");
		game.turn = 0;

		///var test = Resources.Load<UnitView>("Test");
		
		var player = new Player(game, Color.red);
		var player2 = new Player(game, Color.blue);
		unit = new Unit(game, player, position: new Vector2Int(1, 1));
		unit = new Unit(game, player, position: new Vector2Int(3, 3));
		//var a_ = unit.view;

		
		game.state.v = new SelectionState(game);
	}
	private void OnDisable() {
		unit?.Dispose();
		game?.Dispose();
	}


	// Start is ca
	// lled before the first frame update
	void Start() {
		//	var items = PlayerSettings.GetPreloadedAssets();
	}

	// Update is called once per frame
	void Update() { }
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

public class Player {

	public Game game;
	public Team team = Team.None;
	public Color color ;
	public Co co;

	public Player(Game game,Color color) {
		this.game = game;
		this.color = color;
		game.players.Add(color,this);
		game.playerLoop.Add(this);
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

public enum UnitType { Infantry, AntiTank }