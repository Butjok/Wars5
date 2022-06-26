using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class NewBehaviourScript : MonoBehaviour {

	public Units units;
	public Unit unit;

	private void OnEnable() {
		var player = new Player { color = Color.red };
		units = new Units();
		unit = new Unit(units, player, position: new Vector2Int(1, 1), viewPrefab: Factions.novoslavia.v.GetUnitViewPrefab(UnitType.Infantry));
		//var a_ = unit.view;
	}
	private void OnDisable() {
		unit.Dispose();
		units.Dispose();
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


public class Factions {

	public const string Novoslavia = nameof(Novoslavia);
	public const string UnitedTreaty = nameof(UnitedTreaty);
	public static string[] names = { Novoslavia, UnitedTreaty };

	public static Lazy<Faction> novoslavia = new(() => Resources.Load<Faction>(Novoslavia));
	public static Lazy<Faction> unitedTreaty = new(() => Resources.Load<Faction>(UnitedTreaty));

	private static Dictionary<string, Lazy<Faction>> get = new() {
		[Novoslavia] = novoslavia,
		[UnitedTreaty] = unitedTreaty,
	};
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
	
	public Team team = Team.None;
	public Color color = Color.white;
	public Co co;
	
	
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