using System.Collections;
using UnityEngine;

public class LoadMap : MonoBehaviour {
    public string saveName = "Test";
    public bool start = false;
    public void Start() {
        StartCoroutine(Load());
    }
    public IEnumerator Load() {
        while (Game.Instance.stateMachine.TryFind<LevelEditorSessionState>() == null)
            yield return null;
        LevelEditorFacade.TryLoad(saveName);
        if (start)
            Game.Instance.EnqueueCommand(LevelEditorSessionState.SelectModeCommand.Play);
    }
}