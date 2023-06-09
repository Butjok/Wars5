using System;
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
        commands.Enqueue((name, argument));
    }
    public bool TryDequeueCommand(out (object name, object argument) command) {
        return commands.TryDequeue(out command);
    }

    public AiPlayerCommander aiPlayerCommander;
    [Command] public bool autoplay = false;

    private void Awake() {
        aiPlayerCommander = new AiPlayerCommander(this);
    }

    [TextArea(10,20)]public string states;
    private void Update() {
        stateMachine.Tick();
        states = string.Join("\n", stateMachine.StateNames);
        
        if (Input.GetKeyDown(KeyCode.Alpha8))
            autoplay = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    [Command] public static int guiDepth = -1000;
    private void OnGUI() {
        if (Debug.isDebugBuild) {
            GUI.skin = DefaultGuiSkin.TryGet;
            GUI.depth = guiDepth;
            GUILayout.Label(string.Join(" / ", stateMachine.StateNames.Reverse().Select(name => name.EndsWith("State") ? name[..^"State".Length] : name)));
        }
    }
}