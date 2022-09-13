using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {

	public static CursorView instance;

	public MeshRenderer meshRenderer;
	public bool show = true;

	public bool Visible {
		set => gameObject.SetActive(value);
	}
	
	public void Reset() {
		meshRenderer = GetComponent<MeshRenderer>();
	}

	public void Awake() {
		instance = this;
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