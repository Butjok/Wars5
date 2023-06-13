using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class Game : MonoBehaviour {

    public const bool createCommandLineGui = true;

    private static Game instance;
    public static Game Instance {
        get {
            if (instance)
                return instance;

            var instances = FindObjectsOfType<Game>();
            Assert.IsTrue(instances.Length is 0 or 1);
            if (instances.Length == 1)
                return instance = instances[0];

            var go = new GameObject(nameof(Game));
            DontDestroyOnLoad(go);
            instance = go.AddComponent<Game>();

            if (createCommandLineGui && !FindObjectOfType<CommandLineGUI>()) {
                var commandLineGui = instance.gameObject.AddComponent<CommandLineGUI>();
                commandLineGui.assemblies = new List<string> { "CommandLine", "Wars", "Stable" };
                commandLineGui.guiSkin = DefaultGuiSkin.TryGet;
                commandLineGui.Theme = "Default";
                commandLineGui.depth = -2000;
                commandLineGui.FetchCommands();
            }

            return instance;
        }
    }

    public readonly StateMachine stateMachine = new();

    private Queue<(object name, object argument)> commands = new();
    public void EnqueueCommand(object name, object argument = null) {
        Assert.IsTrue(commands.Count < 100);
        commands.Enqueue((name, argument));
    }
    public bool TryDequeueCommand(out (object name, object argument) command) {
        return commands.TryDequeue(out command);
    }

    public AiPlayerCommander aiPlayerCommander;

    private void Awake() {
        aiPlayerCommander = gameObject.AddComponent<AiPlayerCommander>();
        aiPlayerCommander.game = this;
    }

    [TextArea(10, 20)] public string states;
    private void Update() {
        stateMachine.Tick();
    }

    private void OnApplicationQuit() {
        var editorSessionState = stateMachine.TryFind<LevelEditorSessionState>();
        if (editorSessionState != null)
            LevelEditorFileSystem.Save("autosave", editorSessionState.level);
    }

    [Command] public static int guiDepth = -1000;
    private List<string> stateNames = new();
    private void OnGUI() {
        if (Debug.isDebugBuild) {
            GUI.skin = DefaultGuiSkin.TryGet;
            GUI.depth = guiDepth;
            stateNames.Clear();
            stateNames.AddRange(stateMachine.StateNames);
            stateNames.Reverse();
            GUILayout.BeginHorizontal();
            for (var i = 0; i < stateNames.Count; i++) {
                if (i != 0)
                    GUILayout.Label("/");
                GUILayout.Label(stateNames[i]);
            }
            GUILayout.EndHorizontal();
        }
    }
}

public static class GameDebug {
    public static T FindState<T>() where T:StateMachineState {
        var state= Game.Instance.stateMachine.TryFind<T>();
        Assert.IsNotNull(state);
        return state;
    }
}