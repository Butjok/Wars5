using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ControlFlow { Ignore, Replace, Pause }

public class LevelLogic {

    public virtual (ControlFlow controlFlow, IEnumerator state) OnTurnStart(Main main) {
        return (ControlFlow.Pause, PlayMusic(main.CurrentPlayer.co.themes));
    }
    
    protected IEnumerator PlayMusic(IEnumerable<AudioClip> clips) {
        if (MusicPlayer.TryGet(out var musicPlayer))
            musicPlayer.Queue = clips.InfiniteSequence();
        yield break;
    }
    protected IEnumerator PlayMusic(IEnumerable<string> clipNames) {
        return PlayMusic(clipNames.Select(name => name.LoadAs<AudioClip>()));
    }
    protected IEnumerator ExecuteAction(Action action) {
        action();
        yield break;
    }

    public virtual (ControlFlow controlFlow, IEnumerator state) OnTurnEnd(Main main) {
        return (ControlFlow.Ignore, null);
    }

    public virtual (ControlFlow controlFlow, IEnumerator state) OnActionCompletion(Main main, UnitAction action) {
        return (ControlFlow.Ignore, null);
    }

    public virtual IEnumerator OnVictory(Main main,UnitAction winningAction) {
        return null;
    }

    public virtual IEnumerator OnDefeat(Main main,UnitAction defeatingAction) {
        return null;
    }
}