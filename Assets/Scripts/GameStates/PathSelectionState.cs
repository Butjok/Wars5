using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class PathSelectionState : StateMachineState {

    public enum Command {
        Cancel,
        Move,
        ReconstructPath,
        AppendToPath
    }

    public static GameObject pathMeshGameObject;
    public static MeshFilter pathMeshFilter;
    public static MeshRenderer pathMeshRenderer;

    public Vector2Int initialLookDirection;
    public List<Vector2Int> path = new();

    public PathSelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var levelSession = stateMachine.Find<LevelSessionState>();
            var level = levelSession.level;
            var unit = stateMachine.Find<SelectionState>().unit;

            var unitPosition = unit.NonNullPosition;
            Assert.IsTrue(Level.tiles.ContainsKey(unitPosition));

            var pathFinder = new PathFinder();
            pathFinder.FindShortPaths(unit, allowStayOnFriendlyUnits: true);
            var reachable = pathFinder.validShortPathDestinations;

            if (!pathMeshGameObject) {
                pathMeshGameObject = new GameObject("MovePathMesh");
                Object.DontDestroyOnLoad(pathMeshGameObject);
                pathMeshFilter = pathMeshGameObject.AddComponent<MeshFilter>();
                pathMeshRenderer = pathMeshGameObject.AddComponent<MeshRenderer>();
                pathMeshRenderer.sharedMaterial = "MovePath".LoadAs<Material>();
            }

            pathMeshGameObject.SetActive(true);

            if (pathMeshFilter.sharedMesh) {
                Object.Destroy(pathMeshFilter.sharedMesh);
                pathMeshFilter.sharedMesh = null;
            }

            var pathBuilder = new PathBuilder(unitPosition);

            if (!Game.Instance.dontShowMoveUi && level.view.tileMapMeshGenerator) {
                level.view.tileMapMeshGenerator.UseAttackColor = false;
                level.view.tileMapMeshGenerator.Rebuild(reachable);
            }

            void RebuildPathMesh() {
                if (Game.Instance.dontShowMoveUi)
                    return;
                pathMeshFilter.sharedMesh = MoveSequenceMeshBuilder.Build(
                    pathMeshFilter.sharedMesh,
                    new MoveSequence(unit.view.transform, pathBuilder),
                    nameof(MoveSequenceAtlas).LoadAs<MoveSequenceAtlas>());
            }

            unit.view.Selected = true;
            initialLookDirection = unit.view.LookDirection;

            if (Level.EnableTutorial && Level.name == LevelName.Tutorial) {
                if (!Level.tutorialState.startedCapturing && !Level.tutorialState.askedToCaptureBuilding) {
                    Level.tutorialState.askedToCaptureBuilding = true;
                    yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.PleaseCaptureBuilding));
                }

                if (unit.type == UnitType.Apc && !Level.tutorialState.explainedApc) {
                    Level.tutorialState.explainedApc = true;
                    yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.ExplainApc));
                }
            }

            while (true) {
                yield return StateChange.none;

                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    Game.EnqueueCommand(Command.Cancel);

                else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
                    if (Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition) && reachable.Contains(mousePosition)) {
                        if (pathBuilder.Last() == mousePosition)
                            Game.EnqueueCommand(Command.Move);
                        else
                            Game.EnqueueCommand(Command.ReconstructPath, mousePosition);
                    }
                    else
                        UiSound.Instance.notAllowed.PlayOneShot();
                }

                while (Game.TryDequeueCommand(out var command)) {
                    // Tutorial logic
                    if (Level.EnableTutorial && Level.name == LevelName.Tutorial)
                        if (!Level.tutorialState.startedCapturing)
                            switch (command) {
                                case (Command.ReconstructPath or Command.AppendToPath or Command.Cancel, _):
                                    break;
                                case (Command.Move, _):
                                    if (Level.TryGetBuilding(pathBuilder[^1], out var building) && building.Player != Level.localPlayer && building.type == TileType.Factory)
                                        break;
                                    goto default;
                                default:
                                    yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.WrongPathSelectionPleaseMoveToBuilding));
                                    continue;
                            }

                    switch (command) {
                        case (Command.ReconstructPath, Vector2Int targetPosition): {
                            if (pathFinder.TryFindPath(out var path, out _, target: targetPosition)) {
                                pathBuilder.Clear();
                                foreach (var position in path.Skip(1))
                                    pathBuilder.Add(position);
                                RebuildPathMesh();
                            }

                            break;
                        }

                        case (Command.AppendToPath, Vector2Int position):
                            pathBuilder.Add(position);
                            RebuildPathMesh();
                            break;

                        case (Command.Move, _): {
                            pathMeshGameObject.SetActive(false);
                            //TileMask.UnsetGlobal();

                            if (level.view.tileMapMeshGenerator)
                                level.view.tileMapMeshGenerator.Clear();

                            path = pathBuilder.ToList();
                            var animation = new MoveSequence(unit.view.transform, path).Animation();

                            var mineFieldsAlongTheWay = new Queue<MineField>();
                            foreach (var position in path)
                                if (level.mineFields.TryGetValue(position, out var mineField))
                                    mineFieldsAlongTheWay.Enqueue(mineField);

                            Level.view.tilemapCursor.Hide();

                            var cancel = false;
                            var startUnitLookDirection = unit.view.LookDirection;
                            
                            while (unit.Hp > 0 && animation.MoveNext()) {
                                yield return StateChange.none;

                                if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
                                    break;
                                }
                                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape)) {
                                    cancel = true;
                                    break;
                                }

                                if (mineFieldsAlongTheWay.TryPeek(out var mineField) && mineField.position == unit.view.Position) {
                                    mineFieldsAlongTheWay.Dequeue();
                                    if (unit.Hp > 0 && Rules.ShouldExplode(mineField, unit)) 
                                        mineField.Explode(unit);
                                }
                            }

                            if (cancel) {
                                unit.view.Position = path[0];
                                unit.view.LookDirection = startUnitLookDirection;
                                Game.EnqueueCommand(Command.Cancel);
                                break;
                            }

                            while (unit.Hp > 0 && mineFieldsAlongTheWay.TryDequeue(out var mineField))
                                if (unit.Hp > 0 && Rules.ShouldExplode(mineField, unit))
                                    mineField.Explode(unit);

                            if (unit.Hp > 0) {
                                unit.view.Position = path[^1];
                                if (path.Count >= 2)
                                    unit.view.LookDirection = path[^1] - path[^2];
                                yield return StateChange.Push(new ActionSelectionState(stateMachine));
                            }
                            else
                                yield return StateChange.PopThenPush(2, new SelectionState(stateMachine));
                            break;
                        }

                        case (Command.Cancel, _):
                            unit.view.Selected = false;
                            if (level.view.tileMapMeshGenerator)
                                level.view.tileMapMeshGenerator.Clear();
                            yield return StateChange.PopThenPush(2, new SelectionState(stateMachine));
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
                }

                UpdateTilemapCursor();
            }
        }
    }

    public override void Exit() {
        var unit = stateMachine.TryFind<SelectionState>().unit;
        //TileMask.UnsetGlobal();
        // the unit might get destroyed during the move
        if (unit != null && unit.view)
            unit.view.Selected = false;
        if (pathMeshGameObject)
            pathMeshGameObject.SetActive(false);

        var level = stateMachine.Find<LevelSessionState>().level;
        if (level.view.tileMapMeshGenerator)
            level.view.tileMapMeshGenerator.Clear();
    }
}