using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class BlendShapesAnimation : MonoBehaviour {
	public SkinnedMeshRenderer renderer;
	public float speed = 1;
	public void Start() {
		renderer = GetComponent<SkinnedMeshRenderer>();
	}
	public void Update() {
		if (!renderer)
			return;
		var t = Time.time * speed % renderer.sharedMesh.blendShapeCount;
		var index = Mathf.FloorToInt(t);
		var next = (index + 1) % renderer.sharedMesh.blendShapeCount;
		t = t - index;
		for(var i=0;i<renderer.sharedMesh.blendShapeCount;i++)
			renderer.SetBlendShapeWeight(i,0);
		renderer.SetBlendShapeWeight(index, 100*(1 - t));
		renderer.SetBlendShapeWeight(next,t*100);
	}
}