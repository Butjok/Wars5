using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class LevelSessionState : StateMachineState {

    public struct StartParameters {
        public string sceneName;
        public string input;
    }

    public Level level;
    public SavedMission savedMission;
    public bool autoplay;
    public IEnumerator autoplayHandler;
    //public Dictionary<(MoveType, Vector2Int, Vector2Int), int> precalculatedDistances;

    public LevelSessionState(StateMachine stateMachine, SavedMission savedMission) : base(stateMachine) {
        this.savedMission = savedMission;
        //  this.precalculatedDistances = precalculatedDistances;
    }

    public override IEnumerator<StateChange> Enter {
        get {

            QualitySettings.shadowCascades = 0;
            
            var stringReader = new StringReader(savedMission.input);
            var commands = SaveGame.TextFormatter.Parse(stringReader);
            level = SaveGame.Loader.Load<Level>(commands);
            
            if (savedMission.mission != null && SceneManager.GetActiveScene().name != savedMission.mission.SceneName)
                SceneManager.LoadScene(savedMission.mission.SceneName);
            while (!LevelView.TryInstantiatePrefab(out level.view))
                yield return StateChange.none;
            
            level.SpawnActors();

            autoplayHandler = AutoplayHandler();
            stateMachine.Find<GameSessionState>().game.StartCoroutine(autoplayHandler);

            if (level.view.turnButton) {
                level.view.turnButton.Visible = true;
                level.view.turnButton.button.onClick.AddListener(() => stateMachine.Find<GameSessionState>().game.EnqueueCommand(SelectionState.Command.EndTurn));
            }

            var persistentData = stateMachine.Find<GameSessionState>().persistentData;
            var campaign = persistentData.campaign;

            if (stateMachine.TryFind<LevelEditorSessionState>() == null) {
                if (level.mission == campaign.tutorial) {
                    if (true) {

                        level.view.cameraRig.enabled = false;

                        Vector2Int startPosition = new(-21, 12);
                        Vector2Int fromPosition = new(-19, 3);
                        var speed = 2f;

                        IEnumerator unitMoveAnimation = null;
                        if (level.TryGetUnit(startPosition, out var unit)) {
                            var pathBuilder = new PathBuilder(fromPosition);
                            pathBuilder.AddRange(Woo.Traverse2D(fromPosition, startPosition));
                            unitMoveAnimation = new MoveSequence(unit.view.transform, pathBuilder, _speed: speed, _finalDirection: unit.view.LookDirection).Animation();
                        }

                        var zoomFadeAnimation = CameraAnimation.ZoomFadeAnimation(level.view.cameraRig.camera, 4);
                        while (true) {
                            var isPlaying = false;
                            if (unitMoveAnimation != null)
                                isPlaying = unitMoveAnimation.MoveNext() || isPlaying;
                            isPlaying = zoomFadeAnimation.MoveNext() || isPlaying;
                            if (!isPlaying)
                                break;
                            yield return StateChange.none;
                        }

                        //yield return StateChange.Push(new TutorialStartDialogue(stateMachine));

                        level.view.cameraRig.enabled = true;
                    }
                }
            }

            yield return StateChange.Push(new PlayerTurnState(stateMachine));
            //yield return levelLogic.OnLevelEnd(this);
        }
    }

    public override void Exit() {
        stateMachine.Find<GameSessionState>().game.StopCoroutine(autoplayHandler);
        level.DespawnActors();
        if (level.mission!=null && SceneManager.GetActiveScene().name == level.mission.SceneName)
            SceneManager.UnloadSceneAsync(level.mission.SceneName);
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