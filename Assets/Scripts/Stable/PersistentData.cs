using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public class PersistentData {

    [Command]
    public static string Path => System.IO.Path.Combine(Application.persistentDataPath, nameof(PersistentData) + ".json");

    [Command]
    public static void Clear() {
        if (File.Exists(Path))
            File.Delete(Path);
    }
    public static PersistentData Read() {
        return File.Exists(Path) ? File.ReadAllText(Path).FromJson<PersistentData>() : new PersistentData();
    }
    public void Save() {
        File.WriteAllText(Path, this.ToJson());
    }

    public bool firstTimeLaunch = true;
    
    public Campaign campaign = new();
    
    public List<SavedGame> savedGames = new() {
        new SavedGame("hello", MissionName.Tutorial, "", screenshotPath:"/Users/butjok/vfedotov.com/playdead/11.PNG"),
        new SavedGame("world", MissionName.FirstMission, "", screenshotPath:"/Users/butjok/vfedotov.com/playdead/20.PNG"),
    };
    
    public GameSettings gameSettings = new();
    public List<string> log = new();
}

public class GameSettings {

    public float masterVolume = 1;
    public float musicVolume = 1;
    public float sfxVolume = 1;
    public float uiVolume = 1;
    public bool showBattleAnimation = true;
    public float unitSpeed = 3;
    public PostProcessLayer.Antialiasing antiAliasing = PostProcessLayer.Antialiasing.TemporalAntialiasing;
    public float? motionBlurShutterAngle = 270;
    public bool enableBloom = true;
    public bool enableScreenSpaceReflections = true;
    public bool enableAmbientOcclusion = true;
    public bool shuffleMusic = false;
    public bool enableDepthOfField = true;

    public GameSettings ShallowCopy() {
        return (GameSettings)MemberwiseClone();
    }
    public bool DiffersFrom(GameSettings other) {
        return this.ToJson() != other.ToJson();
    }
}

public class Campaign {

    public class Mission {

        public MissionName name;
        public string isAvailable = "true";
        public bool isCompleted;
        public string initializationCode;
    }

    public List<Mission> missions = new() {
        new Mission {
            name = MissionName.Tutorial,
            initializationCode = "Missions/Tutorial".LoadAs<TextAsset>().text,
            isCompleted = true
        },
        new Mission {
            name = MissionName.FirstMission,
            initializationCode = "Missions/FirstMission".LoadAs<TextAsset>().text,
            isAvailable = "Tutorial MissionName type enum isCompleted"
        },
        new Mission {
            name = MissionName.SecondMission,
            initializationCode = "Missions/SecondMission".LoadAs<TextAsset>().text,
            isAvailable = "FirstMission MissionName type enum isCompleted"
        }
    };

    public Mission TryFind(MissionName name) {
        return missions.SingleOrDefault(mission => mission.name == name);
    }

    public bool IsAvailable(MissionName name) {

        var mission = TryFind(name);
        Assert.IsNotNull(mission);

        if (mission.isCompleted)
            return true;

        var stack = new DebugStack();
        foreach (var token in Tokenizer.Tokenize(mission.isAvailable)) {
            switch (token) {
                case "isCompleted":
                    var other = TryFind(stack.Pop<MissionName>());
                    Assert.IsNotNull(other);
                    stack.Push(other.isCompleted);
                    break;
                case "isAvailable":
                    stack.Push(IsAvailable(stack.Pop<MissionName>()));
                    break;
                default:
                    stack.ExecuteToken(token);
                    break;
            }
        }
        Assert.AreEqual(1, stack.Count);
        return stack.Pop<bool>();
    }
}

public class SavedGame {

    public string name;
    public DateTime dateTime;
    public MissionName missionName;
    public string initializationCode;
    public string screenshotPath;

    public static string ScreenshotDirectoryPath => Path.Combine(Application.persistentDataPath, "Screenshots");
    public const string screenshotExtension = ".png";

    public SavedGame(string name, MissionName missionName, string initializationCode, Texture2D screenshot=null, string screenshotPath=null) {
        this.name = name;
        dateTime = DateTime.Now;
        this.missionName = missionName;
        this.initializationCode = initializationCode;
        this.screenshotPath = screenshotPath;
        if (screenshot)
            SaveScreenshot(screenshot);
    }
    
    public Texture2D LoadScreenshot() {
        if (string.IsNullOrWhiteSpace(screenshotPath)|| !File.Exists(screenshotPath))
            return null;
        var texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(screenshotPath), true);
        return texture;
    }
    
    public void SaveScreenshot(Texture2D texture) {
        if (!Directory.Exists(ScreenshotDirectoryPath))
            Directory.CreateDirectory(ScreenshotDirectoryPath);
        if (string.IsNullOrWhiteSpace(screenshotPath))
            screenshotPath = Path.ChangeExtension(Path.Combine(ScreenshotDirectoryPath, Guid.NewGuid().ToString()), screenshotExtension);
        var data = texture.EncodeToPNG();
        File.WriteAllBytes(screenshotPath, data);
    }
}