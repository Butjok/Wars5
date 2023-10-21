using System;
using System.Collections.Generic;
using System.IO;
using Butjok.CommandLine;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class PersistentData {

    [Command]
    public static string Path => System.IO.Path.Combine(Application.persistentDataPath, nameof(PersistentData) + ".json");

    public static PersistentData Read(GameSessionState gameSessionState) {
        var persistentData = File.Exists(Path) ? JsonConvert.DeserializeObject<PersistentData>(File.ReadAllText(Path)) : new PersistentData();
        persistentData.gameSessionState = gameSessionState;
        return persistentData;
    }
    public void Write() {
        File.WriteAllText(Path, this.ToJson());
        Debug.Log($"Written:\n\n" + this.ToJson());
    }

    public GameSessionState gameSessionState;
    [JsonProperty] public Settings settings;
    [JsonProperty] public Campaign campaign;

    public PersistentData() {
        settings = new Settings { persistentData = this };
        campaign = new Campaign { persistentData = this };
    }
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class Settings {

    [JsonProperty] public PersistentData persistentData;
    [JsonProperty] public AudioSettings audio;
    [JsonProperty] public GameSettings game;
    [JsonProperty] public VideoSettings video;

    public Settings() {
        audio = new AudioSettings { settings = this };
        game = new GameSettings { settings = this };
        video = new VideoSettings { settings = this };
    }
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class VideoSettings {

    [JsonProperty] public Settings settings;

    [JsonProperty] public bool enableAntiAliasing = true;
    [JsonProperty] public bool enableMotionBlur = false;
    [JsonProperty] public bool enableBloom = true;
    [JsonProperty] public bool enableAmbientOcclusion = true;
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class GameSettings {

    [JsonProperty] public Settings settings;

    [JsonProperty] public bool showBattleAnimation = true;
    [JsonProperty] public float unitSpeed = 3;
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class AudioSettings {

    [JsonProperty] public Settings settings;

    [JsonProperty] public VolumeAudioSettings volume;
    [JsonProperty] public bool shuffleMusic = true;

    public AudioSettings() {
        volume = new VolumeAudioSettings { audioSettings = this };
    }
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class VolumeAudioSettings {

    [JsonProperty] public AudioSettings audioSettings;

    [JsonProperty] public float master = 1;
    [JsonProperty] public float music = 1;
    [JsonProperty] public float effects = 1;
    [JsonProperty] public float ui = 1;
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class Campaign {

    [JsonProperty] public PersistentData persistentData;

    [JsonProperty] public Missions.Tutorial tutorial;
    [JsonProperty] public Missions.FirstMission firstMission;
    [JsonProperty] public Missions.SecondMission secondMission;

    public Campaign() {
        tutorial = new Missions.Tutorial { campaign = this };
        firstMission = new Missions.FirstMission { campaign = this };
        secondMission = new Missions.SecondMission { campaign = this };
    }

    public IEnumerable<Mission> Missions {
        get {
            yield return tutorial;
            yield return firstMission;
            yield return secondMission;
        }
    }

    public Mission GetMission(Type type) {
        foreach (var mission in Missions)
            if (mission.GetType() == type)
                return mission;
        throw new ArgumentOutOfRangeException(type.Name);
    }
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public abstract class Mission {

    [JsonProperty] public Campaign campaign;
    public virtual string SceneName => "LevelEditor";
    public abstract string Input { get; }
    public virtual bool IsAvailable => true;
    public virtual string Name => GetType().Name;
    public virtual string Description => "";
    public virtual Sprite LoadingScreen => null;

    [JsonProperty] public bool isCompleted;
    [JsonProperty] public List<SavedMission> saves = new();
}

[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class SavedMission {

    [JsonProperty] public Mission mission;
    [JsonProperty] public DateTime dateTimeUtc;
    [JsonProperty] public string input;
    [JsonProperty] public byte[] screenshot;

    public Texture2D Screenshot {
        get {
            if (screenshot == null)
                return null;
            var texture = new Texture2D(2, 2);
            texture.LoadImage(screenshot, true);
            return texture;
        }
        set => screenshot = value == null ? null : value.EncodeToPNG();
    }
}