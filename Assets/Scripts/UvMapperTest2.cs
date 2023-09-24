using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class UvMapperTest2 : MonoBehaviour {

    public Material material;
    public string uniformName = "_WorldToLocal";
    
    public void Update() {
        if (!material)
            return;
        material.SetMatrix(uniformName, transform.worldToLocalMatrix);
    }
}