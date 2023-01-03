using Butjok.CommandLine;
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
	[Command]
	public bool show = true;
	public TMP_Text text;
	[Command]
	public bool enableText = true;

	public bool Visible {
		set => gameObject.SetActive(value);
	}

	public void Reset() {
		meshRenderer = GetComponent<MeshRenderer>();
	}

	public void LateUpdate() {

		if (Mouse.TryGetPosition(out Vector2Int mousePosition) && (!Level.Instance || Level.Instance.TryGetTile(mousePosition, out _))) {
			meshRenderer.enabled = show;
			transform.position = mousePosition.ToVector3Int();
			if (text) {
				text.enabled = show && enableText;
				if (text.enabled)
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