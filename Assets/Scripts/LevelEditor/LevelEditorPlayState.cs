using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelEditorPlayState : StateMachine.State {

    public string save;

    public LevelEditorPlayState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var editorState = stateMachine.TryFind<LevelEditorState>();
            var level = editorState.level;
            level.view.gameObject.SetActive(false);

            using var tw = new StringWriter();
            LevelWriter.WriteLevel(tw, level);
            save = tw.ToString();

            editorState.gui
                .Push();
            
            yield return StateChange.Push(new PlayState(stateMachine, save));
        }
    }
    public override void Dispose() {
        var editorState = stateMachine.TryFind<LevelEditorState>();
        editorState.level.view.gameObject.SetActive(true);
        editorState.gui.Pop();
    }
}