using System.Collections;
using Butjok.CommandLine;
using UnityEngine;

public class CameraRigAnimation : MonoBehaviour {

    public CameraRig cameraRig;

    [Command]
    public void PlayTestAnimation(float fovDuration) {
        StopAllCoroutines();
        StartCoroutine(ZoomFadeAnimation(cameraRig, fovDuration));
    }

    public static IEnumerator ZoomFadeAnimation(CameraRig cameraRig, float fovDuration = 3) {
        var startTime = Time.time;
        var fadeDuration = fovDuration ;
        cameraRig.enabled = false;
        var endFov = cameraRig.Fov;
        var startFov = endFov * .75f;
        while (Time.time < startTime + Mathf.Max(fovDuration, fadeDuration)) {
            cameraRig.Fov = Mathf.Lerp(startFov, endFov, Easing.OutQuad(Mathf.Clamp01((Time.time - startTime) / fovDuration)));
            PostProcessing.ColorFilter = Color.Lerp(Color.black, Color.white, (Mathf.Clamp01((Time.time - startTime) / fadeDuration)).Square());
            yield return null;
        }
        cameraRig.Fov = endFov;
        PostProcessing.ColorFilter = Color.white;
        cameraRig.enabled = true;
    }
}