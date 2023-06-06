using System;
using UnityEngine;

[ExecuteInEditMode]
public class HoleMaterialUpdater : MonoBehaviour {

    public Renderer renderer;
    public MaterialPropertyBlock materialPropertyBlock;

    private void Update() {
        if (!renderer)
            return;
        var mainCamera = Camera.main;
        if (!mainCamera)
            return;
        materialPropertyBlock ??= new MaterialPropertyBlock();
        materialPropertyBlock.SetVector("_Origin", transform.position);
        materialPropertyBlock.SetVector("_Direction", mainCamera.transform.position - transform.position);
        materialPropertyBlock.SetVector("_CameraPosition", mainCamera.transform.position );
        renderer.SetPropertyBlock(materialPropertyBlock);
    }
}