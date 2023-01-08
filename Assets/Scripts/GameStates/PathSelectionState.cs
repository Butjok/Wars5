using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class PathSelectionState {

    public const string prefix = "path-selection-state.";

    public const string cancel = prefix + "cancel";
    public const string move = prefix + "move";
    public const string reconstructPath = prefix + "reconstruct-path";
    public const string appendToPath = prefix + "append-to-path";

    public static IEnumerator Run(Main main, Unit unit) {

        int? Cost(Vector2Int position, int length) {
            if (length >= Rules.MoveDistance(unit) ||
                !main.TryGetTile(position, out var tile) ||
                main.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
                return null;

            return Rules.MoveCost(unit, tile);
        }

        Assert.IsTrue(unit.position.v != null, "unit.position.v != null");
        var unitPosition = (Vector2Int)unit.position.v;
        Assert.IsTrue(main.tiles.ContainsKey(unitPosition));

        var moveDistance = Rules.MoveDistance(unit);

        var traverser = new Traverser();
        traverser.Traverse(main.tiles.Keys, unitPosition, Cost, moveDistance);

        var pathMeshGameObject = new GameObject();
        Object.DontDestroyOnLoad(pathMeshGameObject);

        var tileMeshGameObject = new GameObject();
        Object.DontDestroyOnLoad(tileMeshGameObject);

        var pathMeshFilter = pathMeshGameObject.AddComponent<MeshFilter>();
        var pathMeshRenderer = pathMeshGameObject.AddComponent<MeshRenderer>();

        var moveTypeAtlas = Resources.Load<MoveSequenceAtlas>(nameof(MoveSequenceAtlas));
        Assert.IsTrue(moveTypeAtlas);

        var pathMaterial = Resources.Load<Material>("MovePath");
        Assert.IsTrue(pathMaterial);

        pathMeshRenderer.sharedMaterial = pathMaterial;
        pathMeshFilter.sharedMesh = new Mesh();

        var pathBuilder = new PathBuilder(unitPosition);

        var tileMeshFilter = tileMeshGameObject.AddComponent<MeshFilter>();
        tileMeshFilter.sharedMesh = new Mesh();
        var tileMeshRenderer = tileMeshGameObject.AddComponent<MeshRenderer>();
        var tileMeshMaterial = Resources.Load<Material>("TileMesh");
        tileMeshRenderer.sharedMaterial = tileMeshMaterial;
        tileMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;

        tileMeshFilter.sharedMesh = TileMeshBuilder.Build(
            tileMeshFilter.sharedMesh,
            main.tiles.Keys.Where(position => traverser.IsReachable(position, moveDistance)));

        var oldPositions = new List<Vector2Int> { unitPosition };

        void CleanUp() {
            Object.Destroy(pathMeshGameObject);
            Object.Destroy(tileMeshGameObject);
        }

        CursorView.TryFind(out var cursor);
        if (cursor)
            cursor.Visible = true;

        while (true) {
            yield return null;

            if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                main.commands.Enqueue(cancel);

            else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {

                if (Mouse.TryGetPosition(out Vector2Int mousePosition) && traverser.IsReachable(mousePosition, moveDistance)) {
                    if (pathBuilder.positions.Last() == mousePosition)
                        main.commands.Enqueue(move);
                    else {
                        main.stack.Push(mousePosition);
                        main.commands.Enqueue(reconstructPath);
                    }
                }
                else
                    UiSound.Instance.notAllowed.PlayOneShot();
            }

            while (main.commands.TryDequeue(out var input))
                foreach (var token in input.Tokenize())
                    switch (token) {

                        case reconstructPath: {
                            var targetPosition = main.stack.Pop<Vector2Int>();
                            var positions = traverser.ReconstructPath(targetPosition)?.Skip(1);
                            if (positions != null) {
                                pathBuilder.Clear();
                                foreach (var position in positions)
                                    pathBuilder.Add(position);
                            }
                            break;
                        }

                        case appendToPath:
                            pathBuilder.Add(main.stack.Pop<Vector2Int>());
                            break;

                        case move:
                            CleanUp();
                            if (cursor)
                                cursor.Visible = false;

                            var initialLookDirection = unit.view.LookDirection;
                            var animation = new MoveSequence(unit.view.transform, pathBuilder.positions, main.settings.unitSpeed).Animation();

                            while (animation.MoveNext()) {
                                yield return null;

                                if (Input.GetMouseButtonDown(Mouse.left) || Input.GetMouseButtonDown(Mouse.right) ||
                                    Input.GetKeyDown(KeyCode.Space)) {
                                    unit.view.Position = pathBuilder.positions[^1];
                                    if (pathBuilder.positions.Count >= 2)
                                        unit.view.LookDirection = pathBuilder.positions[^1] - pathBuilder.positions[^2];
                                    break;
                                }
                            }

                            yield return ActionSelectionState.Run(main, unit, pathBuilder.positions, initialLookDirection);
                            yield break;

                        case cancel:
                            unit.view.Selected = false;
                            CleanUp();
                            yield return SelectionState.Run(main);
                            yield break;

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