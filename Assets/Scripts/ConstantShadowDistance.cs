using UnityEngine;

[ExecuteInEditMode]
public class ConstantShadowDistance : MonoBehaviour {
    public float distance=15;
    public void LateUpdate() {
        QualitySettings.shadowDistance = distance;
    }
}