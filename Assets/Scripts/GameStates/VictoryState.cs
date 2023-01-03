using System.Collections;
using UnityEngine;

public static class VictoryState {
	public static IEnumerator New(Main main) {
	
		Debug.Log("Victory!");
		CursorView.Instance.Visible=false;
		PlayerView.globalVisibility = false;
		yield return null;
		GameUiView.Instance.Victory = true;
	}
}