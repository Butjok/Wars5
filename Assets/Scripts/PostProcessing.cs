using System.Runtime.InteropServices;
using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public static class PostProcessing {

    static PostProcessing() {
        profile = "PostProcessProfile1".LoadAs<PostProcessProfile>();
        colorGrading = profile.GetSetting<ColorGrading>();
        Assert.IsTrue(colorGrading);
        depthOfField = profile.GetSetting<DepthOfField>();
        Assert.IsTrue(depthOfField);
        cameraFlip = profile.GetSetting<CameraFlip>();
    }

    public static PostProcessProfile profile;
    public static ColorGrading colorGrading;
    public static CameraFlip cameraFlip;

    [Command] public static Color ColorFilter {
        get => colorGrading.colorFilter.value;
        set => colorGrading.colorFilter.value = value;
    }
    public static DepthOfField depthOfField;
    [Command] public static bool Blur {
        set => depthOfField.enabled.value = value;
    }
    [Command] public static bool Flip {
        set {
            if (cameraFlip)
                cameraFlip.enabled.value = value;
        }
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
            if (layer)
                layer.antialiasingMode = value;
        }
    }

    public static void Setup(Settings settings) {
        Setup(
            settings.video.enableAntiAliasing ? PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing : PostProcessLayer.Antialiasing.None,
            settings.video.enableMotionBlur ? 270 : null,
            settings.video.enableBloom,
            settings.video.enableAmbientOcclusion);
    }

    public static void Setup(PostProcessLayer.Antialiasing antialiasing, float? motionBlurShutterAngle, bool enableBloom, bool enableAmbientOcclusion) {

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

        var ambientOcclusion = profile.GetSetting<AmbientOcclusion>();
        if (ambientOcclusion)
            ambientOcclusion.enabled.value = enableAmbientOcclusion;
    }
}