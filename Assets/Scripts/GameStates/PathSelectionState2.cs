using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PathSelectionState2 : IDisposableState {

    public const string prefix = "path-selection-state.";

    public const string cancel = prefix + "cancel";
    public const string move = prefix + "move";
    public const string reconstructPath = prefix + "reconstruct-path";
    public const string appendToPath = prefix + "append-to-path";

    public Level level;
    public Unit unit;
    public PathSelectionState2(Level level, Unit unit) {
        this.level = level;
        this.unit = unit;
    }

    public static GameObject pathMeshGameObject;
    public static MeshFilter pathMeshFilter;
    public static MeshRenderer pathMeshRenderer;

    public IEnumerator<StateChange> Run {
        get {
            Assert.IsTrue(level.tiles.ContainsKey(unit.NonNullPosition));

            if (!pathMeshGameObject) {
                pathMeshGameObject = new GameObject();
                Object.DontDestroyOnLoad(pathMeshGameObject);
                pathMeshFilter = pathMeshGameObject.AddComponent<MeshFilter>();
                pathMeshRenderer = pathMeshGameObject.AddComponent<MeshRenderer>();
                pathMeshRenderer.sharedMaterial = "MovePath".LoadAs<Material>();
            }

            var pathBuilder = new PathBuilder(unit.NonNullPosition);
            
            CursorView.TryFind(out var cursorView);
            if (!level.CurrentPlayer.IsAi && !level.autoplay && level.tileAreaMeshFilter) {
                var moveFinder = new MoveFinder2();
                moveFinder.FindMoves(unit);
                level.tileAreaMeshFilter.sharedMesh = TileAreaMeshBuilder.Build(moveFinder.movePositions);
                if (cursorView)
                    cursorView.show = true;
            }
            
            yield break;
        }
    }

    public void Dispose() {
        if (pathMeshFilter.sharedMesh) {
            Object.Destroy(pathMeshFilter.sharedMesh);
            pathMeshFilter.sharedMesh = null;
        }
        if (level.tileAreaMeshFilter)
            level.tileAreaMeshFilter.sharedMesh = null;
    }
}