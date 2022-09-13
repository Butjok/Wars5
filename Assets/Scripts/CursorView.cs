using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {

	private static CursorView instance;
	public static CursorView Instance {
		get {
			if (!instance) {
				instance = FindObjectOfType<CursorView>();
				Assert.IsTrue(instance);
			}
			return instance;
		}
	}

	public MeshRenderer meshRenderer;
	public bool show = true;

	public bool Visible {
		set => gameObject.SetActive(value);
	}
	
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