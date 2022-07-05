using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class NewBehaviourScript : MonoBehaviour {

	public Unit unit;
	public Level level;
	
	private void OnEnable() {
		
		level = new Level("Wars");
		level.turn = 0;

		///var test = Resources.Load<UnitView>("Test");
		
		var player = new Player(level, Color.red, Team.Alpha);
		var player2 = new Player(level, Color.blue,Team.Bravo);
		unit = new Unit(level, player, position: new Vector2Int(1, 1),viewPrefab:Resources.Load<UnitView>("rockets"));
		unit = new Unit(level, player2, position: new Vector2Int(3, 3),viewPrefab:Resources.Load<UnitView>("rockets"));
		//var a_ = unit.view;

		for (var y=0;y<10;y++)
		for (var x = 0; x < 10; x++)
			level.tiles.Add(new Vector2Int(x, y), TileType.Plain);
		
		level.state.v = new SelectionState(level);
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

	public Level level;
	public Team team = Team.None;
	public Color color ;
	public Co co;

	public Player(Level level,Color color,Team team=Team.None) {
		this.level = level;
		this.color = color;
		this.team = team;
		level.players.Add(color,this);
		level.playerLoop.Add(this);
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

[Flags]
public enum UnitType { Infantry = 1 << 0, AntiTank = 1 << 1, Artillery = 1 << 2
}