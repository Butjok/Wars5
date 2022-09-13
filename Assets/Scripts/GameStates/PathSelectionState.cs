using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class PathSelectionState {

	public static Traverser traverser = new();

	public static IEnumerator New(Game2 game, Unit unit) {

		int? cost(Vector2Int position, int length) {
			if (length >= Rules.MoveDistance(unit) ||
			    !game.TryGetTile(position, out var tile) ||
			    game.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
				return null;

			return Rules.MoveCost(unit, tile);
		}

		var startForward = unit.view.transform.forward.ToVector2().RoundToInt();

		Assert.IsTrue(unit.position.v != null, "unit.position.v != null");
		var unitPosition = (Vector2Int)unit.position.v;
		Assert.IsTrue(game.tiles.ContainsKey(unitPosition));

		traverser.Traverse(game.tiles.Keys, unitPosition, cost);

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

		var movePathBuilder = new MovePathBuilder(unitPosition);

		var tileMeshFilter = tileMeshGameObject.AddComponent<MeshFilter>();
		tileMeshFilter.sharedMesh = new Mesh();
		var tileMeshRenderer = tileMeshGameObject.AddComponent<MeshRenderer>();
		var tileMeshMaterial = Resources.Load<Material>("TileMesh");
		tileMeshRenderer.sharedMaterial = tileMeshMaterial;
		tileMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;

		tileMeshFilter.sharedMesh = TileMeshBuilder.Build(tileMeshFilter.sharedMesh, game, traverser);

		void cleanUp() {
			Object.Destroy(movePathGameObject);
			Object.Destroy(tileMeshGameObject);
		}
		
		CursorView.Instance.Visible = true;

		while (true) {
			yield return null;

			if (Mouse.TryGetPosition(out Vector2Int mousePosition) &&
			    game.TryGetTile(mousePosition, out _) &&
			    traverser.IsReachable(mousePosition)) {

				movePathBuilder.Clear();
				foreach (var position in traverser.ReconstructPath(mousePosition).Skip(1))
					movePathBuilder.Add(position);

				var movePath = movePathBuilder.GetMovePath(startForward);
				movePathMeshFilter.sharedMesh = MovePathMeshBuilder.Build(movePathMeshFilter.sharedMesh, movePath, moveTypeAtlas);
			}
			else {
				movePathBuilder.Clear();
				movePathMeshFilter.sharedMesh.Clear();
			}

			if (Input.GetMouseButtonDown(Mouse.right)) {
				
				unit.view.Selected = false;
				
				cleanUp();
				yield return SelectionState.New(game);
				yield break;
			}

			if (Input.GetMouseButtonDown(Mouse.left)) {

				if (Mouse.TryGetPosition(out Vector2Int position) && traverser.IsReachable(position)) {

					var positions = traverser.ReconstructPath(position);
					var path = new MovePath(positions, unit.view.transform.forward.ToVector2().RoundToInt());

					cleanUp();
					
					CursorView.Instance.Visible = false;
					yield return UnitMovementAnimationState.New(game, unit, path);
					yield break;
				}

				UiSound.Instance.notAllowed.Play();
			}
		}
	}
}