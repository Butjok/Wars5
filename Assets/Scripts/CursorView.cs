using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {
	public MeshRenderer meshRenderer;
	public bool show = true;
	public void Reset() {
		meshRenderer = GetComponent<MeshRenderer>();
	}
	public void LateUpdate() {
		if (Input.GetKeyDown(KeyCode.F11)) 
			show = !show;
		if (Mouse.TryGetPosition(out Vector2Int position)) {
			meshRenderer.enabled = show;
			transform.position = position.ToVector3Int();
		}
		else
			meshRenderer.enabled = false;
	}
}