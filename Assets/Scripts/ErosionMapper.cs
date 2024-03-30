using UnityEngine;

public class ErosionMapper : MonoBehaviour {

    public Material targetMaterial;
    public string uniformName = "_Erosion_WorldToLocal";
    
    public void Update() {
        targetMaterial.SetMatrix(uniformName, transform.worldToLocalMatrix);
    }
}