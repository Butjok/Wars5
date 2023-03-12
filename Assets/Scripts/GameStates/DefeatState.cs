using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DefeatState {
    public static IEnumerator<StateChange> Run(Level level, UnitAction defeatingAction) {

        defeatingAction.Dispose();
        
        Debug.Log("Defeat...");
        if (CursorView.TryFind(out var cursor))
            cursor.show = false;
        if (MusicPlayer.TryGet(out var musicPlayer))
            musicPlayer.StartPlaying(new[] { "slow uzicko".LoadAs<AudioClip>() });
        PlayerView.globalVisibility = false;
        yield return StateChange.none;
        // GameUiView.Instance.Defeat = true;
        
    }
}