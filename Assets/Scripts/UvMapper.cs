using UnityEngine;

public class UvMapper : MonoBehaviour {

    public Material material;
    public string uniformName = "_MainTexInverseTransform";

    private void Update() {
        if (material)
            material.SetMatrix(uniformName, transform.worldToLocalMatrix);
    }
}