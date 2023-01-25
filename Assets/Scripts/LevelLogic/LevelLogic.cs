using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLogic {

    public virtual StateChange OnTurnStart(Main main) {
        PlayMusic(main.CurrentPlayer.co.themes);
        return StateChange.none;
    }

    protected static void PlayMusic(IEnumerable<AudioClip> clips) {
        if (MusicPlayer.TryGet(out var musicPlayer))
            musicPlayer.Queue = clips.InfiniteSequence();
    }
    protected static void PlayMusic(IEnumerable<string> clipNames) {
        PlayMusic(clipNames.Select(name => name.LoadAs<AudioClip>()));
    }
    protected IEnumerator ExecuteAction(Action action) {
        action();
        yield return null;
    }

    public virtual StateChange OnTurnEnd(Main main) {
        return StateChange.none;
    }

    public virtual StateChange OnActionCompletion(Main main, UnitAction action) {
        action.Dispose();
        return StateChange.none;
    }

    public virtual StateChange OnVictory(Main main, UnitAction winningAction) {
        return StateChange.none;
    }

    public virtual StateChange OnDefeat(Main main, UnitAction defeatingAction) {
        return StateChange.none;
    }
}