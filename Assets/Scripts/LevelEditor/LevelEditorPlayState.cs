using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelEditorPlayState : StateMachineState {

    public string input;
    public LevelEditorPlayState(StateMachine stateMachine) : base(stateMachine) {
    }

    public override IEnumerator<StateChange> Enter {
        get {
            var editorState = stateMachine.TryFind<LevelEditorSessionState>();
            var level = editorState.level;
            level.view.gameObject.SetActive(false);
            
            editorState.gui.layerStack.Push(() => { });

            using var stringWriter = new StringWriter();
            new LevelWriter(stringWriter).WriteLevel(level);
            input = stringWriter.ToString();
            
            if (editorState.musicSource)
                Music.Mute(editorState.musicSource);

            yield return StateChange.Push(new LevelSessionState(stateMachine, new SavedMission{mission = level.mission, input = input }));
        }
    }
    public override void Exit() {
        var editorState = stateMachine.TryFind<LevelEditorSessionState>();
        editorState.level.view.gameObject.SetActive(true);

        editorState.gui.layerStack.Pop();
        
        if (editorState.musicSource)
            Music.Unmute(editorState.musicSource);
    }
}