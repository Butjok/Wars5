using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
public class PlayerSettings {

    public float masterVolume = 1;
    public float musicVolume = 1;
    public float sfxVolume = 1;
    public bool showBattleAnimation = true;
    public float unitSpeed = 3;
    public PostProcessLayer.Antialiasing antiAliasing = PostProcessLayer.Antialiasing.TemporalAntialiasing;
    public float? motionBlurShutterAngle = 270;
    public bool bloom = true;
    public ScreenSpaceReflectionPreset? screenSpaceReflectionPreset = ScreenSpaceReflectionPreset.Lower;
    public float screenSpaceReflectionMaximumMarchDistance = 25;
    public bool ambientOcclusion = true;
    public bool shuffleMusic = false;

    public const string playerPrefsKey = nameof(PlayerSettings);

    public PlayerSettings() {
/*        var json = PlayerPrefs.GetString(playerPrefsKey);
        if (!string.IsNullOrWhiteSpace(json))
            JsonConvert.PopulateObject(json, this);*/
    }

    public void Save() {
        PlayerPrefs.SetString(playerPrefsKey, this.ToJson());
    }
}