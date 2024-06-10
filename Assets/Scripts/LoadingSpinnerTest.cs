using Butjok.CommandLine;
using UnityEngine;

public class LoadingSpinnerTest : MonoBehaviour {
    
    public Transform start, finish;
    [Command]
    public float targetProgress;

    public float Progress => (transform.position - start.position).magnitude / (finish.position - start.position).magnitude;

    private void OnGUI() {
        GUILayout.Label($"Progress: {Progress}");
    }
}