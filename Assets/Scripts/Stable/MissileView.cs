using UnityEngine;

public class MissileView : BallisticMotion {

    public MeshRenderer renderer;
    public MaterialPropertyBlock materialPropertyBlock;
	
    public Color PlayerColor {
        set {
            materialPropertyBlock ??= new MaterialPropertyBlock();
            materialPropertyBlock.SetColor("_PlayerColor", value);
            renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}