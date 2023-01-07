using UnityEngine;

public static class GameObjectUtils {
	public static void SetLayerRecursively(this GameObject go, int layer) {
		go.layer = layer;
		for (var i = 0; i < go.transform.childCount; i++)
			SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
	}
}