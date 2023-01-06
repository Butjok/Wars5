using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class NewBehaviourScript : MonoBehaviour {

    public Unit unit;
    public MinimapMeshGenerator minimapMeshGenerator;
    public bool showDialogue = true;

    public TextAsset source;
    public Main main;

    private void Awake() {
        main = gameObject.AddComponent<Main>();
        main.levelLogic = new TutorialLogic(showDialogue);
    }
    
    [Command]
    public void SaveAndLoad() {
        var result = GameSaver.SaveToString(main);
        GameLoader.Load(main,result);
        main.RestartGame();
    }

    private void Start() {
        GameLoader.Load(main, source.text);
        main.RestartGame();
    }

#if false
    private void Awake() {


        main.levelLogic = new TutorialLogic(showDialogue);
        

        var commandsListener = gameObject.AddComponent<InputCommandsListener>();
#if WORKSTATION_MACBOOK
        commandsListener.inputPath = "/Users/butjok/Documents/GitHub/Wars5/Input.txt";
        commandsListener.outputPath = "/Users/butjok/Documents/GitHub/Wars5/Output.json";

        game.settings.motionBlurShutterAngle = null;
#elif WORKSTATION_PCY
        commandsListener.inputPath = "/Users/butjok/Documents/GitHub/Wars5/Input.txt";
        commandsListener.outputPath = "/Users/butjok/Documents/GitHub/Wars5/Output.json";

        main.settings.motionBlurShutterAngle = 270;
#endif

        main.UpdatePostProcessing();

        var battleViews = GetComponent<BattleViews>();

        CursorView.Instance.Visible = false;

        // var clampToHull = CameraRig.Instance.GetComponent<ClampToHull>();
        // if (clampToHull)
        //     clampToHull.Recalculate(game);

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

        //foreach (var saveData in SaveDataManager.All.OrderByDescending(s => s.dateTime))
        //    Debug.Log(saveData);


        /*var ser = new SerializedLevel(level);
        Debug.Log(ser.ToJson());
        var deser = ser.ToJson().FromJson<SerializedLevel>();*/

        if (minimapMeshGenerator) {
            minimapMeshGenerator.main = main;
            minimapMeshGenerator.Rebuild();
        }
    }
    #endif

    public void Write(string text, string relativePath = "Out.json") {
        var path = Path.Combine(Application.dataPath, relativePath);
        File.WriteAllText(path, text);
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