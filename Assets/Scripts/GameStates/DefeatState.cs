using System.Collections;
using UnityEngine;

public static class DefeatState {
	public static IEnumerator New(Level level) {
	
		Debug.Log("Defeat...");
		CursorView.Instance.Visible=false;
		MusicPlayer.Instance.Queue = new[] { "slow uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
		PlayerView.globalVisibility = false;
		yield return null;
		GameUiView.Instance.Defeat = true;
		yield break;
	}
}