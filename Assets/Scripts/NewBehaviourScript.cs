using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour {

	public static Level level2;
	
	public Unit unit;
	public Level level;
	public Game game;
	public MinimapMeshGenerator minimapMeshGenerator;

	private void OnEnable() {

		//create
		//var unit = new Unit(this.level, new Player());

		//load settings


		var settings = new PlayerSettings {
			motionBlurShutterAngle = null,
			bloom = true,
			antiAliasing = PostProcessLayer.Antialiasing.None,//PostProcessLayer.Antialiasing.TemporalAntialiasing,
			ambientOcclusion=true
		};

		//WarsPostProcess.Setup(settings, Camera.main ? Camera.main.GetComponent<PostProcessLayer>() : null);

		game = new Game();

		var min = new Vector2Int(-3, -3);
		var max = new Vector2Int(3, 3);

		level = new Level(min,max, game);
		level.turn = 0;
		level.script = new Tutorial(level);

		game.State = level;


		level2 = level;
		


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
		unit = new Unit(level, player, position: new Vector2Int(1, 1), viewPrefab: Resources.Load<UnitView>("mrap0-export"));
		unit = new Unit(level, player2, position: new Vector2Int(2, 2), viewPrefab: Resources.Load<UnitView>("light-tank"));
		unit = new Unit(level, player2, position: new Vector2Int(2, 1), viewPrefab: Resources.Load<UnitView>("light-tank"));
		//var a_ = unit.view;

		new Building(level, new Vector2Int(-2, -3));

		foreach (var position in level.tiles.positions)
			level.tiles[position] = TileType.Plain;

		Write(new SerializedLevel(level).ToJson());
		
		level.State = new SelectionState(level);

		/*var ser = new SerializedLevel(level);
		Debug.Log(ser.ToJson());
		var deser = ser.ToJson().FromJson<SerializedLevel>();*/
		
		if (minimapMeshGenerator) {
			minimapMeshGenerator.level = level;
			minimapMeshGenerator.Rebuild();
		}
	}
	private void OnDisable() {
		unit?.Dispose();
		level?.Dispose();
	}

	public void Write(string text, string relativePath = "Out.json") {
		var path = Path.Combine(Application.dataPath, relativePath);
		File.WriteAllText(path,text);
	}

	[Command]
	public void Save(string name) {
		var saveData = new SaveData {
			name = name,
			sceneName = SceneManager.GetActiveScene().name,
			dateTime = DateTime.UtcNow,
			Level = new SerializedLevel(level)
		};
		SaveDataManager.Save(name,saveData);
		Debug.Log(saveData.json);
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