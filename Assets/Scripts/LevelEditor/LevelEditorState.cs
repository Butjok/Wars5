using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class LevelEditorState : StateMachine.State {

    public enum Command { SelectTilesMode, SelectUnitsMode, SelectTriggersMode, SelectBridgesMode, Play }

    public LevelView levelViewPrefab;
    public Level level = new();
    public MeshFilter tileMeshFilter;
    public MeshCollider tileMeshCollider;
    public LevelEditorGui gui;

    public int autosaveLifespanInDays = 30;

    public string input;
    public LevelEditorState(StateMachine stateMachine, string input = "") : base(stateMachine) {
        this.input = input;
    }

    public override IEnumerator<StateChange> Sequence {
        get {
            LevelView.TryLoadScene(level.missionName);
            level.view = LevelView.TryInstantiate();
            Assert.IsTrue(level.view);
            LevelReader.ReadInto(level, input);

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
                gameObject.layer = LayerMask.NameToLayer("Terrain");
                tileMeshFilter = gameObject.AddComponent<MeshFilter>();
                tileMeshCollider = gameObject.AddComponent<MeshCollider>();
                var tileMeshRenderer = gameObject.AddComponent<MeshRenderer>();
                tileMeshRenderer.sharedMaterial = "EditorTileMap".LoadAs<Material>();
            }

            yield return StateChange.Push(new LevelEditorTilesModeState(stateMachine));
        }
    }

    public override void Dispose() {

        LevelEditorFileSystem.Save("autosave", level);
        LevelEditorFileSystem.DeleteOldAutosaves(autosaveLifespanInDays);

        LevelView.TryUnloadScene(level.missionName);
        Object.Destroy(level.view.gameObject);
        level.view = null;

        Object.Destroy(gui.gameObject);
        Object.Destroy(tileMeshFilter.gameObject);

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
        while (stateMachine.TryPeek(out var state) && state is not LevelEditorState)
            stateMachine.Pop();
        stateMachine.Pop();
    }
    [Command]
    public static void Load(string name) {
        var input = LevelEditorFileSystem.TryReadLatest(name);
        Assert.IsNotNull(input);
        Close();
        var stateMachine = Game.Instance.stateMachine;
        stateMachine.Push(new LevelEditorState(stateMachine, input));
    }
}