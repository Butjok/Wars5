using System;
using System.Collections;
using Butjok.CommandLine;
using Cinemachine;
using UnityEngine;

public static class CameraAnimation {

    public static IEnumerator ZoomFadeAnimation(Camera camera, float fovDuration = 3, float startFovFactor = .75f) {
        var brain = camera.GetComponent<CinemachineBrain>();
        var virtualCamera = brain ? (CinemachineVirtualCamera) brain.ActiveVirtualCamera : null;
        float GetFov() {
            return virtualCamera ? virtualCamera.m_Lens.FieldOfView : camera.fieldOfView;
        }
        void SetFov(float value) {
            if (virtualCamera)
                virtualCamera.m_Lens.FieldOfView = value;
            else
                camera.fieldOfView = value;
        }
        var startTime = Time.time;
        var fadeDuration = fovDuration ;
        var endFov = GetFov();
        var startFov = endFov * startFovFactor;
        while (Time.time < startTime + Mathf.Max(fovDuration, fadeDuration)) {
            SetFov(Mathf.Lerp(startFov, endFov, Easing.OutQuad(Mathf.Clamp01((Time.time - startTime) / fovDuration))));
            Color color = Color.black;
            var t =  Mathf.Clamp01((Time.time - startTime) / fadeDuration);
            PostProcessing.ColorFilter = Color.HSVToRGB(.125f, Mathf.Lerp(.125f, 0, t), t*t); 
            //PostProcessing.ColorFilter = Color.Lerp(Color.black, Color.white, (Mathf.Clamp01((Time.time - startTime) / fadeDuration)).Square());
            yield return null;
        }
        SetFov(endFov);
        PostProcessing.ColorFilter = Color.white;
    }
}