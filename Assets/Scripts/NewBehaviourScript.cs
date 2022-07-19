using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public class NewBehaviourScript : MonoBehaviour {

	public Unit unit;
	public Level level;

	private void OnEnable() {

		var settings = new PlayerSettings {
			motionBlurShutterAngle = 270,
			bloom = true,
			antiAliasing = PostProcessLayer.Antialiasing.TemporalAntialiasing,
			
		};

		WarsPostProcess.Setup(settings, Camera.main ? Camera.main.GetComponent<PostProcessLayer>() : null);

		var game = new Game();

		var level = new Level(game);
		level.turn = 0;
		level.script = new Tutorial(level);

		game.State = level;
		
		

		/*Debug.Log(string.Join("\n",SaveDataManager.Names));
		SaveDataManager.Save("Hello", new SaveData {
			sceneName = "SampleScene",
			dateTime = DateTime.Now,
			Level = new SerializedLevel(level)
		});
		Debug.Log(string.Join("\n",SaveDataManager.Names));*/

		foreach (var saveData in SaveDataManager.All.OrderByDescending(s => s.dateTime))
			Debug.Log(saveData);

		///var test = Resources.Load<UnitView>("Test");

		var player = new Player(level, Palette.red, Team.Alpha);
		var player2 = new Player(level, Palette.green, Team.Bravo);
		unit = new Unit(level, player, position: new Vector2Int(1, 1), viewPrefab: Resources.Load<UnitView>("rockets"));
		unit = new Unit(level, player2, position: new Vector2Int(2, 2), viewPrefab: Resources.Load<UnitView>("rockets"));
		unit = new Unit(level, player2, position: new Vector2Int(2, 1), viewPrefab: Resources.Load<UnitView>("rockets"));
		//var a_ = unit.view;

		new Building(level, new Vector2Int(5, 5));

		for (var y = 0; y < 10; y++)
		for (var x = 0; x < 10; x++)
			level.tiles.Add(new Vector2Int(x, y), TileType.Plain);

		level.State = new SelectionState(level);

		/*var ser = new SerializedLevel(level);
		Debug.Log(ser.ToJson());
		var deser = ser.ToJson().FromJson<SerializedLevel>();*/
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
	public Game() : base(nameof(Game)) { }
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

public static class JsonUtils {
	static JsonUtils() {
		JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
			Formatting = Formatting.Indented,
			Converters = new List<JsonConverter> { new StringEnumConverter() },
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			TypeNameHandling = TypeNameHandling.Auto
		};
	}
	public static string ToJson(this object value) {
		return JsonConvert.SerializeObject(value);
	}
	public static T FromJson<T>(this string json) {
		return JsonConvert.DeserializeObject<T>(json);
	}
}