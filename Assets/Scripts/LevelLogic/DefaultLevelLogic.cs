using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ControlFlow { Ignore, Replace, Pause }

public class DefaultLevelLogic {

    public virtual (ControlFlow controlFlow, IEnumerator state) OnTurnStart(Game game) {
        return (ControlFlow.Pause, PlayMusic(game.CurrentPlayer.co.themes));
    }
    
    public IEnumerator PlayMusic(IEnumerable<AudioClip> clips) {
        MusicPlayer.Instance.Queue = clips.InfiniteSequence();
        yield break;
    }
    public IEnumerator PlayMusic(IEnumerable<string> clipNames) {
        return PlayMusic(clipNames.Select(name => name.LoadAs<AudioClip>()));
    }

    public virtual (ControlFlow controlFlow, IEnumerator state) OnTurnEnd(Game game) {
        return (ControlFlow.Ignore, null);
    }

    public virtual (ControlFlow controlFlow, IEnumerator state) OnActionCompletion(Game game, UnitAction action) {
        return (ControlFlow.Ignore, null);
    }

    public virtual IEnumerator OnVictory(Game game) {
        return null;
    }

    public virtual IEnumerator OnDefeat(Game game) {
        return null;
    }
}