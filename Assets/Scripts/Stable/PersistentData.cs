using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public class PersistentData {

    [Command]
    public static string Path => System.IO.Path.Combine(Application.persistentDataPath, nameof(PersistentData) + ".json");

    private static PersistentData loaded;
    public static PersistentData Get => loaded ??= Read();

    [Command]
    public static void Clear() {
        if (File.Exists(Path))
            File.Delete(Path);
        loaded = null;
    }
    private static PersistentData Read() {
        return File.Exists(Path) ? File.ReadAllText(Path).FromJson<PersistentData>() : new PersistentData();
    }
    [Command]
    public static void Save() {
        File.WriteAllText(Path, Get.ToJson());
    }

    public Campaign campaign = new();

    public List<SavedGame> savedGames = new() {
        new SavedGame { missionName = MissionName.Tutorial },
        new SavedGame { missionName = MissionName.Tutorial },
    };

    public GameSettings gameSettings = new();
    public List<string> log = new();

    [Command]
    public static float UnitSpeed {
        get => Get.gameSettings.unitSpeed;
        set {
            Get.gameSettings.unitSpeed = value;
            Save();
        }
    }
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
    public bool animateNight = true;

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
        public bool isCompleted;

        public static string GetInputCode(MissionName missionName) {
            return missionName switch {
                MissionName.Tutorial => "TutorialSaveData".LoadAs<TextAsset>().text,
                _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
            };
        }
    }

    public List<Mission> missions = new() {
        new Mission { name = MissionName.Tutorial },
        new Mission { name = MissionName.FirstMission },
        new Mission { name = MissionName.SecondMission }
    };

    public bool IsAvailable(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial => true,
            MissionName.FirstMission => Find(MissionName.Tutorial).isCompleted,
            _ => false
        };
    }

    public Mission Find(MissionName name) {
        var mission = missions.SingleOrDefault(mission => mission.name == name);
        Assert.IsNotNull(mission, $"cannot find mission '{name}'");
        return mission;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class SavedGame {

    [JsonProperty]
    public string guid = Guid.NewGuid().ToString();
    [JsonProperty]
    public DateTime dateTime = DateTime.Now;
    [JsonProperty]
    public string name;
    [JsonProperty]
    public MissionName missionName;

    public string ScreenshotPath => Path.Combine(Application.persistentDataPath, guid) + ".png";
    public string InputCodePath => Path.Combine(Application.persistentDataPath, guid) + ".txt";

    public Texture2D Screenshot {
        get {
            if (!File.Exists(ScreenshotPath))
                return null;
            var texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(ScreenshotPath), true);
            return texture;
        }
        set {
            var data = value.EncodeToPNG();
            File.WriteAllBytes(ScreenshotPath, data);
        }
    }

    public string InputData {
        get {
            Assert.IsTrue(File.Exists(InputCodePath));
            return File.ReadAllText(InputCodePath);
        }
        set => File.WriteAllText(InputCodePath, value);
    }
}