using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;
using static Gettext;
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
                    yield return StateChange.Push(new WaitForCoroutineState(stateMachine, CameraRigAnimation.ZoomFadeAnimation(level.view.cameraRig, 4)));
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

public class TutorialStartDialogue : DialogueState {
    public TutorialStartDialogue(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {

            var persistentData = stateMachine.Find<GameSessionState>().persistentData;

            Show();
            yield return AddPerson(PersonName.Natalie);
            yield return SayWait(_("Welcome to Wars3D!"));
            yield return SayWait(_("This is a strategy game and you are in charge!"));

            if (persistentData.playTutorial) {
                yield return Say(_("Do you want to watch tutorial?"));
                bool yes = default;
                yield return ChooseYesNo(value => yes = value);
                if (yes) { 
                    yield return Say(_("Sure thing!"));
                    yield return Wait(.5f);
                    yield return SayWait(_("Let us start with the basics!"));
                    var video = CreateVideo("encoded2".LoadAs<VideoClip>(), target: VideoPanelImage);
                    yield return ShowVideoPanel();
                    //yield return Wait(.5f);
                    video.player.playbackSpeed=1;
                    yield return WaitWhile(() => !video.Completed);
                    yield return SayWait(_("Now that you know the basics, let us get started!"));
                    yield return HideVideoPanel();
                    DestroyVideo(video);
                }
                else
                    yield return SayWait(_("Sure thing, let us get started!"));
            }

            yield return RemovePerson(PersonName.Natalie);
            Hide();
        }
    }
}