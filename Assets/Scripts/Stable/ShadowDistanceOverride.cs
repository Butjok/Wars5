using UnityEngine;

[ExecuteInEditMode]
public class ShadowDistanceOverride : MonoBehaviour {
    public float distance = 15;
    public void LateUpdate() {
        QualitySettings.shadowDistance = distance;
    }
}
