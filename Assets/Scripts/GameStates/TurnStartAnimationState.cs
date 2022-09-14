using System.Collections;
using UnityEngine;

public static class TurnStartAnimationState  {
	public static IEnumerator New(Game2 game) {
		Debug.Log($"Start of turn #{game.turn}");
		//yield return new WaitForSeconds(2);
		Debug.Log("GO!");
		yield break;
	}
}