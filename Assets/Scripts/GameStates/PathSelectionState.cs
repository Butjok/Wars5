using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class PathSelectionState {

    public const string prefix = "path-selection-state.";

    public const string cancel = prefix + "cancel";
    public const string move = prefix + "move";
    public const string reconstructPath = prefix + "reconstruct-path";
    public const string appendToPath = prefix + "append-to-path";

    public static GameObject pathMeshGameObject;
    public static MeshFilter pathMeshFilter;
    public static MeshRenderer pathMeshRenderer;
    public static MoveSequenceAtlas moveSequenceAtlas;

    public static IEnumerator<StateChange> Run(Level level, Unit unit) {

        if (unit.Position is not { } unitPosition)
            throw new AssertionException("unit.position.v != null", "");
        Assert.IsTrue(level.tiles.ContainsKey(unitPosition));

        var moveDistance = Rules.MoveCapacity(unit);

        var traverser = new Traverser();
        traverser.Traverse(level.tiles.Keys, unitPosition, Rules.GetMoveCostFunction(unit));

        if (!pathMeshGameObject) {
            pathMeshGameObject = new GameObject();
            Object.DontDestroyOnLoad(pathMeshGameObject);
            pathMeshFilter = pathMeshGameObject.AddComponent<MeshFilter>();
            pathMeshFilter.sharedMesh = new Mesh();
            pathMeshRenderer = pathMeshGameObject.AddComponent<MeshRenderer>();
            pathMeshRenderer.sharedMaterial = "MovePath".LoadAs<Material>();
        }

        var pathBuilder = new PathBuilder(unitPosition);

        CursorView.TryFind(out var cursor);

        //var tileAreaMeshBuilder = Object.FindObjectOfType<TileAreaMeshuild>()
        if (!level.CurrentPlayer.IsAi && !level.autoplay && level.tileAreaMeshFilter) {
            level.tileAreaMeshFilter.sharedMesh = TileAreaMeshBuilder.Build(traverser.Reachable);
            if (cursor)
                cursor.show = true;
        }

        void CleanUp() {
            pathMeshFilter.sharedMesh = null;
            if (level.tileAreaMeshFilter)
                level.tileAreaMeshFilter.sharedMesh = null;
        }
        void RebuildPathMesh() {
            pathMeshFilter.sharedMesh = MoveSequenceMeshBuilder.Build(
                pathMeshFilter.sharedMesh,
                new MoveSequence(unit.view.transform, pathBuilder.positions),
                nameof(MoveSequenceAtlas).LoadAs<MoveSequenceAtlas>());
        }

        var issuedAiCommands = false;
        while (true) {
            yield return StateChange.none;

            if (level.autoplay || Input.GetKey(KeyCode.Alpha8)) {
                if (!issuedAiCommands) {
                    issuedAiCommands = true;
                    level.IssueAiCommandsForPathSelectionState();
                }
            }
            else if (!level.CurrentPlayer.IsAi) {
                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    level.commands.Enqueue(cancel);

                else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {

                    if (Mouse.TryGetPosition(out Vector2Int mousePosition) && traverser.IsReachable(mousePosition, moveDistance)) {
                        if (pathBuilder.positions.Last() == mousePosition) {
                            var isLastUnmovedUnit = level.FindUnitsOf(level.CurrentPlayer).Count(u => !u.Moved) == 1;
                            level.stack.Push(level.followLastUnit && level.CurrentPlayer != level.localPlayer &&
                                            isLastUnmovedUnit && pathBuilder.positions.Count > 1);
                            level.commands.Enqueue(move);
                        }
                        else {
                            level.stack.Push(mousePosition);
                            level.commands.Enqueue(reconstructPath);
                        }
                    }
                    else
                        UiSound.Instance.notAllowed.PlayOneShot();
                }
            }

            while (level.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {

                        case reconstructPath: {
                            var targetPosition = level.stack.Pop<Vector2Int>();
                            List<Vector2Int> path = null;
                            if (traverser.TryReconstructPath(targetPosition, ref path)) {
                                pathBuilder.Clear();
                                foreach (var position in path.Skip(1))
                                    pathBuilder.Add(position);
                                RebuildPathMesh();
                            }
                            break;
                        }

                        case appendToPath:
                            pathBuilder.Add(level.stack.Pop<Vector2Int>());
                            RebuildPathMesh();
                            break;

                        case move: {

                            var followUnitMove = level.stack.Pop<bool>();

                            CleanUp();
                            if (cursor)
                                cursor.show = false;

                            var initialLookDirection = unit.view.LookDirection;
                            var path = pathBuilder.positions;
                            var animation = new MoveSequence(unit.view.transform, path, PersistentData.Loaded.gameSettings.unitSpeed).Animation();

                            CameraRig.TryFind(out var cameraRig);
                            HardFollow hardFollow = null;
                            if (followUnitMove && cameraRig)
                                hardFollow = cameraRig.GetComponent<HardFollow>();

                            if (hardFollow) {
                                hardFollow.enabled = true;
                                // hardFollow.target = unit.view;
                            }

                            while (animation.MoveNext()) {
                                yield return StateChange.none;

                                if (Input.GetMouseButtonDown(Mouse.left) || Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Space)) {
                                    unit.view.Position = path[^1];
                                    if (path.Count >= 2)
                                        unit.view.LookDirection = path[^1] - path[^2];
                                    break;
                                }
                            }

                            if (hardFollow)
                                hardFollow.enabled = false;

                            yield return StateChange.ReplaceWith(new ActionSelectionState(level, unit, path, initialLookDirection));
                            break;
                        }

                        case cancel:
                            unit.view.Selected = false;
                            CleanUp();
                            yield return StateChange.ReplaceWith(new SelectionState2(level));
                            break;

                        default:
                            level.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}