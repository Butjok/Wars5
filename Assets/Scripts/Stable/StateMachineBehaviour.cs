using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * a prototype with trampolining IEnumerators
 * basic idea is to remove unnecessary coroutine Push
 * similar idea to tail call or trampoline
 */

public class StateMachineBehaviour : MonoBehaviour {

    public Stack<IEnumerator> states = new();

    public void StartState(IEnumerator state) {
        states.Push(state);
    }

    private void Update() {
        if (states.Count > 0) {
            var state = states.Peek();
            if (state.MoveNext()) {
                var value = state.Current;
                switch (value) {
                    case null:
                        break;
                    case IEnumerator subState:
                        states.Push(subState);
                        break;
                    case ReplaceWith replaceWith:
                        states.Pop();
                        states.Push(replaceWith.state);
                        break;
                }
            }
            else
                states.Pop();
        }
    }

    private void Start() {
        StartState(SelectionState());
    }

    public IEnumerator SelectionState() {
        Debug.Log("selection state started");
        yield return WelcomeState();
        while (true) {
            yield return null;
            if (Input.GetKeyDown(KeyCode.Space))
                yield return new ReplaceWith(PathSelectionState());
        }
        Debug.Log("selection state ended");
    }
    public IEnumerator WelcomeState() {
        Debug.Log("WELCOME");
        Debug.Log("Press any key");
        yield return new WaitForSeconds(1);
        Debug.Log("WELCOME ended");
    }
    public IEnumerator PathSelectionState() {
        Debug.Log("in path selection state now");
        yield break;
    }
}

public class ReplaceWith {
    public IEnumerator state;
    public ReplaceWith(IEnumerator state) {
        this.state = state;
    }
}