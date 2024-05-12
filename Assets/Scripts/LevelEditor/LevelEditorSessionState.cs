using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class LevelEditorSessionState : StateMachineState {

    public const string sceneName = "MarchingSquares";

    public enum SelectModeCommand {
        SelectTilesMode,
        SelectUnitsMode,
        SelectTriggersMode,
        SelectAreasMode,
        SelectBridgesMode,
        SelectPropsMode,
        Play,
        SelectPathsMode
    }

    public static Vector3 tileMeshPosition => new(0, -.01f, 0);

    public Level level;
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
    public bool showLevelEditorTileMesh;
    public LevelEditorSessionState(StateMachine stateMachine, string input = null, bool showLevelEditorTileMesh = false) : base(stateMachine) {
        this.input = input;
        this.showLevelEditorTileMesh = showLevelEditorTileMesh;
    }

    public override IEnumerator<StateChange> Enter {
        get {
            QualitySettings.shadowCascades = 0;

            level = new Level();

            if (input != null) {
                var commands = SaveGame.TextFormatter.Parse(new StringReader(input));
                level = SaveGame.Loader.Load<Level>(commands);
            }
            else {
                var bluePlayer = new Player {
                    level = level,
                    ColorName = ColorName.Blue,
                    coName = PersonName.Natalie,
                };
                level.players.Add(bluePlayer);

                var redPlayer = new Player {
                    level = level,
                    ColorName = ColorName.Red,
                    coName = PersonName.Vladan,
                };
                level.players.Add(redPlayer);
            }

            if (SceneManager.GetActiveScene().name != sceneName)
                SceneManager.LoadScene(sceneName);
            while (!LevelView.TryInstantiatePrefab(out level.view))
                yield return StateChange.none;

            foreach (var p in level.players)
                p.Initialize();
            foreach (var b in level.buildings.Values)
                b.Initialize();
            foreach (var u in level.units.Values)
                u.Initialize();

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

                tileMeshRenderer.enabled = showLevelEditorTileMesh;
                tileMeshCollider.enabled = showLevelEditorTileMesh;
            }

            //ai = new Ai(FindState<GameSessionState>().game, level);

            var theme = Resources.Load<AudioClip>("violin uzicko");
            if (theme)
                musicSource = Music.CreateAudioSource(theme);

            yield return StateChange.Push(new LevelEditorUnitsModeState(stateMachine));
        }
    }

    [Command]
    public static void RemoveAllBuildings() {
        var game = Game.Instance;
        var level = game.stateMachine.Find<LevelEditorSessionState>().level;
        foreach (var building in level.buildings.Values.ToList())
            building.Dispose();
    }

    public override void Exit() {

        level.Dispose();
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