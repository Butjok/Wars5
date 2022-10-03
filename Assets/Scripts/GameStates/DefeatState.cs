using System.Collections;
using UnityEngine;

public static class DefeatState {
	public static IEnumerator New(Game game) {
	
		Debug.Log("Defeat...");
		GameUiView.Instance.Defeat = true;

		MusicPlayer.Instance.Queue = new[] { "slow uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
		
		yield break;
	}
}