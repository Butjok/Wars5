using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerSettings {
	public float masterVolume = 1;
	public float musicVolume = 1;
	public float sfxVolume = 1;
	public bool showBattleAnimation = true;
	public float unitSpeed = 3;
	public PostProcessLayer.Antialiasing antiAliasing = PostProcessLayer.Antialiasing.TemporalAntialiasing;
	public float? motionBlurShutterAngle = 270;
	public bool bloom = true;
	public ScreenSpaceReflectionPreset? screenSpaceReflectionPreset = ScreenSpaceReflectionPreset.Overkill;
	public float screenSpaceReflectionMaximumMarchDistance = 5;
	public AmbientOcclusionMode? ambientOcclusionMode = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
}

public static class PlayerSettingsManager {
	public const string playerPrefsKey = nameof(PlayerSettings);
	public static PlayerSettings Read() {
		var json = PlayerPrefs.GetString(playerPrefsKey);
		return string.IsNullOrWhiteSpace(json) ? new PlayerSettings() : json.FromJson<PlayerSettings>();
	}
	public static void Save(PlayerSettings playerSettings) {
		PlayerPrefs.SetString(playerPrefsKey, playerSettings.ToJson());
	}
}