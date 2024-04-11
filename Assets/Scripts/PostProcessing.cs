using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class PostProcessing : MonoBehaviour {

    public void Awake() {
        profile = "PostProcessProfile1".LoadAs<PostProcessProfile>();
        colorGrading = profile.GetSetting<ColorGrading>();
        Assert.IsTrue(colorGrading);
        depthOfField = profile.GetSetting<DepthOfField>();
        //Assert.IsTrue(depthOfField);
        cameraFlip = profile.GetSetting<CameraFlip>();
    }

    private static PostProcessing instance;

    public static PostProcessing Instance {
        get {
            if (!instance) {
                instance = FindObjectOfType<PostProcessing>();
                if (instance)
                    DontDestroyOnLoad(instance.gameObject);
                else {
                    var go = new GameObject(nameof(PostProcessing));
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<PostProcessing>();
                }
            }

            return instance;
        }
    }

    public PostProcessProfile profile;
    public ColorGrading colorGrading;
    public CameraFlip cameraFlip;
    public Image superPowerBorder;

    [Command] public static Color ColorFilter {
        get => Instance.colorGrading.colorFilter.value;
        set => Instance.colorGrading.colorFilter.value = value;
    }

    public static DepthOfField depthOfField;

    [Command] public static bool Blur {
        set => depthOfField.enabled.value = value;
    }

    [Command] public static bool Flip {
        set {
            if (Instance.cameraFlip)
                Instance.cameraFlip.enabled.value = value;
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

        var motionBlur = Instance.profile.GetSetting<MotionBlur>();
        if (motionBlur) {
            if (motionBlurShutterAngle is { } shutterAngle) {
                motionBlur.enabled.value = true;
                motionBlur.shutterAngle.value = shutterAngle;
            }
            else
                motionBlur.enabled.value = false;
        }

        var bloom = Instance.profile.GetSetting<Bloom>();
        if (bloom)
            bloom.enabled.value = enableBloom;

        var ambientOcclusion = Instance.profile.GetSetting<AmbientOcclusion>();
        if (ambientOcclusion)
            ambientOcclusion.enabled.value = enableAmbientOcclusion;
    }

    [Command] public static float ChromaticAberration {
        set {
            if (Instance.profile.TryGetSettings(out ChromaticAberration chromaticAberration))
                if (value != 0) {
                    chromaticAberration.enabled.value = true;
                    chromaticAberration.intensity.value = value;
                }
                else
                    chromaticAberration.enabled.value = false;
        }
    }

    [Command] public static float Bloom {
        set {
            if (Instance.profile.TryGetSettings(out Bloom bloom))
                if (value != 0) {
                    bloom.enabled.value = true;
                    bloom.intensity.value = value;
                }
                else
                    bloom.enabled.value = false;
        }
    }

    [Command] public static float Contrast {
        set {
            if (Instance.profile.TryGetSettings(out ColorGrading colorGrading))
                colorGrading.contrast.value = value;
        }
    }

    [Command] public static float Vignette {
        set {
            if (Instance.profile.TryGetSettings(out Vignette vignette))
                vignette.intensity.value = value;
        }
    }

    [Command] public static float Gain {
        set {
            if (Instance.profile.TryGetSettings(out ColorGrading colorGrading))
                colorGrading.gain.value = Vector4.one * value;
        }
    }

    [Command] public static bool ShowBorder {
        set {
            if (Instance.superPowerBorder)
                Instance.superPowerBorder.enabled = value;
        }
    }

    private static bool superPowerMode = false;
    [Command] public static bool SuperPowerMode {
        set {
            superPowerMode = value;
            if (value) {
                //ChromaticAberration = .1f;
                //Bloom = 5;
                Instance.StartCoroutine(Instance.Dance());
                //Contrast = 20;
                //Vignette = 0;
                ShowBorder = true;
                Music2.Play("hardbass".LoadAs<AudioClip>());
            }
            else {
                //ChromaticAberration = 0;
                //Bloom = 1;
                Instance.StopAllCoroutines();
                //Contrast = 5;
                //Vignette = .45f;
               // Gain = 0;
                ShowBorder = false;
                Music2.Stop();
            }
        }
        get => superPowerMode;
    }

    public float frequency = 5;
    public Vector2 bloomRange = new(5, 6);
    public Vector2 chromaticAberrationRange = new(.3f, .4f);
    public float gainAmplitude = 0;
    public IEnumerator Dance() {
        while (true) {
            var t = Time.time * frequency;
            ChromaticAberration = Mathf.Lerp(chromaticAberrationRange.x, chromaticAberrationRange.y, Mathf.PingPong(t, 1));
            Bloom = Mathf.Lerp(bloomRange.x, bloomRange.y, Mathf.PingPong(t, 1));
            Gain = Mathf.Lerp(0, gainAmplitude, Mathf.PingPong(t, 1));
            yield return null;
        }
    }
}