using System.Collections;
using UnityEngine;

public static class TurnStartAnimationState  {
	public static IEnumerator New(Main main) {
		Debug.Log($"Start of turn #{main.turn}");
		//yield return new WaitForSeconds(2);
		Debug.Log("GO!");
		yield break;
	}
}