using UnityEngine;
using UnityEngine.Assertions;

public class MineFieldView : MonoBehaviour {

    public MeshRenderer meshRenderer;

    public Color PlayerColor {
        set {
            var materialPropertyBlock = new MaterialPropertyBlock();
            if (meshRenderer.HasPropertyBlock())
                meshRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor("_PlayerColor", value);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
    public Vector2Int Position {
        set {
            transform.position = value.ToVector3();
            var bounds = meshRenderer.bounds;
            bounds.size = Vector3.one * 10;
            meshRenderer.bounds = bounds;
        }
    }

    public void Reset() {
        meshRenderer = GetComponent<MeshRenderer>();
        Assert.IsTrue(meshRenderer);
    }
}