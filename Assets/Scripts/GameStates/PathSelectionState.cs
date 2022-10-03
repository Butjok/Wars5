using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class PathSelectionState {

    public static IEnumerator New(Game2 game) {

        if (game.input.path != null) {
            yield return UnitMovementAnimationState.New(game);
            yield break;
        }

        var unit = game.input.Unit;
        Assert.IsTrue(unit != null);

        var startForward = unit.view.transform.forward.ToVector2().RoundToInt();

        Assert.IsTrue(unit.position.v != null, "unit.position.v != null");
        var unitPosition = (Vector2Int)unit.position.v;
        Assert.IsTrue(game.tiles.ContainsKey(unitPosition));

        var movePathGameObject = new GameObject();
        Object.DontDestroyOnLoad(movePathGameObject);

        var tileMeshGameObject = new GameObject();
        Object.DontDestroyOnLoad(tileMeshGameObject);

        var movePathMeshFilter = movePathGameObject.AddComponent<MeshFilter>();
        var movePathMeshRenderer = movePathGameObject.AddComponent<MeshRenderer>();

        var moveTypeAtlas = Resources.Load<MoveTypeAtlas>(nameof(MoveTypeAtlas));
        Assert.IsTrue(moveTypeAtlas);

        var movePathMaterial = Resources.Load<Material>("MovePath");
        Assert.IsTrue(movePathMaterial);

        movePathMeshRenderer.sharedMaterial = movePathMaterial;
        movePathMeshFilter.sharedMesh = new Mesh();

        var movePathBuilder = game.input.pathBuilder;

        var tileMeshFilter = tileMeshGameObject.AddComponent<MeshFilter>();
        tileMeshFilter.sharedMesh = new Mesh();
        var tileMeshRenderer = tileMeshGameObject.AddComponent<MeshRenderer>();
        var tileMeshMaterial = Resources.Load<Material>("TileMesh");
        tileMeshRenderer.sharedMaterial = tileMeshMaterial;
        tileMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;

        var traverser = game.input.traverser;
        tileMeshFilter.sharedMesh = TileMeshBuilder.Build(tileMeshFilter.sharedMesh, game.tiles.Keys.Where(traverser.IsReachable));

        var oldPositions = new List<Vector2Int>{unitPosition};

        void cleanUp() {
            Object.Destroy(movePathGameObject);
            Object.Destroy(tileMeshGameObject);
        }

        CursorView.Instance.Visible = true;

        while (true) {
            yield return null;

            // path is selected
            if (game.input.path != null) {
                cleanUp();
                CursorView.Instance.Visible = false;
                yield return UnitMovementAnimationState.New(game);
                yield break;
            }

            // unit is deselected
            else if (game.input.Unit == null) {
                unit.view.Selected = false;
                cleanUp();
                yield return SelectionState.New(game);
                yield break;
            }

            if (!oldPositions.SequenceEqual(movePathBuilder.Positions)) {
                oldPositions.Clear();
                oldPositions.AddRange(movePathBuilder.Positions);
                var movePath = new MovePath(movePathBuilder.Positions, startForward);
                movePathMeshFilter.sharedMesh = MovePathMeshBuilder.Build(movePathMeshFilter.sharedMesh, movePath, moveTypeAtlas);
            }

            if (game.CurrentPlayer.IsAi)
                continue;

            if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape)) {

                unit.view.Selected = false;
                game.input.Reset();

                cleanUp();
                yield return SelectionState.New(game);
                yield break;
            }

            else if (Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {

                if (Mouse.TryGetPosition(out Vector2Int mousePosition) && traverser.IsReachable(mousePosition)) {
                    if (movePathBuilder.Positions.Last()==mousePosition) 
                        game.input.path = new MovePath(movePathBuilder.Positions, startForward);
                    else {
                        movePathBuilder.Clear();
                        foreach (var position in traverser.ReconstructPath(mousePosition).Skip(1))
                            movePathBuilder.Add(position);
                    }
                }
                else
                    UiSound.Instance.notAllowed.Play();
            }
        }
    }
}