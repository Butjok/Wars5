using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class BlendShapesAnimation : MonoBehaviour {

	public SkinnedMeshRenderer renderer;
	public float speed = 1;
	public float time;
	public int firstFrame;
	public int frameCount;

	[ContextMenu(nameof(Clear))]
	public void Clear() {
		renderer = GetComponent<SkinnedMeshRenderer>();
		if (renderer)
			frameCount = renderer.sharedMesh.blendShapeCount;
	}

	public void Start() {
		renderer = GetComponent<SkinnedMeshRenderer>();
		time = Random.value * 200000;
	}

	public void Update() {
		
		if (!renderer)
			return;
		
		time += Time.deltaTime * speed;
		var index = Mathf.FloorToInt(time) % frameCount;
		var first = index % frameCount + firstFrame;
		var second = (index + 1) % frameCount + firstFrame;
		var t = time - Mathf.FloorToInt(time);
		
		for (var i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
			renderer.SetBlendShapeWeight(i, 0);
		
		renderer.SetBlendShapeWeight(first, 100 * (1 - t));
		renderer.SetBlendShapeWeight(second, t * 100);
	}
}