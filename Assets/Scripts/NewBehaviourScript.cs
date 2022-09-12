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
	public MinimapMeshGenerator minimapMeshGenerator;

	public Game2 game;	

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


		var size = 5;
		var min = new Vector2Int(-size, -size);
		var max = new Vector2Int(size, size);

		game = gameObject.AddComponent<Game2>();
		game.levelLogic = new Tutorial(game);

		var red = new Player(game, Palette.red, Team.Alpha);
		var green = new Player(game, Palette.green, Team.Bravo);
		
		game.players = new List<Player> { red,green};

		game.tiles = new Map2D<TileType>(min, max);
		foreach (var position in game.tiles.positions)
			game.tiles[position] = TileType.Plain;
		
		game.units = new Map2D<Unit>(min, max);
		
		game.buildings = new Map2D<Building>(min, max);
		new Building(game, new Vector2Int(-2, -3));

		unit = new Unit(game, green, position: new Vector2Int(1, 1), viewPrefab: Resources.Load<UnitView>
		("mrap0-export"));
		unit.hp.v = 5;
		
		unit = new Unit(game, red, position: new Vector2Int(2, 2), viewPrefab: Resources.Load<UnitView>("light-tank"));
		unit.hp.v = 7;
		
		unit = new Unit(game, red, position: new Vector2Int(2, 1), viewPrefab: Resources.Load<UnitView>("light-tank"));

		game.Turn = 0;
		game.StartWith(new SelectionState(game));
		
		//var pathBuilderTest = FindObjectOfType<PathBuilderTest>();
		//if (pathBuilderTest)
		//	pathBuilderTest.level = level;


		/*Debug.Log(string.Join("\n",SaveDataManager.Names));
		SaveDataManager.Save("Hello", new SaveData {
			sceneName = "SampleScene",
			dateTime = DateTime.Now,
			Level = new SerializedLevel(level)
		});
		Debug.Log(string.Join("\n",SaveDataManager.Names));*/

		foreach (var saveData in SaveDataManager.All.OrderByDescending(s => s.dateTime))
			Debug.Log(saveData);

		

		/*var ser = new SerializedLevel(level);
		Debug.Log(ser.ToJson());
		var deser = ser.ToJson().FromJson<SerializedLevel>();*/
		
		if (minimapMeshGenerator) {
			minimapMeshGenerator.game = game;
			minimapMeshGenerator.Rebuild();
		}
	}

	public void Write(string text, string relativePath = "Out.json") {
		var path = Path.Combine(Application.dataPath, relativePath);
		File.WriteAllText(path,text);
	}

	/*[Command]
	public void Save(string name) {
		var saveData = new SaveData {
			name = name,
			sceneName = SceneManager.GetActiveScene().name,
			dateTime = DateTime.UtcNow,
			Level = new SerializedLevel(level)
		};
		SaveDataManager.Save(name,saveData);
		Debug.Log(saveData.json);
	}*/


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