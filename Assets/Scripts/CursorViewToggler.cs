using UnityEngine;

public class CursorViewToggler : MonoBehaviour {
	public void Update() {
		if (!Game2.instance || !CursorView.instance)
			return;
		CursorView.instance.Visible = Game2.instance.state is SelectionState or PathSelectionState;
	}
}