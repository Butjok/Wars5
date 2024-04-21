using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Butjok.CommandLine;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Assertions;

public class Game : MonoBehaviour {

    public HoleShaderUpdater holeShaderUpdater;

    [Command]
    public static float TimeScale {
        get => Time.timeScale;
        set => Time.timeScale = value;
    }

    public Color colorPlain = new(.5f, 1, 0);
    public Color colorRoad = new(1, .66f, 0);
    public Color colorSea = new(0, .25f, 1);
    public Color colorMountain = new(0.75f, 0.5f, 0.25f);
    public Color colorForest = new(0f, 0.66f, 0f);
    public Color colorRiver = new(0, .5f, 1);

    [Command] public Color holeUnownedColor = new(0.24f, 0.13f, 0f);

    public Color GetColor(TileType tileType, Building building) {
        if (TileType.Buildings.HasFlag(tileType))
            return building?.Player?.Color ?? Color.white;

        return tileType switch {
            TileType.Plain => colorPlain,
            TileType.Road => colorRoad,
            TileType.Sea => colorSea,
            TileType.Mountain => colorMountain,
            TileType.Forest => colorForest,
            TileType.River => colorRiver,
            _ => Color.red
        };
    }

    [Command]
    public void ClearSaveData() {
        var persistentData = stateMachine.Find<GameSessionState>().persistentData;
        foreach (var mission in persistentData.campaign.Missions)
            mission.saves.Clear();
        persistentData.Write();
    }

    [Command]
    public void AddRandomSaveData() {
        var gameSessionState = stateMachine.Find<GameSessionState>();
        var campaign = gameSessionState.persistentData.campaign;
        var mission = campaign.Missions.Random();
        mission.saves.Add(new SavedMission {
            mission = mission,
            dateTimeUtc = DateTime.UtcNow,
            input = "Hello World",
            Screenshot = Resources.Load<Texture2D>("NatalieHappy")
        });
    }

    [Command]
    public void SavePersistentData() {
        stateMachine.Find<GameSessionState>().persistentData.Write();
    }

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
                commandLineGui.guiSkin = "CommandLine1".LoadAs<GUISkin>();
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
    public bool TryDequeueCommand(out (object name, object argument) command, [ CallerMemberName]  string callerMemberName=null ) {
        if (commands.TryDequeue(out command)) {
            Debug.Log($"Command ({command}) was dequeued by {callerMemberName}.");
            commandsDebugInfo.Dequeue();
            return true;
        }

        return false;
    }

    public AiPlayerCommander aiPlayerCommander;

    private void Awake() {
        aiPlayerCommander = gameObject.AddComponent<AiPlayerCommander>();
        aiPlayerCommander.game = this;
    }

    private void Start() {
        //stateMachine.Push(new GameSessionState(this));
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

        if (!holeShaderUpdater) {
            var prefab = "HoleShaderUpdater".LoadAs<HoleShaderUpdater>();
            holeShaderUpdater = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            DontDestroyOnLoad(holeShaderUpdater.gameObject);
        }

        if (holeShaderUpdater) {
            Level level = null;
            if (stateMachine.TryFind<LevelSessionState>() is { } levelSessionState)
                level = levelSessionState.level;
            else if (stateMachine.TryFind<LevelEditorSessionState>() is { } levelEditorSessionState)
                level = levelEditorSessionState.level;

            if (level != null) {
                newHoles.Clear();
                newHoles.UnionWith(level.units.Values.Select(u => u.view.body.transform.position.ToVector2Int()));
                //newHoles.IntersectWith(level.buildings.Keys);

                oldHoles.SymmetricExceptWith(newHoles);
                if (oldHoles.Count > 0) {
                    holeMask = holeShaderUpdater.UpdateTexture(newHoles.Select(position => {
                        level.buildings.TryGetValue(position, out var levelBuilding);
                        var color = levelBuilding?.Player?.Color ?? holeUnownedColor;
                        return (position, color);
                    }));
                    foreach (var building in level.buildings.Values)
                        building.view.EnableInteriorLights = newHoles.Contains(building.position);
                }

                oldHoles.Clear();
                oldHoles.UnionWith(newHoles);
            }

            else if (holeShaderUpdater.texture) {
                holeShaderUpdater.ResetTexture();
                holeMask = null;
            }
        }
    }

    public HashSet<Vector2Int> newHoles = new();
    public HashSet<Vector2Int> oldHoles = new();
    public Texture2D holeMask;

    private void OnApplicationQuit() {
        stateMachine.Pop(all: true);
    }

    [Command] public static int guiDepth = -1000;
    private List<string> stateNames = new();
    [Command] public bool showAllUnitBrainStates;
    [Command] public float unitBrainStateFontScale = 1;
    [Command] public int statesFontSize = 9;

    [Command]
    public bool ShowStates {
        get => PlayerPrefs.GetInt(nameof(ShowStates), 0) != 0;
        set => PlayerPrefs.SetInt(nameof(ShowStates), value ? 1 : 0);
    }

    [Command]
    public bool ShowCursorPosition {
        get =>  PlayerPrefs.GetInt(nameof(ShowCursorPosition), 0) != 0;
        set => PlayerPrefs.SetInt(nameof(ShowCursorPosition), value ? 1 : 0);
    }

    public GUIStyle statesLabelStyle;

    private readonly Dictionary<object, Action> guiCommands = new();
    public void SetGui(object key, Action action) {
        guiCommands[key] = action;
    }
    public void RemoveGui(object key) {
        guiCommands.Remove(key);
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUI.depth = guiDepth;

        if (ShowStates) {
            stateNames.Clear();
            stateNames.AddRange(stateMachine.StateNames);
            stateNames.Reverse();
            statesLabelStyle ??= new GUIStyle(GUI.skin.label) {
                fontSize = statesFontSize
            };
            var sb = new StringBuilder();
            for (var i = 0; i < stateNames.Count; i++) {
                if (i != 0)
                    sb.Append(" / ");
                sb.Append(stateNames[i]);
            }

            var text = sb.ToString();
            var size = statesLabelStyle.CalcSize(new GUIContent(text));
            var rect = new Rect(0, Screen.height - size.y, size.x, size.y);
            GUI.Label(rect, text, statesLabelStyle);
        }

        if (ShowCursorPosition && Camera.main.TryPhysicsRaycast(out Vector3 hit)) {
            var mousePosition =  Input.mousePosition;
            var labelPosition = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            var text = hit.ToVector2Int().ToString();
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            var rect = new Rect(labelPosition, size);
            GUI.Label(rect, text);
        }

        foreach (var action in guiCommands.Values)
            action();
    }
}

public static class GameDebug {
    public static T FindState<T>() where T : StateMachineState {
        var state = Game.Instance.stateMachine.TryFind<T>();
        Assert.IsNotNull(state);
        return state;
    }
}