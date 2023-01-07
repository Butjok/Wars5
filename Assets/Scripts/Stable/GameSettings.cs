using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
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

    public GameSettings ShallowCopy() {
        return (GameSettings)MemberwiseClone();
    }
    public bool DiffersFrom(GameSettings other) {
        return this.ToJson() != other.ToJson();
    }
}