using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class PathSelectionState : State2<Game2> {

    public static Traverser traverser = new();
    public Unit unit;
    public Vector2Int startForward;
    public MovePath path;
    public MeshRenderer terrainMeshRenderer;
    public MaterialPropertyBlock propertyBlock;

    public MeshFilter movePathMeshFilter;
    public MeshRenderer movePathMeshRenderer;
    public MoveTypeAtlas moveTypeAtlas;
    public MovePathBuilder movePathBuilder;
    public Material movePathMaterial;
    public MovePath movePath;

    public GameObject movePathGameObject, tileMeshGameObject;
    public MeshFilter tileMeshFilter;
    public MeshRenderer tileMeshRenderer;
    public Material tileMeshMaterial;

    public PathSelectionState(Game2 parent, Unit unit) : base(parent) {

        this.unit = unit;
        startForward = unit.view.transform.forward.ToVector2().RoundToInt();

        if (unit.position.v is not { } unitPosition)
            throw new AssertionException(null, null);

        Assert.IsTrue(parent.tiles.ContainsKey(unitPosition));

        traverser.Traverse(parent.tiles.Keys, unitPosition, Cost);

        movePathGameObject = new GameObject();
        Object.DontDestroyOnLoad(movePathGameObject);

        tileMeshGameObject = new GameObject();
        Object.DontDestroyOnLoad(tileMeshGameObject);
        
        movePathMeshFilter = movePathGameObject.AddComponent<MeshFilter>();
        movePathMeshRenderer = movePathGameObject.AddComponent<MeshRenderer>();

        moveTypeAtlas = Resources.Load<MoveTypeAtlas>(nameof(MoveTypeAtlas));
        Assert.IsTrue(moveTypeAtlas);

        movePathMaterial = Resources.Load<Material>("MovePath");
        Assert.IsTrue(movePathMaterial);

        movePathMeshRenderer.sharedMaterial = movePathMaterial;
        movePathMeshFilter.sharedMesh = new Mesh();

        movePathBuilder = new MovePathBuilder(unitPosition);

        tileMeshFilter = tileMeshGameObject.AddComponent<MeshFilter>();
        tileMeshFilter.sharedMesh = new Mesh();
        tileMeshRenderer = tileMeshGameObject.AddComponent<MeshRenderer>();
        tileMeshMaterial = Resources.Load<Material>("TileMesh");
        tileMeshRenderer.sharedMaterial = tileMeshMaterial;
        tileMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;

        tileMeshFilter.sharedMesh = TileMeshBuilder.Build(tileMeshFilter.sharedMesh, parent, traverser);

        var terrain = GameObject.FindGameObjectWithTag("Terrain");
        if (terrain) {
            terrainMeshRenderer = terrain.GetComponent<MeshRenderer>();
            if (terrainMeshRenderer) {
                var positions = parent.tiles.Keys.Where(p => traverser.IsReachable(p)).Select(p => (Vector4)(Vector2)p).ToList();
                propertyBlock = new MaterialPropertyBlock();
                Debug.Log(positions.Count);
                propertyBlock.SetInteger("_Size", positions.Count);
                propertyBlock.SetVectorArray("_Positions", positions);
                propertyBlock.SetVector("_From", (Vector2)unitPosition);
                propertyBlock.SetFloat("_SelectTime", Time.time);
                propertyBlock.SetFloat("_TimeDirection", 1);
                terrainMeshRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }

    public int? Cost(Vector2Int position, int length) {
        if (length >= Rules.MoveDistance(unit) ||
            !parent.TryGetTile(position, out var tile) ||
            parent.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
            return null;

        return Rules.MoveCost(unit, tile);
    }

    public override void Update() {
        base.Update();

        if (Mouse.TryGetPosition(out Vector2Int mousePosition) &&
            parent.TryGetTile(mousePosition, out _) &&
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
            ChangeTo(new SelectionState(parent));
            unit.view.selected.v = false;
            return;
        }
        if (Input.GetMouseButtonDown(Mouse.left)) {

            if (Mouse.TryGetPosition(out Vector2Int position) && traverser.IsReachable(position)) {
                var positions = traverser.ReconstructPath(position);
                path = new MovePath(positions, unit.view.transform.forward.ToVector2().RoundToInt());
                ChangeTo(new UnitMovementAnimationState(parent, unit, path));
                return;
            }
            else
                UiSound.Instance.notAllowed.Play();
        }
    }

    /*public override void DrawGizmos() {
        base.DrawGizmos();
        foreach (var position in level.tiles.Keys)
            Handles.Label(position.ToVector3Int(), traverser.GetDistance(position).ToString(), new GUIStyle { normal = new GUIStyleState { textColor = Color.black } });
    }*/

    public override void Dispose() {
        base.Dispose();
        if (terrainMeshRenderer) {
            propertyBlock.SetFloat("_SelectTime", Time.time + .1f);
            propertyBlock.SetFloat("_TimeDirection", -1);
            terrainMeshRenderer.SetPropertyBlock(propertyBlock);
        }
        Object.Destroy(movePathGameObject);
        Object.Destroy(tileMeshGameObject);
    }
}