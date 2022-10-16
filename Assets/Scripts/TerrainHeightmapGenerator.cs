using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainHeightmapGenerator : MonoBehaviour {

	public BoxCollider boxCollider;
	public Texture2D heightmap;
	public int size = 1024;
	public float maxHeight = 10;
	public LayerMask layerMask;

	public void Reset() {
		layerMask = LayerMasks.Terrain;
		boxCollider = GetComponent<BoxCollider>();
	}

	[ContextMenu(nameof(Generate))]
	[Button]
	public void Generate() {

		if (!boxCollider) {
			Debug.Log("Please specify BoxCollider.", this);
			return;
		}

		heightmap = new Texture2D(size, size, TextureFormat.RFloat, false, true);

		var bounds = boxCollider.bounds;
		Assert.AreEqual(bounds.size.x, bounds.size.z);

		var pixelSize = bounds.size.x / size;

		for (var y = 0; y < size; y++)
		for (var x = 0; x < size; x++) {
			var uv = new Vector2((float)x / size, (float)y / size);
			var origin2d = bounds.min.ToVector2() + bounds.size.ToVector2() * uv + Vector2.one * pixelSize / 2;
			var origin3d = origin2d.ToVector3() + Vector3.up * maxHeight;

			if (Physics.Raycast(origin3d, Vector3.down, out var hit, float.MaxValue, layerMask)) {
				var color = new Color(maxHeight - hit.distance, 0, 0);
				heightmap.SetPixel(x, y, color);	
			}
			//heightmap.SetPixel(x, y, new Color(uv.y, 0, 0));
		}
		heightmap.Apply();

#if UNITY_EDITOR
		AssetDatabase.CreateAsset(heightmap, "Assets/Scenes/Heightmap.asset");
		AssetDatabase.SaveAssets();
#endif
	}
}

