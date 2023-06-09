using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;


public class PathSelectionState : StateMachine.State {

    public enum Command { Cancel, Move, ReconstructPath, AppendToPath }

    public GameObject pathMeshGameObject;
    public Vector2Int initialLookDirection;
    public List<Vector2Int> path = new();

    public PathSelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Sequence {
        get {
            var game = stateMachine.TryFind<GameSessionState>()?.game;
            var level = stateMachine.TryFind<PlayState>()?.level;
            var unit = stateMachine.TryFind<SelectionState>()?.unit;
            Assert.IsNotNull(game);
            Assert.IsNotNull(level);
            Assert.IsNotNull(unit);

            var unitPosition = unit.NonNullPosition;
            Assert.IsTrue(level.tiles.ContainsKey(unitPosition));

            var pathFinder = new PathFinder();
            pathFinder.FindStayMoves(unit);
            var reachable = pathFinder.movePositions;

            pathMeshGameObject = new GameObject();
            Object.DontDestroyOnLoad(pathMeshGameObject);
            var pathMeshFilter = pathMeshGameObject.AddComponent<MeshFilter>();
            pathMeshFilter.sharedMesh = new Mesh();
            var pathMeshRenderer = pathMeshGameObject.AddComponent<MeshRenderer>();
            pathMeshRenderer.sharedMaterial = "MovePath".LoadAs<Material>();

            var pathBuilder = new PathBuilder(unitPosition);

            var cursor = level.view.cursorView;

            if (!level.CurrentPlayer.IsAi && !game.autoplay) {
                var (texture, transform) = TileMaskTexture.Create(reachable, 8);
                TileMaskTexture.Set(level.view.terrainMaterial, "_TileMask", texture, transform);
                if (cursor)
                    cursor.show = true;
            }

            void RebuildPathMesh() {
                pathMeshFilter.sharedMesh = MoveSequenceMeshBuilder.Build(
                    pathMeshFilter.sharedMesh,
                    new MoveSequence(unit.view.transform, pathBuilder.positions),
                    nameof(MoveSequenceAtlas).LoadAs<MoveSequenceAtlas>());
            }

            unit.view.Selected = true;
            initialLookDirection = unit.view.LookDirection;

            var issuedAiCommands = false;
            while (true) {
                yield return StateChange.none;

                if (game.autoplay || Input.GetKey(KeyCode.Alpha8)) {
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
                            if (pathBuilder.positions.Last() == mousePosition)
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

                            if (cursor)
                                cursor.show = false;

                            path = pathBuilder.positions;
                            var animation = new MoveSequence(unit.view.transform, path, PersistentData.Loaded.gameSettings.unitSpeed).Animation();

                            while (animation.MoveNext()) {
                                yield return StateChange.none;

                                if (Input.GetMouseButtonDown(Mouse.left) || Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Space)) {
                                    unit.view.Position = path[^1];
                                    if (path.Count >= 2)
                                        unit.view.LookDirection = path[^1] - path[^2];
                                    break;
                                }
                            }

                            yield return StateChange.ReplaceWith(new ActionSelectionState(stateMachine));
                            break;
                        }

                        case (Command.Cancel, _):
                            unit.view.Selected = false;
                            yield return StateChange.Pop();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
        }
    }

    public override void Dispose() {
        var level = stateMachine.TryFind<PlayState>().level;
        var unit = stateMachine.TryFind<SelectionState>().unit;
        level.view.terrainMaterial.SetTexture("_TileMask", null);
        unit.view.Selected = false;
        Object.Destroy(pathMeshGameObject);
    }
}