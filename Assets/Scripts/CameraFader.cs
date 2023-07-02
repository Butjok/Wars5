using System;
using System.Collections;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraFader : MonoBehaviour {
    public static CameraFader instance;
    [Command]
    public static float duration = .4f;    
    public const string profileName = "PostProcessProfile";
    public static PostProcessProfile Profile => Resources.Load<PostProcessProfile>(profileName);
    public static CameraFading CameraFading {
        get {
            var profile = Profile;
            return profile ? profile.GetSetting<CameraFading>() : null;
        }
    }
    public static bool ?IsBlack {
        get {
            var cameraFading = CameraFading;
            if (!cameraFading)
                return null;
            return cameraFading.invert.value && cameraFading.progress.value > .5f ||
                   !cameraFading.invert.value && cameraFading.progress.value < .5f;
        }
    }
    public static IEnumerator Fade(bool inverted, float duration, Action onComplete) {
        var cameraFading = CameraFading;
        if (cameraFading) {
            cameraFading.invert.value = inverted;
            var startTime = Time.time;
            while (Time.time < startTime + duration) {
                cameraFading.progress.value = (Time.time - startTime) / duration;
                yield return null;
            }
            cameraFading.progress.value = 1;
        }
        onComplete?.Invoke();
    }
    public static Func<bool> Fade(bool inverted) {
        if (!instance) {
            var go = new GameObject(nameof(CameraFader));
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CameraFader>();
        }
        instance.StopAllCoroutines();
        var completed = false;
        instance.StartCoroutine(Fade(inverted, duration, () => completed = true));
        return () => completed;
    }
    [Command]
    public static Func<bool> FadeToBlack() => Fade(true);
    [Command]
    public static Func<bool> FadeToWhite() => Fade(false);
}