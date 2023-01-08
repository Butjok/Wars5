using System.Collections;
using UnityEngine;

public static class DefeatState {
	public static IEnumerator Run(Main main,UnitAction defeatingAction) {
	
		Debug.Log("Defeat...");
		CursorView.Instance.Visible=false;
		MusicPlayer.Instance.Queue = new[] { "slow uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
		PlayerView.globalVisibility = false;
		yield return null;
		GameUiView.Instance.Defeat = true;
		yield break;
	}
}