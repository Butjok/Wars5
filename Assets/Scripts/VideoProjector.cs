using UnityEngine;

[ExecuteInEditMode]
public class VideoProjector : MonoBehaviour {

    public Transform projection;
    public Material material;

    public void Update() {
        if (projection && material)
            material.SetMatrix("_WorldToProjection", projection.worldToLocalMatrix);
    }
}