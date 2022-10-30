using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
public class GameSettings {

    public float masterVolume = 1;
    public float musicVolume = 1;
    public float sfxVolume = 1;
    public bool showBattleAnimation = true;
    public float unitSpeed = 3;
    public PostProcessLayer.Antialiasing antiAliasing = PostProcessLayer.Antialiasing.TemporalAntialiasing;
    public float? motionBlurShutterAngle = 270;
    public bool enableBloom = true;
    public bool enableScreenSpaceReflections = true;
    public bool enableAmbientOcclusion = true;
    public bool shuffleMusic = false;

    public const string playerPrefsKey = nameof(GameSettings);

    public GameSettings ShallowCopy() {
        return (GameSettings)MemberwiseClone();
    }
    public static GameSettings Load() {
        var json = PlayerPrefs.GetString(playerPrefsKey);
        return string.IsNullOrWhiteSpace(json) ? new GameSettings() : JsonConvert.DeserializeObject<GameSettings>(json);
    }
    public void Save() {
        PlayerPrefs.SetString(playerPrefsKey, this.ToJson());
    }

    public bool DiffersFrom(GameSettings other) {
        return this.ToJson() != other.ToJson();
    }
}