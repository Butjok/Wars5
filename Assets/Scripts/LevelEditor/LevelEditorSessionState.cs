using System.Collections.Generic;
using System.Threading;
using Butjok.CommandLine;
using Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class LevelEditorSessionState : StateMachineState {

    public enum SelectModeCommand { SelectTilesMode, SelectUnitsMode, SelectTriggersMode, SelectAreasMode, SelectBridgesMode, Play }

    public static Vector3 tileMeshPosition => new(0, -.01f, 0);
    
    public LevelView levelViewPrefab;
    public Level level = new();
    public MeshFilter tileMeshFilter;
    public MeshCollider tileMeshCollider;
    public LevelEditorGui gui;
    public bool playAsFreshStart;

    [Command] public static bool PlayAsFreshStart {
        get => GameDebug.FindState<LevelEditorSessionState>().playAsFreshStart;
        set => GameDebug.FindState<LevelEditorSessionState>().playAsFreshStart = value;
    }

    public int autosaveLifespanInDays = 30;
    public AudioSource musicSource;

    public string input;
    public LevelEditorSessionState(StateMachine stateMachine, string input = "") : base(stateMachine) {
        this.input = input;
    }

    public override IEnumerator<StateChange> Enter {
        get {
            LevelView.TryLoadScene(level.missionName);
            Assert.IsTrue(LevelView.TryInstantiatePrefab(out level.view));
            LevelReader.ReadInto(level, input.ToPostfix());

            new Thread(() => PrecalculatedDistances.TryLoad(level.missionName, out level.precalculatedDistances)).Start();

            if (level.players.Count == 0) {
                level.players.Add(new Player(level, ColorName.Red));
                level.players.Add(new Player(level, ColorName.Blue));
            }

            {
                var gameObject = new GameObject("LevelEditorGui");
                gui = gameObject.AddComponent<LevelEditorGui>();
            }
            {
                var gameObject = new GameObject("LevelEditorTileMesh");
                gameObject.transform.position = tileMeshPosition;
                gameObject.layer = LayerMask.NameToLayer("Terrain");
                tileMeshFilter = gameObject.AddComponent<MeshFilter>();
                tileMeshCollider = gameObject.AddComponent<MeshCollider>();
                // gameObject.AddComponent<CursorInteractor>();
                var tileMeshRenderer = gameObject.AddComponent<MeshRenderer>();
                tileMeshRenderer.sharedMaterial = "EditorTileMap".LoadAs<Material>();
            }

            //ai = new Ai(FindState<GameSessionState>().game, level);

            var theme = Resources.Load<AudioClip>("violin uzicko");
            if (theme)
                musicSource = Music.Play(theme);

            yield return StateChange.Push(new LevelEditorTilesModeState(stateMachine));
        }
    }

    public void SaveTerrainMesh() {
        AssetDatabase.CreateAsset(tileMeshFilter.sharedMesh, "Assets/Resources/TilemapMeshes/" + level.missionName + ".asset");
    }

    public override void Exit() {

        LevelEditorFileSystem.Save("autosave", level);
        LevelEditorFileSystem.DeleteOldAutosaves(autosaveLifespanInDays);

        level.Dispose();
        LevelView.TryUnloadScene(level.missionName);
        Object.Destroy(level.view.gameObject);
        level.view = null;

        Object.Destroy(gui.gameObject);
        Object.Destroy(tileMeshFilter.gameObject);

        if (musicSource)
            Music.Kill(musicSource);
    }

    public void DrawBridges() {
        foreach (var bridge in level.bridges) {
            var index = level.bridges.IndexOf(bridge);
            if (bridge.tiles.Count > 0) {

                var center = Vector2.zero;
                var count = 0;
                foreach (var position in bridge.tiles.Keys) {
                    center += position;
                    count++;

                    Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, Color.white);
                }

                center /= count;
                Draw.ingame.Label2D(center.ToVector3(), $"Bridge{index}: {bridge.Hp}", 14, LabelAlignment.Center, Color.black);
            }
        }
    }

    [Command]
    public static void Close() {
        var stateMachine = Game.Instance.stateMachine;
        while (stateMachine.TryPeek(out var state) && state is not LevelEditorSessionState)
            stateMachine.Pop();
        stateMachine.Pop();
    }
    [Command]
    public static void Load(string name) {
        var input = LevelEditorFileSystem.TryReadLatest(name);
        Assert.IsNotNull(input);
        Close();
        var stateMachine = Game.Instance.stateMachine;
        stateMachine.Push(new LevelEditorSessionState(stateMachine, input));
    }
}