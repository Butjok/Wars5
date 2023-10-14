using System.Collections;
using Butjok.CommandLine;
using UnityEngine;

public static class CameraAnimation {

    public static IEnumerator ZoomFadeAnimation(Camera camera, float fovDuration = 3, float startFovFactor = .75f) {
        var startTime = Time.time;
        var fadeDuration = fovDuration ;
        var endFov = camera.fieldOfView;
        var startFov = endFov * startFovFactor;
        while (Time.time < startTime + Mathf.Max(fovDuration, fadeDuration)) {
            camera.fieldOfView = Mathf.Lerp(startFov, endFov, Easing.OutQuad(Mathf.Clamp01((Time.time - startTime) / fovDuration)));
            PostProcessing.ColorFilter = Color.Lerp(Color.black, Color.white, (Mathf.Clamp01((Time.time - startTime) / fadeDuration)).Square());
            yield return null;
        }
        camera.fieldOfView = endFov;
        PostProcessing.ColorFilter = Color.white;
    }
}