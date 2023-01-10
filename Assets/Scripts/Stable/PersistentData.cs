using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class PersistentData {

    [Command]
    public static void Clear() {
        PlayerPrefs.DeleteKey(nameof(PersistentData));
    }
    public static PersistentData Read() {
        return PlayerPrefs.GetString(nameof(PersistentData))?.FromJson<PersistentData>() ?? new PersistentData();
    }
    public void Save() {
        PlayerPrefs.SetString(nameof(PersistentData), this.ToJson());
    }

    public bool firstTimeLaunch = true;
    public Campaign campaign = new();
    public List<SavedGame> savedGames = new() {
        new SavedGame {
            name = "Hello",
            screenshotPath = "/Users/butjok/vfedotov.com/playdead/11.PNG"
        },
        new SavedGame {
            name = "World",
            screenshotPath = "/Users/butjok/vfedotov.com/playdead/20.PNG"
        },
    };
    public GameSettings gameSettings = new();
    public List<string> log = new();
}

public class Campaign {

    public class Mission {
        public enum Name { Tutorial,FirstMission,SecondMission }
        
        public Name name;
        public string isAvailable = "true";
        public bool isCompleted;
        public string initializationCode;
    }

    public List<Mission> missions = new() {
        new Mission {
            name = Mission.Name.Tutorial,
            initializationCode = "Missions/Tutorial".LoadAs<TextAsset>().text,
            isCompleted = true
        },
        new Mission {
            name = Mission.Name.FirstMission,
            initializationCode = "Missions/FirstMission".LoadAs<TextAsset>().text,
            isAvailable = "Tutorial Campaign+Mission+Name type enum isCompleted"
        },
        new Mission {
            name = Mission.Name.SecondMission,
            initializationCode = "Missions/SecondMission".LoadAs<TextAsset>().text,
            isAvailable = "FirstMission Campaign+Mission+Name type enum isCompleted"
        }
    };

    public Mission TryFind(Mission.Name name) {
        return missions.SingleOrDefault(mission => mission.name == name);
    }

    public bool IsAvailable(Mission.Name name) {

        var mission = TryFind(name);
        Assert.IsNotNull(mission);

        if (mission.isCompleted)
            return true;

        var stack = new Stack();
        foreach (var token in mission.isAvailable.Tokenize()) {
            switch (token) {
                case "isCompleted":
                    var other = TryFind(stack.Pop<Mission.Name>());
                    Assert.IsNotNull(other);
                    stack.Push(other.isCompleted);
                    break;
                case "isAvailable":
                    stack.Push(IsAvailable(stack.Pop<Mission.Name>()));
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
    public string missionId;
    public string initializationCode;
    public string screenshotPath;

    public static string ScreenshotDirectoryPath => System.IO.Path.Combine(Application.persistentDataPath, "Screenshots");
    public const string screenshotExtension = ".png";

    public Texture2D LoadScreenshot() {
        if (!File.Exists(screenshotPath))
            return null;
        var texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(screenshotPath), true);
        return texture;
    }
    public void SaveScreenshot(Texture2D texture) {
        if (string.IsNullOrWhiteSpace(screenshotPath))
            screenshotPath = System.IO.Path.ChangeExtension(System.IO.Path.Combine(ScreenshotDirectoryPath, Guid.NewGuid().ToString()), screenshotExtension);
        var data = texture.EncodeToPNG();
        File.WriteAllBytes(screenshotPath, data);
    }
}