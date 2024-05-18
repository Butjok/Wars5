using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
public class UpdateGodRaysShader : MonoBehaviour {
    public Material material;
    public Transform sun;
    public Color darkColor;
    public Color lightColor;
    public Vector2 distances;
    public Camera camera;
    public Vector2 bloomDistances;
     public Vector2 bloomIntensities;
    public PostProcessProfile postProcessProfile;
    public Bloom bloom;

    public void Update() {
        material.SetMatrix("_WorldToCookie", sun.worldToLocalMatrix);

        var height = camera.transform.position.y;
        var t = Mathf.Clamp01((height - distances[0]) / (distances[1] - distances[0]));
        material.SetColor("_Color", Color.Lerp(darkColor, lightColor, t));

        if (postProcessProfile && !bloom)
            bloom = postProcessProfile.GetSetting<Bloom>();
        if (bloom) {
            var bloomT = Mathf.Clamp01((height - bloomDistances[0]) / (bloomDistances[1] - bloomDistances[0]));
            bloom.intensity.value = Mathf.Lerp(bloomIntensities[0], bloomIntensities[1], bloomT);
        }
    }
}