using UnityEngine.Rendering.PostProcessing;

public static class WarsPostProcess {
	
	public static void Setup(PlayerSettings playerSettings, PostProcessLayer postProcessLayer = null) {

		if (postProcessLayer)
			postProcessLayer.antialiasingMode = playerSettings.antiAliasing;

		var postProcessProfile = GlobalPostProcessVolume.Instance.profile;

		var motionBlur = postProcessProfile.GetSetting<MotionBlur>();
		if (motionBlur) {
			if (playerSettings.motionBlurShutterAngle is { } shutterAngle) {
				motionBlur.enabled.value = true;
				motionBlur.shutterAngle.value = shutterAngle;
			}
			else
				motionBlur.enabled.value = false;
		}

		var bloom = postProcessProfile.GetSetting<Bloom>();
		if (bloom)
			bloom.enabled.value = playerSettings.bloom;

		var screenSpaceReflections = postProcessProfile.GetSetting<ScreenSpaceReflections>();
		if (screenSpaceReflections) {
			if (playerSettings.screenSpaceReflectionPreset is { } screenSpaceReflectionPreset) {
				screenSpaceReflections.enabled.value = true;
				screenSpaceReflections.preset.value = screenSpaceReflectionPreset;
				screenSpaceReflections.maximumMarchDistance.value = playerSettings.screenSpaceReflectionMaximumMarchDistance;
			}
			else
				screenSpaceReflections.enabled.value = false;
		}

		var ambientOcclusion = postProcessProfile.GetSetting<AmbientOcclusion>();
		if (ambientOcclusion) {
			if (playerSettings.ambientOcclusionMode is { } ambientOcclusionMode) {
				ambientOcclusion.enabled.value = true;
				ambientOcclusion.mode.value = ambientOcclusionMode;
			}
			else
				ambientOcclusion.enabled.value = false;
		}
	}
}