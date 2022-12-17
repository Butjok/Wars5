using Butjok.CommandLine;
using UnityEngine;

public static class AiScriptSandbox {

    private static readonly AiScriptInterpreter interpreter = new();

    [Command]
    private static void Execute(string input) {
        Debug.Log(interpreter.Evaluate(input));
    }
    [Command]
    private static void ClearEnvironment() {
        interpreter.environment.Clear();
    }
}