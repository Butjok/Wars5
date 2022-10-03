using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class WarsPostProcess {

	public static void Setup(GameSettings gameSettings, PostProcessLayer postProcessLayer = null) {

		if (!postProcessLayer)
			postProcessLayer = Camera.main ? Camera.main.GetComponent<PostProcessLayer>() : null;
		if (postProcessLayer)
			postProcessLayer.antialiasingMode = gameSettings.antiAliasing;

		var postProcessProfile = Resources.Load<PostProcessProfile>(nameof(PostProcessProfile));
		if (!postProcessProfile)
			return;

		var motionBlur = postProcessProfile.GetSetting<MotionBlur>();
		if (motionBlur) {
			if (gameSettings.motionBlurShutterAngle is { } shutterAngle) {
				motionBlur.enabled.value = true;
				motionBlur.shutterAngle.value = shutterAngle;
			}
			else
				motionBlur.enabled.value = false;
		}

		var bloom = postProcessProfile.GetSetting<Bloom>();
		if (bloom)
			bloom.enabled.value = gameSettings.bloom;

		var screenSpaceReflections = postProcessProfile.GetSetting<ScreenSpaceReflections>();
		if (screenSpaceReflections) {
			screenSpaceReflections.enabled.value = gameSettings.screenSpaceReflections;
		}

		var ambientOcclusion = postProcessProfile.GetSetting<AmbientOcclusion>();
		if (ambientOcclusion)
			ambientOcclusion.enabled.value = gameSettings.ambientOcclusion;
	}
}