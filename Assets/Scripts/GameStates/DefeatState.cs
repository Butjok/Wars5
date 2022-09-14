using System.Collections;
using UnityEngine;

public static class DefeatState {
	public static IEnumerator New(Game2 game) {
	
		Debug.Log("Defeat...");
		GameUiView.Instance.Defeat = true;

		MusicPlayer.Instance.source.Stop();
		MusicPlayer.Instance.queue = new[] { "slow uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
		
		yield break;
	}
}