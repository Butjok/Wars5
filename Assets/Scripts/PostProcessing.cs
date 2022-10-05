using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class PostProcessing {

    public static PostProcessProfile Profile => Resources.Load<PostProcessProfile>(nameof(PostProcessProfile));
    public static ColorGrading ColorGrading => Profile.GetSetting<ColorGrading>();
    public static ColorParameter ColorFilter => ColorGrading.colorFilter;
    public static Tweener fadeTweener;

    public static PostProcessLayer.Antialiasing Antialiasing {
        set {
            var layer = Camera.main ? Camera.main.GetComponent<PostProcessLayer>() : null;
            if (layer)
                layer.antialiasingMode = value;
        }
    }

    public static void Setup(
        PostProcessLayer.Antialiasing antialiasing = PostProcessLayer.Antialiasing.TemporalAntialiasing,
        float? motionBlurShutterAngle = 270,
        bool enableBloom = true,
        bool enableScreenSpaceReflections = true,
        bool enableAmbientOcclusion = true) {

        Antialiasing = antialiasing;

        var profile = Profile;
        if (!profile)
            return;

        var motionBlur = profile.GetSetting<MotionBlur>();
        if (motionBlur) {
            if (motionBlurShutterAngle is { } shutterAngle) {
                motionBlur.enabled.value = true;
                motionBlur.shutterAngle.value = shutterAngle;
            }
            else
                motionBlur.enabled.value = false;
        }

        var bloom = profile.GetSetting<Bloom>();
        if (bloom)
            bloom.enabled.value = enableBloom;

        var screenSpaceReflections = profile.GetSetting<ScreenSpaceReflections>();
        if (screenSpaceReflections) {
            screenSpaceReflections.enabled.value = enableScreenSpaceReflections;
        }

        var ambientOcclusion = profile.GetSetting<AmbientOcclusion>();
        if (ambientOcclusion)
            ambientOcclusion.enabled.value = enableAmbientOcclusion;
    }
}