using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelEditorPlayState : StateMachineState {

    public string save;
    public LevelEditorPlayState(StateMachine stateMachine) : base(stateMachine) {
    }

    public override IEnumerator<StateChange> Enter {
        get {
            var editorState = stateMachine.TryFind<LevelEditorSessionState>();
            var level = editorState.level;
            level.view.gameObject.SetActive(false);

            using var stringWriter = new StringWriter();
            new PrefixWriter(stringWriter).Level(level);
            save = stringWriter.ToString();

            editorState.gui
                .Push();

            yield return StateChange.Push(new LevelSessionState(stateMachine, save, level.missionName, editorState.playAsFreshStart, level.precalculatedDistances));
        }
    }
    public override void Exit() {
        var editorState = stateMachine.TryFind<LevelEditorSessionState>();
        editorState.level.view.gameObject.SetActive(true);
        editorState.gui.Pop();
    }
}