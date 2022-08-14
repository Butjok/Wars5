using UnityEngine;

public class StoneSplatMapSetter : MonoBehaviour {

	public BoxCollider boxCollider;
	public Material material;
	public Texture2D splat;
	public bool flipX;
	public bool flipY;

	public void Start() {
		material.SetTexture("_Splat", splat);
		var bounds = boxCollider.bounds;
		material.SetVector("_Bounds", new Vector4(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z));
		material.SetVector("_Flip", new Vector4(flipX?1:0,flipY?1:0,0,0));
		boxCollider.enabled = false;
	}
}