using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {
	public MeshRenderer meshRenderer;
	public void Reset() {
		meshRenderer = GetComponent<MeshRenderer>();
	}
	public void LateUpdate() {
		if (Mouse.TryGetPosition(out Vector2Int position)) {
			meshRenderer.enabled = true;
			transform.position = position.ToVector3Int();
		}
		else
			meshRenderer.enabled = false;
	}
}