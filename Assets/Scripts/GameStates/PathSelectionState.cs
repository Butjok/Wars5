using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class PathSelectionState {

    public static IEnumerator New(Game game, Unit unit) {
        
        int? cost(Vector2Int position, int length) {
            if (length >= Rules.MoveDistance(unit) ||
                !game.TryGetTile(position, out var tile) ||
                game.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
                return null;

            return Rules.MoveCost(unit, tile);
        }
        
        Assert.IsTrue(unit.position.v != null, "unit.position.v != null");
        var unitPosition = (Vector2Int)unit.position.v;
        Assert.IsTrue(game.tiles.ContainsKey(unitPosition));
        
        var traverser = new Traverser();
        traverser.Traverse(game.tiles.Keys, unitPosition, cost);

        var startForward = unit.view.transform.forward.ToVector2().RoundToInt();

        var pathMeshGameObject = new GameObject();
        Object.DontDestroyOnLoad(pathMeshGameObject);

        var tileMeshGameObject = new GameObject();
        Object.DontDestroyOnLoad(tileMeshGameObject);

        var pathMeshFilter = pathMeshGameObject.AddComponent<MeshFilter>();
        var pathMeshRenderer = pathMeshGameObject.AddComponent<MeshRenderer>();

        var moveTypeAtlas = Resources.Load<MoveTypeAtlas>(nameof(MoveTypeAtlas));
        Assert.IsTrue(moveTypeAtlas);

        var pathMaterial = Resources.Load<Material>("MovePath");
        Assert.IsTrue(pathMaterial);

        pathMeshRenderer.sharedMaterial = pathMaterial;
        pathMeshFilter.sharedMesh = new Mesh();

        var pathBuilder = new MovePathBuilder(unitPosition);

        var tileMeshFilter = tileMeshGameObject.AddComponent<MeshFilter>();
        tileMeshFilter.sharedMesh = new Mesh();
        var tileMeshRenderer = tileMeshGameObject.AddComponent<MeshRenderer>();
        var tileMeshMaterial = Resources.Load<Material>("TileMesh");
        tileMeshRenderer.sharedMaterial = tileMeshMaterial;
        tileMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;

        tileMeshFilter.sharedMesh = TileMeshBuilder.Build(tileMeshFilter.sharedMesh, game.tiles.Keys.Where(traverser.IsReachable));

        var oldPositions = new List<Vector2Int> { unitPosition };

        void cleanUp() {
            Object.Destroy(pathMeshGameObject);
            Object.Destroy(tileMeshGameObject);
        }

        CursorView.Instance.Visible = true;

        while (true) {

            if (game.input.reconstructPathTo is { } targetPosition) {
                game.input.reconstructPathTo = null;

                var positions = traverser.ReconstructPath(targetPosition)?.Skip(1);
                if (positions != null) {
                    pathBuilder.Clear();
                    foreach (var position in positions)
                        pathBuilder.Add(position);
                }
            }
            else
                while (game.input.appendToPath.Count > 0)
                    pathBuilder.Add(game.input.appendToPath.Dequeue());
            
            if (game.input.moveUnit) {
                game.input.moveUnit = false;
                cleanUp();
                CursorView.Instance.Visible = false;
                yield return UnitMovementAnimationState.New(game, unit, new MovePath(pathBuilder.Positions, startForward));
                yield break;
            }

            else if (game.input.cancel) {
                game.input.cancel = false;
                unit.view.Selected = false;
                cleanUp();
                yield return SelectionState.New(game);
                yield break;
            }

            if (!oldPositions.SequenceEqual(pathBuilder.Positions)) {
                oldPositions.Clear();
                oldPositions.AddRange(pathBuilder.Positions);
                var path = new MovePath(pathBuilder.Positions, startForward);
                pathMeshFilter.sharedMesh = MovePathMeshBuilder.Build(pathMeshFilter.sharedMesh, path, moveTypeAtlas);
            }

            if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape)) {

                unit.view.Selected = false;
                game.input.Reset();

                cleanUp();
                yield return SelectionState.New(game);
                yield break;
            }

            else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {

                if (Mouse.TryGetPosition(out Vector2Int mousePosition) && traverser.IsReachable(mousePosition)) {
                    if (pathBuilder.Positions.Last() == mousePosition)
                        game.input.moveUnit = true;
                    else {
                        pathBuilder.Clear();
                        foreach (var position in traverser.ReconstructPath(mousePosition).Skip(1))
                            pathBuilder.Add(position);
                    }
                }
                else
                    UiSound.Instance.notAllowed.PlayOneShot();
            }
            
            yield return null;
        }
    }
}