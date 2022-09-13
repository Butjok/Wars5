using System.Collections;
using UnityEngine;

public static class VictoryState {
	public static IEnumerator New(Game2 game) {
		Debug.Log("Victory!");
		yield break;
	}
}