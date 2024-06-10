using UnityEngine;

[ExecuteInEditMode]
public class HoleMaterialUpdater : MonoBehaviour {

    public Renderer renderer;
    public MaterialPropertyBlock materialPropertyBlock;
    public Transform unitTransform;

    private void Update() {
        if (!renderer || !unitTransform)
            return;
        materialPropertyBlock ??= new MaterialPropertyBlock();
        materialPropertyBlock.SetVector("_UnitPosition", unitTransform.position);
        renderer.SetPropertyBlock(materialPropertyBlock);
    }
}