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
        commands.Enqueue((name, argument));
    }
    public bool TryDequeueCommand(out (object name, object argument) command) {
        return commands.TryDequeue(out command);
    }

    public AiPlayerCommander aiPlayerCommander;
    public bool autoplay;

    private void Awake() {
        aiPlayerCommander = gameObject.AddComponent<AiPlayerCommander>();
        aiPlayerCommander.game = this;
    }

    private void Start() {
        StartCoroutine(AutoplayHandler());
    }

    [TextArea(10, 20)] public string states;
    private void Update() {
        stateMachine.Tick();
    }

    private void OnApplicationQuit() {
        stateMachine.Pop(all:true);
    }

    public IEnumerator AutoplayHandler() {
        const KeyCode key = KeyCode.Alpha8;
        while (true) {
            yield return null;
            if (Input.GetKeyDown(key)) {
                autoplay = true;
                var onHoldKey = !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
                yield return null;
                while (onHoldKey ? !Input.GetKeyUp(key) : !Input.GetKeyDown(key))
                    yield return null;
                yield return null;
                autoplay = false;
            }
        }
    }

    [Command] public static int guiDepth = -1000;
    private List<string> stateNames = new();
    private void OnGUI() {
        if (Debug.isDebugBuild) {
            GUI.skin = DefaultGuiSkin.TryGet;
            GUI.depth = guiDepth;
            GUILayout.BeginHorizontal();
            stateNames.Clear();
            stateNames.AddRange(stateMachine.StateNames);
            stateNames.Reverse();
            for (var i = 0; i < stateNames.Count; i++) {
                if (i != 0)
                    GUILayout.Label("/");
                GUILayout.Label(stateNames[i]);
            }
            GUILayout.EndHorizontal();
        }
    }
}