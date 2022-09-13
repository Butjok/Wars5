using System.Collections;
using UnityEngine;

public static class VictoryState {
	public static IEnumerator New(Game2 game) {
	
		Debug.Log("Victory!");
		GameUiView.Instance.Victory = true;
		
		yield break;
	}
}