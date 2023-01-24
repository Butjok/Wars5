using System.Collections;
using UnityEngine;

public static class DefeatState {
    public static IEnumerator Run(Main main, UnitAction defeatingAction) {

        Debug.Log("Defeat...");
        if (CursorView.TryFind(out var cursor))
            cursor.Visible = false;
        if (MusicPlayer.TryGet(out var musicPlayer))
            musicPlayer.Queue = new[] { "slow uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
        PlayerView.globalVisibility = false;
        yield return null;
        // GameUiView.Instance.Defeat = true;
        yield break;
    }
}