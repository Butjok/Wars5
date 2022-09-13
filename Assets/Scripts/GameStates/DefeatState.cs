using System.Collections;
using UnityEngine;

public static class DefeatState {
	public static IEnumerator New(Game2 game) {
		Debug.Log("Defeat...");
		yield break;
	}
}