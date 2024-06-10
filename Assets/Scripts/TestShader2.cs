using UnityEngine;

public class TestShader2 : MonoBehaviour {
    public Renderer renderer;
    private void Update() {
        LightProbeUtility.SetSHCoefficients(transform.position, renderer.sharedMaterial);
    }
}