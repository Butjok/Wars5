using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

public class SplatMapSetter : MonoBehaviour {

    public BoxCollider boxCollider;
    public Material material;
    [HideIf("material")] public Renderer renderer;
    [HideIf("material")] public bool instanceMaterial = true;
    public Texture2D splat;
    public bool flipX;
    public bool flipY;
    public string textureName = "_Splat";
    public string boundsName = "_Bounds";
    public string flipName = "_Flip";

    public void Start() {

        if (!material) {
            Assert.IsTrue(renderer);
            material = instanceMaterial ? renderer.material : renderer.sharedMaterial;
        }

        material.SetTexture(textureName, splat);
        boxCollider.enabled = true;
        var bounds = boxCollider.bounds;
        material.SetVector(boundsName, new Vector4(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z));
        material.SetVector(flipName, new Vector4(flipX ? 1 : 0, flipY ? 1 : 0, 0, 0));
        boxCollider.enabled = false;
    }
}