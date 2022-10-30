using System.Collections;
using UnityEngine;

public static class VictoryState {
	public static IEnumerator New(Game game) {
	
		Debug.Log("Victory!");
		CursorView.Instance.Visible=false;
		GameUiView.Instance.Victory = true;
		
		yield break;
	}
}