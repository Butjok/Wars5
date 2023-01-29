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

    public static IEnumerator<StateChange> Run(Main main, Unit unit) {

        if (unit.Position is not { } unitPosition)
            throw new AssertionException("unit.position.v != null", "");
        Assert.IsTrue(main.tiles.ContainsKey(unitPosition));

        var moveDistance = Rules.MoveDistance(unit);

        var traverser = new Traverser();
        traverser.Traverse(main.tiles.Keys, unitPosition, Rules.GetMoveCostFunction(unit));

        var pathMeshGameObject = new GameObject();
        Object.DontDestroyOnLoad(pathMeshGameObject);


        var pathMeshFilter = pathMeshGameObject.AddComponent<MeshFilter>();
        var pathMeshRenderer = pathMeshGameObject.AddComponent<MeshRenderer>();

        var moveTypeAtlas = Resources.Load<MoveSequenceAtlas>(nameof(MoveSequenceAtlas));
        Assert.IsTrue(moveTypeAtlas);

        var pathMaterial = Resources.Load<Material>("MovePath");
        Assert.IsTrue(pathMaterial);

        pathMeshRenderer.sharedMaterial = pathMaterial;
        pathMeshFilter.sharedMesh = new Mesh();

        var pathBuilder = new PathBuilder(unitPosition);

        //var tileAreaMeshBuilder = Object.FindObjectOfType<TileAreaMeshuild>()
        if (main.tileAreaMeshFilter)
            main.tileAreaMeshFilter.sharedMesh = TileAreaMeshBuilder.Build(traverser.Reachable);

        var oldPositions = new List<Vector2Int> { unitPosition };

        void CleanUp() {
            Object.Destroy(pathMeshGameObject);
            if (main.tileAreaMeshFilter)
                main.tileAreaMeshFilter.sharedMesh = null;
        }

        CursorView.TryFind(out var cursor);
        if (cursor)
            cursor.show = true;

        while (true) {
            yield return StateChange.none;

            if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                main.commands.Enqueue(cancel);

            else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {

                if (Mouse.TryGetPosition(out Vector2Int mousePosition) && traverser.IsReachable(mousePosition, moveDistance)) {
                    if (pathBuilder.positions.Last() == mousePosition) {
                        var isLastUnmovedUnit = main.FindUnitsOf(main.CurrentPlayer).Count(u => !u.Moved) == 1;
                        main.stack.Push(main.followLastUnit && main.CurrentPlayer != main.localPlayer &&
                                        isLastUnmovedUnit && pathBuilder.positions.Count > 1);
                        main.commands.Enqueue(move);
                    }
                    else {
                        main.stack.Push(mousePosition);
                        main.commands.Enqueue(reconstructPath);
                    }
                }
                else
                    UiSound.Instance.notAllowed.PlayOneShot();
            }

            while (main.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {

                        case reconstructPath: {
                            var targetPosition = main.stack.Pop<Vector2Int>();
                            var path = new List<Vector2Int>();
                            if (traverser.TryReconstructPath(targetPosition, path)) {
                                pathBuilder.Clear();
                                foreach (var position in path.Skip(1))
                                    pathBuilder.Add(position);
                            }
                            break;
                        }

                        case appendToPath:
                            pathBuilder.Add(main.stack.Pop<Vector2Int>());
                            break;

                        case move: {

                            var followUnitMove = main.stack.Pop<bool>();

                            CleanUp();
                            if (cursor)
                                cursor.show = false;

                            var initialLookDirection = unit.view.LookDirection;
                            var path = pathBuilder.positions;
                            var animation = new MoveSequence(unit.view.transform, path, main.persistentData.gameSettings.unitSpeed).Animation();

                            CameraRig.TryFind(out var cameraRig);
                            HardFollow hardFollow = null;
                            if (followUnitMove && cameraRig)
                                hardFollow = cameraRig.GetComponent<HardFollow>();

                            string cameraRigSettings = null;
                            if (cameraRig) {
                                using var sw = new StringWriter();
                                GameWriter.WriteCameraRig(sw, cameraRig);
                                cameraRigSettings = sw.ToString();
                            }
                            if (hardFollow) {
                                hardFollow.enabled = true;
                                hardFollow.target = unit.view;
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

                            yield return StateChange.ReplaceWith("action-selection", ActionSelectionState.Run(main, unit, path, initialLookDirection));
                            break;
                        }

                        case cancel:
                            unit.view.Selected = false;
                            CleanUp();
                            yield return StateChange.ReplaceWith("selection", SelectionState.Run(main));
                            break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }

            if (!oldPositions.SequenceEqual(pathBuilder.positions)) {
                oldPositions.Clear();
                oldPositions.AddRange(pathBuilder.positions);
                pathMeshFilter.sharedMesh = MoveSequenceMeshBuilder.Build(
                    pathMeshFilter.sharedMesh,
                    new MoveSequence(unit.view.transform, pathBuilder.positions),
                    moveTypeAtlas);
            }
        }
    }
}