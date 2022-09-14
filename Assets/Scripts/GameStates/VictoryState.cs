using System.Collections;
using UnityEngine;

public static class VictoryState {
	public static IEnumerator New(Game2 game) {
	
		Debug.Log("Victory!");
		GameUiView.Instance.Victory = true;
		
		MusicPlayer.Instance.source.Stop();
		MusicPlayer.Instance.queue = new[] { "fast uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
		
		yield break;
	}
}