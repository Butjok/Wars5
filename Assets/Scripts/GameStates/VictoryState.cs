using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VictoryDefeatState {

    public static IEnumerator<StateChange> Victory(Main main, UnitAction winningAction = null) {

        winningAction?.Dispose();

        if (CursorView.TryFind(out var cursor))
            cursor.show = false;
        PlayerView.globalVisibility = false;
        yield return StateChange.none;

        using var dialogue = new Dialogue();
        foreach (var stateChange in dialogue.Play(Dialogues.Victory))
            yield return stateChange;
    }

    public static IEnumerator<StateChange> Defeat(Main main, UnitAction defeatingAction = null) {

        defeatingAction?.Dispose();

        if (CursorView.TryFind(out var cursor))
            cursor.show = false;
        if (MusicPlayer.TryGet(out var musicPlayer))
            musicPlayer.Queue = new[] { "slow uzicko".LoadAs<AudioClip>() }.InfiniteSequence();
        PlayerView.globalVisibility = false;
        yield return StateChange.none;

        using var dialogue = new Dialogue();
        foreach (var stateChange in dialogue.Play(Dialogues.Defeat))
            yield return stateChange;
    }
}