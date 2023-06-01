using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VictoryDefeatState {

    public static IEnumerator<StateChange> Victory(Level level, UnitAction winningAction = null) {

        winningAction?.Dispose();

        if (CursorView.TryFind(out var cursor))
            cursor.show = false;
        PlayerView.globalVisibility = false;
        yield return StateChange.none;

        using var dialogue = new DialoguePlayer();
        foreach (var stateChange in dialogue.Play(Strings.Victory))
            yield return stateChange;
    }

    public static IEnumerator<StateChange> Defeat(Level level, UnitAction defeatingAction = null) {

        defeatingAction?.Dispose();
        
        if (CursorView.TryFind(out var cursor))
            cursor.show = false;
        if (MusicPlayer.TryGet(out var musicPlayer))
            musicPlayer.StartPlaying(new[] { "slow uzicko".LoadAs<AudioClip>() });
        PlayerView.globalVisibility = false;
        yield return StateChange.none;

        using var dialogue = new DialoguePlayer();
        foreach (var stateChange in dialogue.Play(Strings.Defeat))
            yield return stateChange;
    }
}