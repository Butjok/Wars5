using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class PathSelectionState : LevelState {

	public static Traverser traverser = new();
	public Unit unit;
	public Vector2Int startForward;
	public MovePath path;
	public MeshRenderer renderer;
	public MaterialPropertyBlock propertyBlock;

	public MeshFilter movePathMeshFilter;
	public MeshRenderer movePathMeshRenderer;
	public MoveTypeAtlas moveTypeAtlas;
	public MovePathBuilder movePathBuilder;
	public Material movePathMaterial;
	public MovePath movePath;

	public GameObject go;
	public MeshFilter tileMeshFilter;
	public MeshRenderer tileMeshRenderer;
	public Material tileMeshMaterial;

	public PathSelectionState(Level level, Unit unit) : base(level) {

		this.unit = unit;
		startForward = unit.view.transform.forward.ToVector2().RoundToInt();
		
		if (unit.position.v is not { } unitPosition)
			throw new AssertionException(null, null);
		
		Assert.IsTrue(level.tiles.ContainsKey(unitPosition));

		traverser.Traverse(level.tiles.Keys, unitPosition, Cost);

		movePathMeshFilter = Sm.Runner.gameObject.AddComponent<MeshFilter>();
		movePathMeshRenderer = Sm.Runner.gameObject.AddComponent<MeshRenderer>();

		moveTypeAtlas = Resources.Load<MoveTypeAtlas>(nameof(MoveTypeAtlas));
		Assert.IsTrue(moveTypeAtlas);

		movePathMaterial = Resources.Load<Material>("MovePath");
		Assert.IsTrue(movePathMaterial);

		movePathMeshRenderer.sharedMaterial = movePathMaterial;
		movePathMeshFilter.sharedMesh = new Mesh();

		movePathBuilder = new MovePathBuilder(unitPosition);

		go = new GameObject();
		go.transform.SetParent(Sm.Runner.transform);

		tileMeshFilter = go.AddComponent<MeshFilter>();
		tileMeshFilter.sharedMesh = new Mesh();
		tileMeshRenderer = go.AddComponent<MeshRenderer>();
		tileMeshMaterial = Resources.Load<Material>("TileMesh");
		tileMeshRenderer.sharedMaterial = tileMeshMaterial;
		tileMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;

		tileMeshFilter.sharedMesh = TileMeshBuilder.Build(tileMeshFilter.sharedMesh, level, traverser);

		var terrain = GameObject.FindGameObjectWithTag("Terrain");
		if (terrain) {
			renderer = terrain.GetComponent<MeshRenderer>();
			if (renderer) {
				var positions = level.tiles.Keys.Where(p => traverser.IsReachable(p)).Select(p => (Vector4)(Vector2)p).ToList();
				propertyBlock = new MaterialPropertyBlock();
				Debug.Log(positions.Count);
				propertyBlock.SetInteger("_Size", positions.Count);
				propertyBlock.SetVectorArray("_Positions", positions);
				propertyBlock.SetVector("_From", (Vector2)unitPosition);
				propertyBlock.SetFloat("_SelectTime", Time.time);
				propertyBlock.SetFloat("_TimeDirection", 1);
				renderer.SetPropertyBlock(propertyBlock);
			}
		}
	}

	public int? Cost(Vector2Int position, int length) {
		if (length >= Rules.MoveDistance(unit) ||
		    !level.TryGetTile(position, out var tile) ||
		    level.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
			return null;

		return Rules.MoveCost(unit, tile);
	}

	public override void Update() {
		base.Update();

		if (Mouse.TryGetPosition(out Vector2Int mousePosition) &&
		    level.TryGetTile(mousePosition, out _) &&
		    traverser.IsReachable(mousePosition)) {
			
			movePathBuilder.Clear();
			foreach (var position in traverser.ReconstructPath(mousePosition).Skip(1))
				movePathBuilder.Add(position);

			movePath = movePathBuilder.GetMovePath(startForward);
			movePathMeshFilter.sharedMesh = MovePathMeshBuilder.Build(movePathMeshFilter.sharedMesh, movePath, moveTypeAtlas);
		}
		else {
			movePathBuilder.Clear();
			movePathMeshFilter.sharedMesh.Clear();
		}
		
		if (Input.GetMouseButtonDown(Mouse.right)) {
			level.State = new SelectionState(level);
			unit.view.selected.v = false;
			return;
		}
		if (Input.GetMouseButtonDown(Mouse.left)) {

			if (Mouse.TryGetPosition(out Vector2Int position) && traverser.IsReachable(position)) {
				var positions = traverser.ReconstructPath(position);
				path = new MovePath(positions, unit.view.transform.forward.ToVector2().RoundToInt());
				level.State = new UnitMovementAnimationState(level, unit, path);
				return;
			}
			else
				Sounds.NotAllowed.Play();
		}
	}

	/*public override void DrawGizmos() {
		base.DrawGizmos();
		foreach (var position in level.tiles.Keys)
			Handles.Label(position.ToVector3Int(), traverser.GetDistance(position).ToString(), new GUIStyle { normal = new GUIStyleState { textColor = Color.black } });
	}*/

	public override void Dispose() {
		base.Dispose();
		if (renderer) {
			propertyBlock.SetFloat("_SelectTime", Time.time + .1f);
			propertyBlock.SetFloat("_TimeDirection", -1);
			renderer.SetPropertyBlock(propertyBlock);
		}
		Object.Destroy(movePathMeshFilter);
		Object.Destroy(movePathMeshRenderer);

		Object.Destroy(go);
	}
}