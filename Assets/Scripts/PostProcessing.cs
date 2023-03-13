using System.Runtime.InteropServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public static class PostProcessing {

    static PostProcessing() {
        profile = nameof(PostProcessProfile).LoadAs<PostProcessProfile>();
        colorGrading = profile.GetSetting<ColorGrading>();
        Assert.IsTrue(colorGrading);
    }

    public static PostProcessProfile profile;
    public static ColorGrading colorGrading;
    public static Color ColorFilter {
        get => colorGrading.colorFilter.value;
        set => colorGrading.colorFilter.value = value;
    }
    
    private static Tweener fadeTweener;

    public static Tweener Fade(Color color, float duration, Ease ease = default) {
        fadeTweener?.Kill();
        fadeTweener = DOTween.To(() => ColorFilter, value => ColorFilter = value, color, duration).SetEase(ease);
        fadeTweener.onComplete += () => ColorFilter = color;
        return fadeTweener;
    }

    public static PostProcessLayer.Antialiasing Antialiasing {
        set {
            var layer = Camera.main ? Camera.main.GetComponent<PostProcessLayer>() : null;
            //if (layer)
                //layer.antialiasingMode = value;
        }
    }

    public static void Setup(GameSettings gameSettings) {
        Setup(
            gameSettings.antiAliasing,
            gameSettings.motionBlurShutterAngle,
            gameSettings.enableBloom,
            gameSettings.enableScreenSpaceReflections,
            gameSettings.enableAmbientOcclusion);
    }

    public static void Setup(
        PostProcessLayer.Antialiasing antialiasing = PostProcessLayer.Antialiasing.TemporalAntialiasing,
        float? motionBlurShutterAngle = 270,
        bool enableBloom = true,
        bool enableScreenSpaceReflections = true,
        bool enableAmbientOcclusion = true) {

        Antialiasing = antialiasing;

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