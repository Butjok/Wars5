using UnityEngine;

public class ForceStableFitShadowMapping : MonoBehaviour {
    private void OnEnable() {
        QualitySettings.shadowProjection = ShadowProjection.StableFit;
    }
    private void OnDisable() {
        QualitySettings.shadowProjection = ShadowProjection.CloseFit;
    }
}