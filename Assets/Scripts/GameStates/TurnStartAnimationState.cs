using System.Collections;
using UnityEngine;

public static class TurnStartAnimationState  {
	public static IEnumerator New(Level level) {
		Debug.Log($"Start of turn #{level.turn}");
		//yield return new WaitForSeconds(2);
		Debug.Log("GO!");
		yield break;
	}
}