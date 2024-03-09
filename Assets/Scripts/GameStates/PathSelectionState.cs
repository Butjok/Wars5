using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class PathSelectionState : StateMachineState {

    public enum Command { Cancel, Move, ReconstructPath, AppendToPath }

    public static GameObject pathMeshGameObject;
    public static MeshFilter pathMeshFilter;
    public static MeshRenderer pathMeshRenderer;

    public Vector2Int initialLookDirection;
    public List<Vector2Int> path = new();

    public PathSelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var levelSession = stateMachine.Find<LevelSessionState>();
            var (game, level, unit) = (stateMachine.Find<GameSessionState>().game, levelSession.level, stateMachine.Find<SelectionState>().unit);

            var unitPosition = unit.NonNullPosition;
            Assert.IsTrue(level.tiles.ContainsKey(unitPosition));

            var pathFinder = new PathFinder();
            pathFinder.FindMoves(unit);
            var reachable = pathFinder.movePositions;

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

            if (!levelSession.autoplay) 
                TileMask.ReplaceGlobal(reachable);

            void RebuildPathMesh() {
                if (levelSession.autoplay)
                    return;
                pathMeshFilter.sharedMesh = MoveSequenceMeshBuilder.Build(
                    pathMeshFilter.sharedMesh,
                    new MoveSequence(unit.view.transform, pathBuilder),
                    nameof(MoveSequenceAtlas).LoadAs<MoveSequenceAtlas>());
            }

            unit.view.Selected = true;
            initialLookDirection = unit.view.LookDirection;

            var issuedAiCommands = false;
            while (true) {
                yield return StateChange.none;

                if (levelSession.autoplay) {
                    if (!issuedAiCommands) {
                        issuedAiCommands = true;
                        game.aiPlayerCommander.IssueCommandsForPathSelectionState();
                    }
                }
                else if (!level.CurrentPlayer.IsAi) {
                    if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                        game.EnqueueCommand(Command.Cancel);

                    else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {

                        if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition) && reachable.Contains(mousePosition)) {
                            if (pathBuilder.Last() == mousePosition)
                                game.EnqueueCommand(Command.Move);
                            else
                                game.EnqueueCommand(Command.ReconstructPath, mousePosition);
                        }
                        else
                            UiSound.Instance.notAllowed.PlayOneShot();
                    }
                }

                while (game.TryDequeueCommand(out var command))
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

                            TileMask.UnsetGlobal();

                            path = pathBuilder.ToList();
                            var animation = new MoveSequence(unit.view.transform, path).Animation();

                            level.view.tilemapCursor.Hide();
                            
                            while (animation.MoveNext()) {
                                yield return StateChange.none;

                                if (Input.GetMouseButtonDown(Mouse.left) || Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Space) ||
                                    Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape)) {
                                    unit.view.Position = path[^1];
                                    if (path.Count >= 2)
                                        unit.view.LookDirection = path[^1] - path[^2];
                                    break;
                                }
                            }

                            yield return StateChange.Push(new ActionSelectionState(stateMachine));
                            break;
                        }

                        case (Command.Cancel, _):
                            unit.view.Selected = false;
                            TileMask.UnsetGlobal();
                            yield return StateChange.PopThenPush(2, new SelectionState(stateMachine));
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
                
                level.UpdateTilemapCursor();
            }
        }
    }

    public override void Exit() {
        var level = stateMachine.TryFind<LevelSessionState>().level;
        var unit = stateMachine.TryFind<SelectionState>().unit;
        TileMask.UnsetGlobal();
        unit.view.Selected = false;
        if (pathMeshGameObject)
            pathMeshGameObject.SetActive(false);
    }
}