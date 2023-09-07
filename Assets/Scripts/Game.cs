using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
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

    public Level Level {
        get {
            var level = stateMachine.TryFind<LevelSessionState>()?.level ?? stateMachine.TryFind<LevelEditorSessionState>()?.level;
            Assert.IsNotNull(level);
            return level;
        }
    }

    [Command]
    public void FillTest() {
        var level = Level;
        var tiles = level.tiles.Keys;
        var vehicleReachable = tiles.Where(position => Rules.TryGetMoveCost(MoveType.Tracks, level.tiles[position], out _)).ToHashSet();
        var enemies = level.units.Keys.Where(position => level.units[position].Player.Color.r < .1f).ToHashSet();
        level.view.tilemapCursor.isValidPosition = position => vehicleReachable.Contains(position) && !enemies.Contains(position);
    }

    public readonly StateMachine stateMachine = new();

    private Queue<(object name, object argument)> commands = new();
    private Queue<(string callerMemberName, string callerFilePath, int callerLineNumber)> commandsDebugInfo = new();

    public void EnqueueCommand(object name, object argument = null,
        [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) {
        Assert.IsTrue(name != null);
        Assert.IsTrue(commands.Count < 100);
        commands.Enqueue((name, argument));
        commandsDebugInfo.Enqueue((callerMemberName, callerFilePath, callerLineNumber));
    }
    public bool TryDequeueCommand(out (object name, object argument) command) {
        if (commands.TryDequeue(out command)) {
            commandsDebugInfo.Dequeue();
            return true;
        }
        return false;
    }
    public bool TryDequeueCommand(out (object name, object argument) command, out (string callerMemberName, string callerFilePath, int callerLineNumber) debugInfo) {
        if (commands.TryDequeue(out command)) {
            debugInfo = commandsDebugInfo.Dequeue();
            return true;
        }
        debugInfo = default;
        return false;
    }

    public AiPlayerCommander aiPlayerCommander;

    private void Awake() {
        aiPlayerCommander = gameObject.AddComponent<AiPlayerCommander>();
        aiPlayerCommander.game = this;
    }

    private void Update() {

        stateMachine.Tick();

        if (Debug.isDebugBuild) {
            var level = stateMachine.TryFind<LevelSessionState>()?.level ?? stateMachine.TryFind<LevelEditorSessionState>()?.level;
            if (level != null && level.view) {
                var units = new HashSet<Unit>();
                if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition) && level.TryGetUnit(mousePosition, out var unit))
                    units.Add(unit);
                if (showAllUnitBrainStates)
                    foreach (var u in level.units.Values)
                        units.Add(u);
                foreach (var u in units)
                    DebugDraw.Unit(u);
            }
        }
    }

    private void OnApplicationQuit() {
        var editorSessionState = stateMachine.TryFind<LevelEditorSessionState>();
        if (editorSessionState != null) {
            LevelEditorFileSystem.Save("autosave", editorSessionState.level);
            editorSessionState.SaveTerrainMesh();
            // PrecalculatedDistances.CalculateAndWrite(editorSessionState.level.tiles, editorSessionState.level.missionName);
        }
    }

    [Command] public static int guiDepth = -1000;
    private List<string> stateNames = new();
    [Command] public bool showAllUnitBrainStates;
    [Command] public float unitBrainStateFontScale = 1;
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

    [Command]
    public void LoadPrecalculatedDistances() {
        new Thread(() => {
            PrecalculatedDistances.TryLoad(Level.missionName, out Level.precalculatedDistances);
            Debug.Log("Loaded");
        }).Start();
    }
}

public static class GameDebug {
    public static T FindState<T>() where T : StateMachineState {
        var state = Game.Instance.stateMachine.TryFind<T>();
        Assert.IsNotNull(state);
        return state;
    }
}