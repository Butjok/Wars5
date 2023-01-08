using UnityEngine;

public class HighlightingCursorView : MonoBehaviour {
	
	private static HighlightingCursorView instance;
	public static bool TryFind(out HighlightingCursorView result) {
		if (!instance)
			instance = FindObjectOfType<HighlightingCursorView>();
		result = instance;
		return result;
	}

	public void ShowAt(Vector2Int position) {
		transform.position.ToVector2().RoundToInt();
	}
	public void Hide() {
		gameObject.SetActive(false);
	}
}