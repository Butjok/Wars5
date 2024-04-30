using System.Collections;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class BorderScenario2 : MonoBehaviour {

    public CameraRigFlythrough cameraRigFlythrough;

    public Game game;
    public StateMachine stateMachine;

    [Command]
    public void Play(string slotName) {
        StopAllCoroutines();
        StartCoroutine(Animation(slotName));
    }

    public IEnumerator Animation(string slotName) {
        /*var animation = cameraRigFlythrough.Animation(slotName);
        while (animation.MoveNext())
            yield return null;*/

        game = FindObjectOfType<Game>();
        Assert.IsTrue(game);
        stateMachine = game.stateMachine;
        
        stateMachine.Push(new BorderIncidentIntroDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentIntroDialogueState>() != null)
            yield return null;
        stateMachine.Push(new BorderIncidentRedRocketeersDialogueState(stateMachine));
    }

    public IEnumerator WaitForState<T>() where T : StateMachineState {
        while (!stateMachine.IsInState<T>())
            yield return null;
    }
}