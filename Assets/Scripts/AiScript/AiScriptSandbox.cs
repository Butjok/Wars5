using Butjok.CommandLine;
using UnityEngine;

public class AiScriptSandbox : MonoBehaviour {

    private  readonly AiScriptInterpreter interpreter = new();
    public GUISkin skin;
    public string text="";
    
    private void OnGUI() {
        GUI.skin = skin;
        //GUIUtility.
        //text=GUILayout.TextArea(text);
        //GUILayout.Button("EXECUTE");
    }

    [Command]
    private void Execute(string input) {
        Debug.Log(interpreter.Evaluate(input));
    }
    [Command]
    private void ClearEnvironment() {
        interpreter.environment.Clear();
    }
}