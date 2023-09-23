using UnityEngine;

[ExecuteInEditMode]
public class VoronoiTest : MonoBehaviour {
    public Material material;
    public float scale = 1;
    public void Update() {
        material.SetVector("_Size", transform.localScale);
        material.SetFloat("_Scale", scale);
    }
}