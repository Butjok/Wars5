using UnityEngine;

[ExecuteInEditMode]
public class UpdateGodRaysShader : MonoBehaviour {
    public Material material;
    public Transform sun;

    public void Update() {
        material.SetMatrix( "_WorldToCookie", sun.worldToLocalMatrix);
    }
}