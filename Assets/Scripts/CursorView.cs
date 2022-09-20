using TMPro;
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
	public TMP_Text text;

	public bool Visible {
		set => gameObject.SetActive(value);
	}
	
	public void Reset() {
		meshRenderer = GetComponent<MeshRenderer>();
	}

	public void LateUpdate() {
		if (Input.GetKeyDown(KeyCode.Keypad7))
			show = !show;
		if (Mouse.TryGetPosition(out Vector2Int mousePosition)) {
			meshRenderer.enabled = show;
			transform.position =  mousePosition.ToVector3Int();
			if (text) {
				text.enabled = true;
				text.text = mousePosition.ToString();
			}
		}
		else {
			meshRenderer.enabled = false;
			if (text)
				text.enabled = false;
		}
	}
}