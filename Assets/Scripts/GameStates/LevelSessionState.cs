using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class LevelSessionState : StateMachineState {

    public Level level;
    public string input;
    public MissionName missionName;
    public bool isFreshStart;
    public LevelLogic levelLogic;
    public bool autoplay;
    public IEnumerator autoplayHandler;
    public Dictionary<(MoveType, Vector2Int, Vector2Int), int> precalculatedDistances;

    public LevelSessionState(StateMachine stateMachine, string input, MissionName missionName, bool isFreshStart, Dictionary<(MoveType, Vector2Int, Vector2Int), int> precalculatedDistances = null) : base(stateMachine) {
        this.input = input;
        this.missionName = missionName;
        this.isFreshStart = isFreshStart;
        levelLogic = missionName switch {
            MissionName.Tutorial => new TutorialLevelLogic(),
            _ => new LevelLogic()
        };
        levelLogic = new TutorialLevelLogic();
        this.precalculatedDistances = precalculatedDistances;
    }

    public override IEnumerator<StateChange> Enter {
        get {
            level = new Level { missionName = missionName };
            if (precalculatedDistances != null)
                level.precalculatedDistances = precalculatedDistances;
            else
                new Thread(() => PrecalculatedDistances.TryLoad(level.missionName, out level.precalculatedDistances)).Start();

            LevelView.TryLoadScene(level.missionName);
            // give one extra frame to load the scene
            yield return StateChange.none;
            Assert.IsTrue(LevelView.TryInstantiatePrefab(out level.view));
            LevelReader.ReadInto(level, input.ToPostfix());

            autoplayHandler = AutoplayHandler();
            FindState<GameSessionState>().game.StartCoroutine(autoplayHandler);

            /*if (stateMachine.TryFind<LevelEditorSessionState>() == null) {
                var tilemapMesh = Resources.Load<Mesh>("TilemapMeshes/" + level.missionName);
                var tilemapMaterial = "EditorTileMap".LoadAs<Material>();
                if (tilemapMesh && tilemapMaterial) {
                    var gameObject = new GameObject("TilemapMesh");
                    gameObject.transform.SetParent(level.view.transform);
                    gameObject.layer = LayerMask.NameToLayer("Terrain");
                    var meshFilter = gameObject.AddComponent<MeshFilter>();
                    var meshRenderer = gameObject.AddComponent<MeshRenderer>();
                    var meshCollider = gameObject.AddComponent<MeshCollider>();
                    meshFilter.sharedMesh = tilemapMesh;
                    meshCollider.sharedMesh = tilemapMesh;
                    meshCollider.convex = false;
                    meshRenderer.sharedMaterial = tilemapMaterial;
                }
            }*/

            if (level.view.turnButton) {
                level.view.turnButton.Visible = true;
                level.view.turnButton.button.onClick.AddListener(() => FindState<GameSessionState>().game.EnqueueCommand(SelectionState.Command.EndTurn));
            }

            //yield return levelLogic.OnLevelStart(this);

            var persistentData = stateMachine.Find<GameSessionState>().persistentData;

            if (missionName == MissionName.Tutorial) {
                if (persistentData.showIntroDialogue) {

                    Vector2Int startPosition = new(-21, 12);
                    Vector2Int fromPosition = new(-19, 3);
                    var speed = 2f;

                    IEnumerator unitMoveAnimation = null;
                    if (level.TryGetUnit(startPosition, out var unit)) {
                        var pathBuilder = new PathBuilder(fromPosition);
                        pathBuilder.AddRange(Woo.Traverse2D(fromPosition, startPosition));
                        unitMoveAnimation = new MoveSequence(unit.view.transform, pathBuilder, _speed: speed, _finalDirection: unit.view.LookDirection).Animation();
                    }

                    var zoomFadeAnimation = CameraRigAnimation.ZoomFadeAnimation(level.view.cameraRig, 4);
                    while (true) {
                        var isPlaying = false;
                        if (unitMoveAnimation != null)
                            isPlaying = unitMoveAnimation.MoveNext() || isPlaying;
                        isPlaying = zoomFadeAnimation.MoveNext() || isPlaying;
                        if (!isPlaying)
                            break;
                        yield return StateChange.none;
                    }

                    yield return StateChange.Push(new TutorialStartDialogue(stateMachine));
                }
            }

            yield return StateChange.Push(new PlayerTurnState(stateMachine));
            //yield return levelLogic.OnLevelEnd(this);
        }
    }

    public override void Exit() {
        FindState<GameSessionState>().game.StopCoroutine(autoplayHandler);
        level.Dispose();
        LevelView.TryUnloadScene(level.missionName);
        Object.Destroy(level.view.gameObject);
        level.view = null;
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
}