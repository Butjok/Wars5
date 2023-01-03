using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class PathSelectionState {

    public static IEnumerator New(Level level, Unit unit) {
        
        int? cost(Vector2Int position, int length) {
            if (length >= Rules.MoveDistance(unit) ||
                !level.TryGetTile(position, out var tile) ||
                level.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
                return null;

            return Rules.MoveCost(unit, tile);
        }
        
        Assert.IsTrue(unit.position.v != null, "unit.position.v != null");
        var unitPosition = (Vector2Int)unit.position.v;
        Assert.IsTrue(level.tiles.ContainsKey(unitPosition));

        var moveDistance = Rules.MoveDistance(unit);
        
        var traverser = new Traverser();
        traverser.Traverse(level.tiles.Keys, unitPosition, cost, moveDistance);

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

        tileMeshFilter.sharedMesh = TileMeshBuilder.Build(
            tileMeshFilter.sharedMesh, 
            level.tiles.Keys.Where(position => traverser.IsReachable(position,moveDistance)));

        var oldPositions = new List<Vector2Int> { unitPosition };

        void cleanUp() {
            Object.Destroy(pathMeshGameObject);
            Object.Destroy(tileMeshGameObject);
        }

        CursorView.Instance.Visible = true;

        while (true) {
            
            yield return null;

            if (level.input.reconstructPathTo is { } targetPosition) {
                level.input.reconstructPathTo = null;

                var positions = traverser.ReconstructPath(targetPosition)?.Skip(1);
                if (positions != null) {
                    pathBuilder.Clear();
                    foreach (var position in positions)
                        pathBuilder.Add(position);
                }
            }
            else
                while (level.input.appendToPath.Count > 0)
                    pathBuilder.Add(level.input.appendToPath.Dequeue());
            
            if (level.input.moveUnit) {
                level.input.moveUnit = false;
                cleanUp();
                CursorView.Instance.Visible = false;
                yield return UnitMovementAnimationState.New(level, unit, new MovePath(pathBuilder.Positions, startForward));
                yield break;
            }

            else if (level.input.cancel) {
                level.input.cancel = false;
                unit.view.Selected = false;
                cleanUp();
                yield return SelectionState.New(level);
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
                level.input.Reset();

                cleanUp();
                yield return SelectionState.New(level);
                yield break;
            }

            else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {

                if (Mouse.TryGetPosition(out Vector2Int mousePosition) && traverser.IsReachable(mousePosition, moveDistance)) {
                    if (pathBuilder.Positions.Last() == mousePosition)
                        level.input.moveUnit = true;
                    else {
                        pathBuilder.Clear();
                        foreach (var position in traverser.ReconstructPath(mousePosition).Skip(1))
                            pathBuilder.Add(position);
                    }
                }
                else
                    UiSound.Instance.notAllowed.PlayOneShot();
            }
        }
    }
}