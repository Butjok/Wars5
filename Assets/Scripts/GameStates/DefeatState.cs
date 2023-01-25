using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DefeatState {
    public static IEnumerator<StateChange> Run(Main main, UnitAction defeatingAction) {

        defeatingAction.Dispose();
        
        Debug.Log("Defeat...");
        if (CursorView.TryFind(out var cursor))
            cursor.show = false;
        if (MusicPlayer.TryGet(out var musicPlayer))
            musicPlayer.Queue = new[] { "slow uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
        PlayerView.globalVisibility = false;
        yield return StateChange.none;
        // GameUiView.Instance.Defeat = true;
        
    }
}