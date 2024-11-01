using System.Collections.Generic;
using System.IO;

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

            var commands = SaveGame.CommandEmitter.Emit(level);
            var stringWriter = new StringWriter();
            SaveGame.TextFormatter.Format(stringWriter, commands);
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